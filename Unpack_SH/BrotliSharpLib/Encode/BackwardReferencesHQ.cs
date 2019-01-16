using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        [StructLayout(LayoutKind.Sequential)]
        private struct ZopfliNode {
            public struct u_ {
                private uint v;

                /* Smallest cost to get to this byte from the beginning, as found so far. */
                public unsafe float cost {
                    get {
                        uint vv = v;
                        return *(float*)(&vv);
                    }
                    set { v = *(uint*) &value; }
                }

                /* Offset to the next node on the path. Equals to command_length() of the
                   next node on the path. For last node equals to BROTLI_UINT32_MAX */
                public uint next {
                    get { return v; }
                    set { v = value; }
                }

                /* Node position that provides next distance for distance cache. */
                public uint shortcut {
                    get { return v; }
                    set { v = value; }
                }
            }

            /* best length to get up to this byte (not including this byte itself)
               highest 8 bit is used to reconstruct the length code */
            public uint length;

            /* distance associated with the length
               highest 7 bit contains distance short code + 1 (or zero if no short code)
            */
            public uint distance;

            /* number of literal inserts before this copy */
            public uint insert_length;

            /* This union holds information used by dynamic-programming. During forward
               pass |cost| it used to store the goal function. When node is processed its
               |cost| is invalidated in favor of |shortcut|. On path back-tracing pass
               |next| is assigned the offset to next node on the path. */
            public u_ u;
        }

        /* Histogram based cost model for zopflification. */
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct ZopfliCostModel {
            /* The insert and copy length symbols. */
            public fixed float cost_cmd_[BROTLI_NUM_COMMAND_SYMBOLS];

            public fixed float cost_dist_[BROTLI_NUM_DISTANCE_SYMBOLS];

            /* Cumulative costs of literals per position in the stream. */
            public float* literal_costs_;

            public float min_cost_cmd_;
            public size_t num_bytes_;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct PosData {
            public size_t pos;
            public fixed int distance_cache[4];
            public float costdiff;
            public float cost;
        }

        /* Maintains the smallest 8 cost difference together with their positions */
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct StartPosQueue {
            private PosData q0, q1, q2, q3, q4, q5, q6, q7;
            public size_t idx_;

            public PosData* q_ {
                get {
                    fixed (PosData* q = &q0)
                        return q;
                }
            }
        }

        private static unsafe void InitStartPosQueue(StartPosQueue* self) {
            self->idx_ = 0;
        }

        private static unsafe void BrotliInitZopfliNodes(ZopfliNode* array, size_t length) {
            ZopfliNode stub = new ZopfliNode();
            size_t i;
            stub.length = 1;
            stub.distance = 0;
            stub.insert_length = 0;
            stub.u.cost = kInfinity;
            for (i = 0; i < length; ++i) array[i] = stub;
        }

        private static unsafe void CleanupZopfliCostModel(ref MemoryManager m, ZopfliCostModel* self) {
            BrotliFree(ref m, self->literal_costs_);
        }

        private static unsafe uint ZopfliNodeCopyLength(ZopfliNode* self) {
            return self->length & 0xffffff;
        }

        private static unsafe uint ZopfliNodeLengthCode(ZopfliNode* self) {
            uint modifier = self->length >> 24;
            return ZopfliNodeCopyLength(self) + 9u - modifier;
        }

        private static unsafe uint ZopfliNodeCopyDistance(ZopfliNode* self) {
            return self->distance & 0x1ffffff;
        }

        private static unsafe uint ZopfliNodeDistanceCode(ZopfliNode* self) {
            uint short_code = self->distance >> 25;
            return short_code == 0
                ? ZopfliNodeCopyDistance(self) + BROTLI_NUM_DISTANCE_SHORT_CODES - 1
                : short_code - 1;
        }

        private static unsafe uint ZopfliNodeCommandLength(ZopfliNode* self) {
            return ZopfliNodeCopyLength(self) + self->insert_length;
        }

        private static unsafe void InitZopfliCostModel(
            ref MemoryManager m, ZopfliCostModel* self, size_t num_bytes) {
            self->num_bytes_ = num_bytes;
            self->literal_costs_ = (float*) BrotliAllocate(ref m, (num_bytes + 2) * sizeof(float));
        }

        private static unsafe void ZopfliCostModelSetFromLiteralCosts(ZopfliCostModel* self,
            size_t position,
            byte* ringbuffer,
            size_t ringbuffer_mask) {
            float* literal_costs = self->literal_costs_;
            float* cost_dist = self->cost_dist_;
            float* cost_cmd = self->cost_cmd_;
            size_t num_bytes = self->num_bytes_;
            size_t i;
            BrotliEstimateBitCostsForLiterals(position, num_bytes, ringbuffer_mask,
                ringbuffer, &literal_costs[1]);
            literal_costs[0] = 0.0f;
            for (i = 0; i < num_bytes; ++i) {
                literal_costs[i + 1] += literal_costs[i];
            }
            for (i = 0; i < BROTLI_NUM_COMMAND_SYMBOLS; ++i) {
                cost_cmd[i] = (float) FastLog2(11 + (uint) i);
            }
            for (i = 0; i < BROTLI_NUM_DISTANCE_SYMBOLS; ++i) {
                cost_dist[i] = (float) FastLog2(20 + (uint) i);
            }
            self->min_cost_cmd_ = (float) FastLog2(11);
        }

        private static unsafe float ZopfliCostModelGetCommandCost(
            ZopfliCostModel* self, ushort cmdcode) {
            return self->cost_cmd_[cmdcode];
        }

        private static unsafe float ZopfliCostModelGetDistanceCost(
            ZopfliCostModel* self, size_t distcode) {
            return self->cost_dist_[distcode];
        }

        private static unsafe float ZopfliCostModelGetLiteralCosts(
            ZopfliCostModel* self, size_t from, size_t to) {
            return self->literal_costs_[to] - self->literal_costs_[from];
        }

        private static unsafe float ZopfliCostModelGetMinCostCmd(
            ZopfliCostModel* self) {
            return self->min_cost_cmd_;
        }

        /* Fills in dist_cache[0..3] with the last four distances (as defined by
           Section 4. of the Spec) that would be used at (block_start + pos) if we
           used the shortest path of commands from block_start, computed from
           nodes[0..pos]. The last four distances at block_start are in
           starting_dist_cache[0..3].
           REQUIRES: nodes[pos].cost < kInfinity
           REQUIRES: nodes[0..pos] satisfies that "ZopfliNode array invariant". */
        private static unsafe void ComputeDistanceCache(size_t pos,
            int* starting_dist_cache,
            ZopfliNode* nodes,
            int* dist_cache) {
            int idx = 0;
            size_t p = nodes[pos].u.shortcut;
            while (idx < 4 && p > 0) {
                size_t ilen = nodes[p].insert_length;
                size_t clen = ZopfliNodeCopyLength(&nodes[p]);
                size_t dist = ZopfliNodeCopyDistance(&nodes[p]);
                dist_cache[idx++] = (int) dist;
                /* Because of prerequisite, p >= clen + ilen >= 2. */
                p = nodes[p - clen - ilen].u.shortcut;
            }
            for (; idx < 4; ++idx) {
                dist_cache[idx] = *starting_dist_cache++;
            }
        }

        /* REQUIRES: nodes[pos].cost < kInfinity
   REQUIRES: nodes[0..pos] satisfies that "ZopfliNode array invariant". */
        private static unsafe uint ComputeDistanceShortcut(size_t block_start,
            size_t pos,
            size_t max_backward,
            ZopfliNode* nodes) {
            size_t clen = ZopfliNodeCopyLength(&nodes[pos]);
            size_t ilen = nodes[pos].insert_length;
            size_t dist = ZopfliNodeCopyDistance(&nodes[pos]);
            /* Since |block_start + pos| is the end position of the command, the copy part
               starts from |block_start + pos - clen|. Distances that are greater than
               this or greater than |max_backward| are private static unsafe dictionary references, and
               do not update the last distances. Also distance code 0 (last distance)
               does not update the last distances. */
            if (pos == 0) {
                return 0;
            }
            else if (dist + clen <= block_start + pos &&
                     dist <= max_backward &&
                     ZopfliNodeDistanceCode(&nodes[pos]) > 0) {
                return (uint) pos;
            }
            else {
                return nodes[pos - clen - ilen].u.shortcut;
            }
        }

        private static unsafe size_t StartPosQueueSize(StartPosQueue* self) {
            return Math.Min(self->idx_, 8);
        }

        private static unsafe void StartPosQueuePush(StartPosQueue* self, PosData* posdata) {
            size_t offset = ~(self->idx_++) & 7;
            size_t len = StartPosQueueSize(self);
            size_t i;
            PosData* q = self->q_;
            q[offset] = *posdata;
            /* Restore the sorted order. In the list of |len| items at most |len - 1|
               adjacent element comparisons / swaps are required. */
            for (i = 1; i < len; ++i) {
                if (q[offset & 7].costdiff > q[(offset + 1) & 7].costdiff) {
                    PosData tmp = q[offset & 7];
                    q[offset & 7] = q[(offset + 1) & 7];
                    q[(offset + 1) & 7] = tmp;
                }
                ++offset;
            }
        }

        /* Maintains "ZopfliNode array invariant" and pushes node to the queue, if it
           is eligible. */
        private static unsafe void EvaluateNode(
            size_t block_start, size_t pos, size_t max_backward_limit,
            int* starting_dist_cache, ZopfliCostModel* model,
            StartPosQueue* queue, ZopfliNode* nodes) {
            /* Save cost, because ComputeDistanceCache invalidates it. */
            float node_cost = nodes[pos].u.cost;
            nodes[pos].u.shortcut = ComputeDistanceShortcut(
                block_start, pos, max_backward_limit, nodes);
            if (node_cost <= ZopfliCostModelGetLiteralCosts(model, 0, pos)) {
                PosData posdata;
                posdata.pos = pos;
                posdata.cost = node_cost;
                posdata.costdiff = node_cost -
                                   ZopfliCostModelGetLiteralCosts(model, 0, pos);
                ComputeDistanceCache(
                    pos, starting_dist_cache, nodes, posdata.distance_cache);
                StartPosQueuePush(queue, &posdata);
            }
        }

        private static unsafe PosData* StartPosQueueAt(StartPosQueue* self, size_t k) {
            return &self->q_[(k - self->idx_) & 7];
        }

        /* Returns the minimum possible copy length that can improve the cost of any */
        /* future position. */
        private static unsafe size_t ComputeMinimumCopyLength(float start_cost,
            ZopfliNode* nodes,
            size_t num_bytes,
            size_t pos) {
            /* Compute the minimum possible cost of reaching any future position. */
            float min_cost = start_cost;
            size_t len = 2;
            size_t next_len_bucket = 4;
            size_t next_len_offset = 10;
            while (pos + len <= num_bytes && nodes[pos + len].u.cost <= min_cost) {
                /* We already reached (pos + len) with no more cost than the minimum
                   possible cost of reaching anything from this pos, so there is no point in
                   looking for lengths <= len. */
                ++len;
                if (len == next_len_offset) {
                    /* We reached the next copy length code bucket, so we add one more
                       extra bit to the minimum cost. */
                    min_cost += 1.0f;
                    next_len_offset += next_len_bucket;
                    next_len_bucket *= 2;
                }
            }
            return len;
        }

        /* REQUIRES: len >= 2, start_pos <= pos */
        /* REQUIRES: cost < kInfinity, nodes[start_pos].cost < kInfinity */
        /* Maintains the "ZopfliNode array invariant". */
        private static unsafe void UpdateZopfliNode(ZopfliNode* nodes, size_t pos,
            size_t start_pos, size_t len, size_t len_code, size_t dist,
            size_t short_code, float cost) {
            ZopfliNode* next = &nodes[pos + len];
            next->length = (uint) (len | ((len + 9u - len_code) << 24));
            next->distance = (uint) (dist | (short_code << 25));
            next->insert_length = (uint) (pos - start_pos);
            next->u.cost = cost;
        }

        /* Returns longest copy length. */
        private static unsafe size_t UpdateNodes(
            size_t num_bytes, size_t block_start, size_t pos,
            byte* ringbuffer, size_t ringbuffer_mask,
            BrotliEncoderParams* params_, size_t max_backward_limit,
            int* starting_dist_cache, size_t num_matches,
            BackwardMatch* matches, ZopfliCostModel* model,
            StartPosQueue* queue, ZopfliNode* nodes) {
            size_t cur_ix = block_start + pos;
            size_t cur_ix_masked = cur_ix & ringbuffer_mask;
            size_t max_distance = Math.Min(cur_ix, max_backward_limit);
            size_t max_len = num_bytes - pos;
            size_t max_zopfli_len = MaxZopfliLen(params_);
            size_t max_iters = MaxZopfliCandidates(params_);
            size_t min_len;
            size_t result = 0;
            size_t k;

            EvaluateNode(block_start, pos, max_backward_limit, starting_dist_cache, model,
                queue, nodes);

            {
                PosData* posdata = StartPosQueueAt(queue, 0);
                float min_cost = (posdata->cost + ZopfliCostModelGetMinCostCmd(model) +
                                  ZopfliCostModelGetLiteralCosts(model, posdata->pos, pos));
                min_len = ComputeMinimumCopyLength(min_cost, nodes, num_bytes, pos);
            }

            /* Go over the command starting positions in order of increasing cost
               difference. */
            for (k = 0; k < max_iters && k < StartPosQueueSize(queue); ++k) {
                PosData* posdata = StartPosQueueAt(queue, k);
                size_t start = posdata->pos;
                ushort inscode = GetInsertLengthCode(pos - start);
                float start_costdiff = posdata->costdiff;
                float base_cost = start_costdiff + (float) GetInsertExtra(inscode) +
                                  ZopfliCostModelGetLiteralCosts(model, 0, pos);

                /* Look for last distance matches using the distance cache from this
                   starting position. */
                size_t best_len = min_len - 1;
                size_t j = 0;
                for (; j < BROTLI_NUM_DISTANCE_SHORT_CODES && best_len < max_len; ++j) {
                    size_t idx = kDistanceCacheIndex[j];
                    size_t backward =
                        (size_t) (posdata->distance_cache[idx] + kDistanceCacheOffset[j]);
                    size_t prev_ix = cur_ix - backward;
                    if (prev_ix >= cur_ix) {
                        continue;
                    }
                    if ((backward > max_distance)) {
                        continue;
                    }
                    prev_ix &= ringbuffer_mask;

                    if (cur_ix_masked + best_len > ringbuffer_mask ||
                        prev_ix + best_len > ringbuffer_mask ||
                        ringbuffer[cur_ix_masked + best_len] !=
                        ringbuffer[prev_ix + best_len]) {
                        continue;
                    }
                    {
                        size_t len =
                            FindMatchLengthWithLimit(&ringbuffer[prev_ix],
                                &ringbuffer[cur_ix_masked],
                                max_len);
                        float dist_cost = base_cost +
                                          ZopfliCostModelGetDistanceCost(model, j);
                        size_t l;
                        for (l = best_len + 1; l <= len; ++l) {
                            ushort copycode = GetCopyLengthCode(l);
                            ushort cmdcode =
                                CombineLengthCodes(inscode, copycode, j == 0);
                            float cost = (cmdcode < 128 ? base_cost : dist_cost) +
                                         (float) GetCopyExtra(copycode) +
                                         ZopfliCostModelGetCommandCost(model, cmdcode);
                            if (cost < nodes[pos + l].u.cost) {
                                UpdateZopfliNode(nodes, pos, start, l, l, backward, j + 1, cost);
                                result = Math.Max(result, l);
                            }
                            best_len = l;
                        }
                    }
                }

                /* At higher iterations look only for new last distance matches, since
                   looking only for new command start positions with the same distances
                   does not help much. */
                if (k >= 2) continue;

                {
                    /* Loop through all possible copy lengths at this position. */
                    size_t len = min_len;
                    for (j = 0; j < num_matches; ++j) {
                        BackwardMatch match = matches[j];
                        size_t dist = match.distance;
                        bool is_dictionary_match = (dist > max_distance);
                        /* We already tried all possible last distance matches, so we can use
                           normal distance code here. */
                        size_t dist_code = dist + BROTLI_NUM_DISTANCE_SHORT_CODES - 1;
                        ushort dist_symbol;
                        uint distextra;
                        uint distnumextra;
                        float dist_cost;
                        size_t max_match_len;
                        PrefixEncodeCopyDistance(dist_code, 0, 0, &dist_symbol, &distextra);
                        distnumextra = distextra >> 24;
                        dist_cost = base_cost + (float) distnumextra +
                                    ZopfliCostModelGetDistanceCost(model, dist_symbol);

                        /* Try all copy lengths up until the maximum copy length corresponding
                           to this distance. If the distance refers to the private static unsafe dictionary, or
                           the maximum length is long enough, try only one maximum length. */
                        max_match_len = BackwardMatchLength(&match);
                        if (len < max_match_len &&
                            (is_dictionary_match || max_match_len > max_zopfli_len)) {
                            len = max_match_len;
                        }
                        for (; len <= max_match_len; ++len) {
                            size_t len_code =
                                is_dictionary_match ? BackwardMatchLengthCode(&match) : len;
                            ushort copycode = GetCopyLengthCode(len_code);
                            ushort cmdcode = CombineLengthCodes(inscode, copycode, false);
                            float cost = dist_cost + (float) GetCopyExtra(copycode) +
                                         ZopfliCostModelGetCommandCost(model, cmdcode);
                            if (cost < nodes[pos + len].u.cost) {
                                UpdateZopfliNode(nodes, pos, start, len, len_code, dist, 0, cost);
                                result = Math.Max(result, len);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static unsafe size_t ComputeShortestPathFromNodes(size_t num_bytes,
            ZopfliNode* nodes) {
            size_t index = num_bytes;
            size_t num_commands = 0;
            while (nodes[index].insert_length == 0 && nodes[index].length == 1) --index;
            nodes[index].u.next = uint.MaxValue;
            while (index != 0) {
                size_t len = ZopfliNodeCommandLength(&nodes[index]);
                index -= len;
                nodes[index].u.next = (uint) len;
                num_commands++;
            }
            return num_commands;
        }

        /* REQUIRES: nodes != NULL and len(nodes) >= num_bytes + 1 */
        private static unsafe size_t BrotliZopfliComputeShortestPath(ref MemoryManager m,
            size_t num_bytes,
            size_t position,
            byte* ringbuffer,
            size_t ringbuffer_mask,
            BrotliEncoderParams* params_,
            size_t max_backward_limit,
            int* dist_cache,
            HasherHandle hasher,
            ZopfliNode* nodes) {
            size_t max_zopfli_len = MaxZopfliLen(params_);
            ZopfliCostModel model;
            StartPosQueue queue;
            BackwardMatch* matches = stackalloc BackwardMatch[MAX_NUM_MATCHES_H10];
            HashToBinaryTreeH10 h10 = (HashToBinaryTreeH10) kHashers[10];
            size_t store_end = num_bytes >= h10.StoreLookahead()
                ? position + num_bytes - h10.StoreLookahead() + 1
                : position;
            size_t i;
            nodes[0].length = 0;
            nodes[0].u.cost = 0;
            InitZopfliCostModel(ref m, &model, num_bytes);
            ZopfliCostModelSetFromLiteralCosts(
                &model, position, ringbuffer, ringbuffer_mask);
            InitStartPosQueue(&queue);
            for (i = 0; i + h10.HashTypeLength() - 1 < num_bytes; i++) {
                size_t pos = position + i;
                size_t max_distance = Math.Min(pos, max_backward_limit);
                size_t num_matches = HashToBinaryTreeH10.FindAllMatches(hasher, ringbuffer,
                    ringbuffer_mask, pos, num_bytes - i, max_distance, params_, matches);
                size_t skip;
                if (num_matches > 0 &&
                    BackwardMatchLength(&matches[num_matches - 1]) > max_zopfli_len) {
                    matches[0] = matches[num_matches - 1];
                    num_matches = 1;
                }
                skip = UpdateNodes(num_bytes, position, i, ringbuffer, ringbuffer_mask,
                    params_, max_backward_limit, dist_cache, num_matches, matches, &model,
                    &queue, nodes);
                if (skip < BROTLI_LONG_COPY_QUICK_STEP) skip = 0;
                if (num_matches == 1 && BackwardMatchLength(&matches[0]) > max_zopfli_len) {
                    skip = Math.Max(BackwardMatchLength(&matches[0]), skip);
                }
                if (skip > 1) {
                    /* Add the tail of the copy to the hasher. */
                    h10.StoreRange(hasher, ringbuffer, ringbuffer_mask, pos + 1, Math.Min(
                        pos + skip, store_end));
                    skip--;
                    while (skip != 0) {
                        i++;
                        if (i + h10.HashTypeLength() - 1 >= num_bytes) break;
                        EvaluateNode(
                            position, i, max_backward_limit, dist_cache, &model, &queue, nodes);
                        skip--;
                    }
                }
            }
            CleanupZopfliCostModel(ref m, &model);
            return ComputeShortestPathFromNodes(num_bytes, nodes);
        }

        /* REQUIRES: nodes != NULL and len(nodes) >= num_bytes + 1 */
        private static unsafe void BrotliZopfliCreateCommands(size_t num_bytes,
            size_t block_start,
            size_t max_backward_limit,
            ZopfliNode* nodes,
            int* dist_cache,
            size_t* last_insert_len,
            Command* commands,
            size_t* num_literals) {
            size_t pos = 0;
            uint offset = nodes[0].u.next;
            size_t i;
            for (i = 0; offset != uint.MaxValue; i++) {
                ZopfliNode* next = &nodes[pos + offset];
                size_t copy_length = ZopfliNodeCopyLength(next);
                size_t insert_length = next->insert_length;
                pos += insert_length;
                offset = next->u.next;
                if (i == 0) {
                    insert_length += *last_insert_len;
                    *last_insert_len = 0;
                }
                {
                    size_t distance = ZopfliNodeCopyDistance(next);
                    size_t len_code = ZopfliNodeLengthCode(next);
                    size_t max_distance =
                        Math.Min(block_start + pos, max_backward_limit);
                    bool is_dictionary = (distance > max_distance);
                    size_t dist_code = ZopfliNodeDistanceCode(next);

                    InitCommand(
                        &commands[i], insert_length, copy_length, len_code, dist_code);

                    if (!is_dictionary && dist_code > 0) {
                        dist_cache[3] = dist_cache[2];
                        dist_cache[2] = dist_cache[1];
                        dist_cache[1] = dist_cache[0];
                        dist_cache[0] = (int) distance;
                    }
                }

                *num_literals += insert_length;
                pos += copy_length;
            }
            *last_insert_len += num_bytes - pos;
        }

        private static unsafe void BrotliCreateZopfliBackwardReferences(
            ref MemoryManager m, size_t num_bytes,
            size_t position, byte* ringbuffer, size_t ringbuffer_mask,
            BrotliEncoderParams* params_, HasherHandle hasher, int* dist_cache,
            size_t* last_insert_len, Command* commands, size_t* num_commands,
            size_t* num_literals) {
            size_t max_backward_limit = BROTLI_MAX_BACKWARD_LIMIT(params_->lgwin);
            ZopfliNode* nodes;
            nodes = (ZopfliNode*) BrotliAllocate(ref m, (num_bytes + 1) * sizeof(ZopfliNode));
            BrotliInitZopfliNodes(nodes, num_bytes + 1);
            *num_commands += BrotliZopfliComputeShortestPath(ref m, num_bytes,
                position, ringbuffer, ringbuffer_mask, params_, max_backward_limit,
                dist_cache, hasher, nodes);
            BrotliZopfliCreateCommands(num_bytes, position, max_backward_limit, nodes,
                dist_cache, last_insert_len, commands, num_literals);
            BrotliFree(ref m, nodes);
        }

        private static unsafe void SetCost(uint* histogram, size_t histogram_size,
            float* cost) {
            size_t sum = 0;
            float log2sum;
            size_t i;
            for (i = 0; i < histogram_size; i++) {
                sum += histogram[i];
            }
            log2sum = (float) FastLog2(sum);
            for (i = 0; i < histogram_size; i++) {
                if (histogram[i] == 0) {
                    cost[i] = log2sum + 2;
                    continue;
                }

                /* Shannon bits for this symbol. */
                cost[i] = log2sum - (float) FastLog2(histogram[i]);

                /* Cannot be coded with less than 1 bit */
                if (cost[i] < 1) cost[i] = 1;
            }
        }

        private static unsafe void ZopfliCostModelSetFromCommands(ZopfliCostModel* self,
            size_t position,
            byte* ringbuffer,
            size_t ringbuffer_mask,
            Command* commands,
            size_t num_commands,
            size_t last_insert_len) {
            uint* histogram_literal = stackalloc uint[BROTLI_NUM_LITERAL_SYMBOLS];
            uint* histogram_cmd = stackalloc uint[BROTLI_NUM_COMMAND_SYMBOLS];
            uint* histogram_dist = stackalloc uint[BROTLI_NUM_DISTANCE_SYMBOLS];
            float* cost_literal = stackalloc float[BROTLI_NUM_LITERAL_SYMBOLS];
            size_t pos = position - last_insert_len;
            float min_cost_cmd = kInfinity;
            size_t i;
            float* cost_cmd = self->cost_cmd_;

            memset(histogram_literal, 0, BROTLI_NUM_LITERAL_SYMBOLS * sizeof(uint));
            memset(histogram_cmd, 0, BROTLI_NUM_COMMAND_SYMBOLS * sizeof(uint));
            memset(histogram_dist, 0, BROTLI_NUM_DISTANCE_SYMBOLS * sizeof(uint));

            for (i = 0; i < num_commands; i++) {
                size_t inslength = commands[i].insert_len_;
                size_t copylength = CommandCopyLen(&commands[i]);
                size_t distcode = commands[i].dist_prefix_;
                size_t cmdcode = commands[i].cmd_prefix_;
                size_t j;

                histogram_cmd[cmdcode]++;
                if (cmdcode >= 128) histogram_dist[distcode]++;

                for (j = 0; j < inslength; j++) {
                    histogram_literal[ringbuffer[(pos + j) & ringbuffer_mask]]++;
                }

                pos += inslength + copylength;
            }

            SetCost(histogram_literal, BROTLI_NUM_LITERAL_SYMBOLS, cost_literal);
            SetCost(histogram_cmd, BROTLI_NUM_COMMAND_SYMBOLS, cost_cmd);
            SetCost(histogram_dist, BROTLI_NUM_DISTANCE_SYMBOLS, self->cost_dist_);

            for (i = 0; i < BROTLI_NUM_COMMAND_SYMBOLS; ++i) {
                min_cost_cmd = Math.Min(min_cost_cmd, cost_cmd[i]);
            }
            self->min_cost_cmd_ = min_cost_cmd;

            {
                float* literal_costs = self->literal_costs_;
                size_t num_bytes = self->num_bytes_;
                literal_costs[0] = 0.0f;
                for (i = 0; i < num_bytes; ++i) {
                    literal_costs[i + 1] = literal_costs[i] +
                                           cost_literal[ringbuffer[(position + i) & ringbuffer_mask]];
                }
            }
        }

        private static unsafe size_t ZopfliIterate(size_t num_bytes,
            size_t position,
            byte* ringbuffer,
            size_t ringbuffer_mask,
            BrotliEncoderParams* params_,
            size_t max_backward_limit,
            int* dist_cache,
            ZopfliCostModel* model,
            uint* num_matches,
            BackwardMatch* matches,
            ZopfliNode* nodes) {
            size_t max_zopfli_len = MaxZopfliLen(params_);
            StartPosQueue queue;
            size_t cur_match_pos = 0;
            size_t i;
            nodes[0].length = 0;
            nodes[0].u.cost = 0;
            InitStartPosQueue(&queue);
            for (i = 0; i + 3 < num_bytes; i++) {
                size_t skip = UpdateNodes(num_bytes, position, i, ringbuffer,
                    ringbuffer_mask, params_, max_backward_limit, dist_cache,
                    num_matches[i], &matches[cur_match_pos], model, &queue, nodes);
                if (skip < BROTLI_LONG_COPY_QUICK_STEP) skip = 0;
                cur_match_pos += num_matches[i];
                if (num_matches[i] == 1 &&
                    BackwardMatchLength(&matches[cur_match_pos - 1]) > max_zopfli_len) {
                    skip = Math.Max(
                        BackwardMatchLength(&matches[cur_match_pos - 1]), skip);
                }
                if (skip > 1) {
                    skip--;
                    while (skip != 0) {
                        i++;
                        if (i + 3 >= num_bytes) break;
                        EvaluateNode(
                            position, i, max_backward_limit, dist_cache, model, &queue, nodes);
                        cur_match_pos += num_matches[i];
                        skip--;
                    }
                }
            }
            return ComputeShortestPathFromNodes(num_bytes, nodes);
        }

        private static unsafe void BrotliCreateHqZopfliBackwardReferences(
            ref MemoryManager m, size_t num_bytes,
            size_t position, byte* ringbuffer, size_t ringbuffer_mask,
            BrotliEncoderParams* params_, HasherHandle hasher, int* dist_cache,
            size_t* last_insert_len, Command* commands, size_t* num_commands,
            size_t* num_literals) {
            size_t max_backward_limit = BROTLI_MAX_BACKWARD_LIMIT(params_->lgwin);
            uint* num_matches = (uint*) BrotliAllocate(ref m, num_bytes * sizeof(uint));
            size_t matches_size = 4 * num_bytes;
            Hasher h = kHashers[10];
            size_t store_end = num_bytes >= h.StoreLookahead()
                ? position + num_bytes - h.StoreLookahead() + 1
                : position;
            size_t cur_match_pos = 0;
            size_t i;
            size_t orig_num_literals;
            size_t orig_last_insert_len;
            int* orig_dist_cache = stackalloc int[4];
            size_t orig_num_commands;
            ZopfliCostModel model;
            ZopfliNode* nodes;
            BackwardMatch* matches = (BackwardMatch*) BrotliAllocate(ref m, matches_size * sizeof(BackwardMatch));
            for (i = 0; i + h.HashTypeLength() - 1 < num_bytes; ++i) {
                size_t pos = position + i;
                size_t max_distance = Math.Min(pos, max_backward_limit);
                size_t max_length = num_bytes - i;
                size_t num_found_matches;
                size_t cur_match_end;
                /* Ensure that we have enough free slots. */
                BrotliEnsureCapacity(ref m, sizeof(BackwardMatch), (void**) &matches, &matches_size, cur_match_pos + MAX_NUM_MATCHES_H10);
                num_found_matches = HashToBinaryTreeH10.FindAllMatches(hasher, ringbuffer,
                    ringbuffer_mask, pos, max_length, max_distance, params_,
                    &matches[cur_match_pos]);
                cur_match_end = cur_match_pos + num_found_matches;
                num_matches[i] = (uint) num_found_matches;
                if (num_found_matches > 0) {
                    size_t match_len = BackwardMatchLength(&matches[cur_match_end - 1]);
                    if (match_len > MAX_ZOPFLI_LEN_QUALITY_11) {
                        size_t skip = match_len - 1;
                        matches[cur_match_pos++] = matches[cur_match_end - 1];
                        num_matches[i] = 1;
                        /* Add the tail of the copy to the hasher. */
                        h.StoreRange(hasher, ringbuffer, ringbuffer_mask, pos + 1,
                            Math.Min(pos + match_len, store_end));
                        memset(&num_matches[i + 1], 0, skip * sizeof(uint));
                        i += skip;
                    }
                    else {
                        cur_match_pos = cur_match_end;
                    }
                }
            }
            orig_num_literals = *num_literals;
            orig_last_insert_len = *last_insert_len;
            memcpy(orig_dist_cache, dist_cache, 4 * sizeof(int));
            orig_num_commands = *num_commands;
            nodes = (ZopfliNode*) BrotliAllocate(ref m, (num_bytes + 1) * sizeof(ZopfliNode));
            InitZopfliCostModel(ref m, &model, num_bytes);
            for (i = 0; i < 2; i++) {
                BrotliInitZopfliNodes(nodes, num_bytes + 1);
                if (i == 0) {
                    ZopfliCostModelSetFromLiteralCosts(
                        &model, position, ringbuffer, ringbuffer_mask);
                }
                else {
                    ZopfliCostModelSetFromCommands(&model, position, ringbuffer,
                        ringbuffer_mask, commands, *num_commands - orig_num_commands,
                        orig_last_insert_len);
                }
                *num_commands = orig_num_commands;
                *num_literals = orig_num_literals;
                *last_insert_len = orig_last_insert_len;
                memcpy(dist_cache, orig_dist_cache, 4 * sizeof(int));
                *num_commands += ZopfliIterate(num_bytes, position, ringbuffer,
                    ringbuffer_mask, params_, max_backward_limit, dist_cache,
                    &model, num_matches, matches, nodes);
                BrotliZopfliCreateCommands(num_bytes, position, max_backward_limit,
                    nodes, dist_cache, last_insert_len, commands, num_literals);
            }
            CleanupZopfliCostModel(ref m, &model);
            BrotliFree(ref m, nodes);
            BrotliFree(ref m, matches);
            BrotliFree(ref m, num_matches);
        }
    }
}