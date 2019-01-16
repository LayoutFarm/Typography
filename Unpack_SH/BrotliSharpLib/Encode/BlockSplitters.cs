using System;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private static readonly size_t kMaxLiteralHistograms = 100;
        private static readonly size_t kMaxCommandHistograms = 50;
        private const double kLiteralBlockSwitchCost = 28.1;
        private const double kCommandBlockSwitchCost = 13.5;
        private const double kDistanceBlockSwitchCost = 14.6;
        private static readonly size_t kLiteralStrideLength = 70;
        private static readonly size_t kCommandStrideLength = 40;
        private static readonly size_t kSymbolsPerLiteralHistogram = 544;
        private static readonly size_t kSymbolsPerCommandHistogram = 530;
        private static readonly size_t kSymbolsPerDistanceHistogram = 544;
        private static readonly size_t kMinLengthForBlockSplitting = 128;
        private static readonly size_t kIterMulForRefining = 2;
        private static readonly size_t kMinItersForRefining = 100;

        private unsafe partial struct BlockSplitterLiteral {
            private static void InitialEntropyCodes(byte* data, size_t length,
                size_t stride,
                size_t num_histograms,
                HistogramLiteral* histograms) {
                uint seed = 7;
                size_t block_length = length / num_histograms;
                size_t i;
                HistogramLiteral.ClearHistograms(histograms, num_histograms);
                for (i = 0; i < num_histograms; ++i) {
                    size_t pos = length * i / num_histograms;
                    if (i != 0) {
                        pos += MyRand(&seed) % block_length;
                    }
                    if (pos + stride >= length) {
                        pos = length - stride - 1;
                    }
                    HistogramLiteral.HistogramAddVector(&histograms[i], data + pos, stride);
                }
            }

            private static unsafe void RandomSample(uint* seed,
                byte* data,
                size_t length,
                size_t stride,
                HistogramLiteral* sample) {
                size_t pos = 0;
                if (stride >= length) {
                    pos = 0;
                    stride = length;
                }
                else {
                    pos = MyRand(seed) % (length - stride + 1);
                }
                HistogramLiteral.HistogramAddVector(sample, data + pos, stride);
            }

            private static unsafe void RefineEntropyCodes(byte* data, size_t length,
                size_t stride,
                size_t num_histograms,
                HistogramLiteral* histograms) {
                size_t iters =
                    kIterMulForRefining * length / stride + kMinItersForRefining;
                uint seed = 7;
                size_t iter;
                iters = ((iters + num_histograms - 1) / num_histograms) * num_histograms;
                for (iter = 0; iter < iters; ++iter) {
                    HistogramLiteral sample;
                    HistogramLiteral.HistogramClear(&sample);
                    RandomSample(&seed, data, length, stride, &sample);
                    HistogramLiteral.HistogramAddHistogram(&histograms[iter % num_histograms], &sample);
                }
            }

            /* Assigns a block id from the range [0, num_histograms) to each data element
               in data[0..length) and fills in block_id[0..length) with the assigned values.
               Returns the number of blocks, i.e. one plus the number of block switches. */
            private static unsafe size_t FindBlocks(byte* data, size_t length,
                double block_switch_bitcost,
                size_t num_histograms,
                HistogramLiteral* histograms,
                double* insert_cost,
                double* cost,
                byte* switch_signal,
                byte* block_id) {
                size_t data_size = HistogramLiteral.HistogramDataSize();
                size_t bitmaplen = (num_histograms + 7) >> 3;
                size_t num_blocks = 1;
                size_t i;
                size_t j;
                if (num_histograms <= 1) {
                    for (i = 0; i < length; ++i) {
                        block_id[i] = 0;
                    }
                    return 1;
                }
                memset(insert_cost, 0, sizeof(double) * data_size * num_histograms);
                for (i = 0; i < num_histograms; ++i) {
                    insert_cost[i] = FastLog2((uint) histograms[i].total_count_);
                }
                for (i = data_size; i != 0;) {
                    --i;
                    for (j = 0; j < num_histograms; ++j) {
                        insert_cost[i * num_histograms + j] =
                            insert_cost[j] - BitCost(histograms[j].data_[i]);
                    }
                }
                memset(cost, 0, sizeof(double) * num_histograms);
                memset(switch_signal, 0, sizeof(byte) * length * bitmaplen);
                /* After each iteration of this loop, cost[k] will contain the difference
                   between the minimum cost of arriving at the current byte position using
                   entropy code k, and the minimum cost of arriving at the current byte
                   position. This difference is capped at the block switch cost, and if it
                   reaches block switch cost, it means that when we trace back from the last
                   position, we need to switch here. */
                for (i = 0; i < length; ++i) {
                    size_t byte_ix = i;
                    size_t ix = byte_ix * bitmaplen;
                    size_t insert_cost_ix = data[byte_ix] * num_histograms;
                    double min_cost = 1e99;
                    double block_switch_cost = block_switch_bitcost;
                    size_t k;
                    for (k = 0; k < num_histograms; ++k) {
                        /* We are coding the symbol in data[byte_ix] with entropy code k. */
                        cost[k] += insert_cost[insert_cost_ix + k];
                        if (cost[k] < min_cost) {
                            min_cost = cost[k];
                            block_id[byte_ix] = (byte) k;
                        }
                    }
                    /* More blocks for the beginning. */
                    if (byte_ix < 2000) {
                        block_switch_cost *= 0.77 + 0.07 * (double) byte_ix / 2000;
                    }
                    for (k = 0; k < num_histograms; ++k) {
                        cost[k] -= min_cost;
                        if (cost[k] >= block_switch_cost) {
                            byte mask = (byte) (1u << (int) (k & 7));
                            cost[k] = block_switch_cost;
                            switch_signal[ix + (k >> 3)] |= mask;
                        }
                    }
                }
                {
                    /* Trace back from the last position and switch at the marked places. */
                    size_t byte_ix = length - 1;
                    size_t ix = byte_ix * bitmaplen;
                    byte cur_id = block_id[byte_ix];
                    while (byte_ix > 0) {
                        byte mask = (byte) (1u << (cur_id & 7));
                        --byte_ix;
                        ix -= bitmaplen;
                        if ((switch_signal[ix + (cur_id >> 3)] & mask) != 0) {
                            if (cur_id != block_id[byte_ix]) {
                                cur_id = block_id[byte_ix];
                                ++num_blocks;
                            }
                        }
                        block_id[byte_ix] = cur_id;
                    }
                }
                return num_blocks;
            }

            private static size_t RemapBlockIds(byte* block_ids, size_t length,
                ushort* new_id, size_t num_histograms) {
                const ushort kInvalidId = 256;
                ushort next_id = 0;
                size_t i;
                for (i = 0; i < num_histograms; ++i) {
                    new_id[i] = kInvalidId;
                }
                for (i = 0; i < length; ++i) {
                    if (new_id[block_ids[i]] == kInvalidId) {
                        new_id[block_ids[i]] = next_id++;
                    }
                }
                for (i = 0; i < length; ++i) {
                    block_ids[i] = (byte) new_id[block_ids[i]];
                }
                return next_id;
            }

            private static unsafe void BuildBlockHistograms(byte* data, size_t length,
                byte* block_ids,
                size_t num_histograms,
                HistogramLiteral* histograms) {
                size_t i;
                HistogramLiteral.ClearHistograms(histograms, num_histograms);
                for (i = 0; i < length; ++i) {
                    HistogramLiteral.HistogramAdd(&histograms[block_ids[i]], data[i]);
                }
            }

            private static unsafe void ClusterBlocks(ref MemoryManager m,
                byte* data, size_t length,
                size_t num_blocks,
                byte* block_ids,
                BlockSplit* split) {
                uint* histogram_symbols = (uint*) BrotliAllocate(ref m, num_blocks * sizeof(uint));
                uint* block_lengths = (uint*) BrotliAllocate(ref m, num_blocks * sizeof(uint));
                size_t expected_num_clusters = CLUSTERS_PER_BATCH *
                                               (num_blocks + HISTOGRAMS_PER_BATCH - 1) / HISTOGRAMS_PER_BATCH;
                size_t all_histograms_size = 0;
                size_t all_histograms_capacity = expected_num_clusters;
                HistogramLiteral* all_histograms =
                    (HistogramLiteral*) BrotliAllocate(ref m, all_histograms_capacity * sizeof(HistogramLiteral));
                size_t cluster_size_size = 0;
                size_t cluster_size_capacity = expected_num_clusters;
                uint* cluster_size = (uint*) BrotliAllocate(ref m, cluster_size_capacity * sizeof(uint));
                size_t num_clusters = 0;
                HistogramLiteral* histograms = (HistogramLiteral*) BrotliAllocate(ref m,
                    Math.Min(num_blocks, HISTOGRAMS_PER_BATCH) * sizeof(HistogramLiteral));
                size_t max_num_pairs =
                    HISTOGRAMS_PER_BATCH * HISTOGRAMS_PER_BATCH / 2;
                size_t pairs_capacity = max_num_pairs + 1;
                HistogramPair* pairs = (HistogramPair*) BrotliAllocate(ref m, pairs_capacity * sizeof(HistogramPair));
                size_t pos = 0;
                uint* clusters;
                size_t num_final_clusters;
                const uint kInvalidIndex = uint.MaxValue;
                uint* new_index;
                size_t i;
                uint* sizes = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(sizes, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* new_clusters = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(new_clusters, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* symbols = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(symbols, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* remap = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(remap, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));


                memset(block_lengths, 0, num_blocks * sizeof(uint));

                {
                    size_t block_idx = 0;
                    for (i = 0; i < length; ++i) {
                        ++block_lengths[block_idx];
                        if (i + 1 == length || block_ids[i] != block_ids[i + 1]) {
                            ++block_idx;
                        }
                    }
                }

                for (i = 0; i < num_blocks; i += HISTOGRAMS_PER_BATCH) {
                    size_t num_to_combine =
                        Math.Min(num_blocks - i, HISTOGRAMS_PER_BATCH);
                    size_t num_new_clusters;
                    size_t j;
                    for (j = 0; j < num_to_combine; ++j) {
                        size_t k;
                        HistogramLiteral.HistogramClear(&histograms[j]);
                        for (k = 0; k < block_lengths[i + j]; ++k) {
                            HistogramLiteral.HistogramAdd(&histograms[j], data[pos++]);
                        }
                        histograms[j].bit_cost_ = BitCostLiteral.BrotliPopulationCost(&histograms[j]);
                        new_clusters[j] = (uint) j;
                        symbols[j] = (uint) j;
                        sizes[j] = 1;
                    }
                    num_new_clusters = ClusterLiteral.BrotliHistogramCombine(
                        histograms, sizes, symbols, new_clusters, pairs, num_to_combine,
                        num_to_combine, HISTOGRAMS_PER_BATCH, max_num_pairs);

                    BrotliEnsureCapacity(ref m, sizeof(HistogramLiteral), (void**)&all_histograms, &all_histograms_capacity, all_histograms_size + num_new_clusters);
                    BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&cluster_size, &cluster_size_capacity, cluster_size_size + num_new_clusters);

                    for (j = 0; j < num_new_clusters; ++j) {
                        all_histograms[all_histograms_size++] = histograms[new_clusters[j]];
                        cluster_size[cluster_size_size++] = sizes[new_clusters[j]];
                        remap[new_clusters[j]] = (uint) j;
                    }
                    for (j = 0; j < num_to_combine; ++j) {
                        histogram_symbols[i + j] = (uint) num_clusters + remap[symbols[j]];
                    }
                    num_clusters += num_new_clusters;
                }
                BrotliFree(ref m, histograms);

                max_num_pairs =
                    Math.Min(64 * num_clusters, (num_clusters / 2) * num_clusters);
                if (pairs_capacity < max_num_pairs + 1) {
                    BrotliFree(ref m, pairs);
                    pairs = (HistogramPair*) BrotliAllocate(ref m, (max_num_pairs + 1) * sizeof(HistogramPair));
                }

                clusters = (uint*) BrotliAllocate(ref m, num_clusters * sizeof(uint));

                for (i = 0; i < num_clusters; ++i) {
                    clusters[i] = (uint) i;
                }
                num_final_clusters = ClusterLiteral.BrotliHistogramCombine(
                    all_histograms, cluster_size, histogram_symbols, clusters, pairs,
                    num_clusters, num_blocks, BROTLI_MAX_NUMBER_OF_BLOCK_TYPES,
                    max_num_pairs);
                BrotliFree(ref m, pairs);
                BrotliFree(ref m, cluster_size);

                new_index = (uint*) BrotliAllocate(ref m, num_clusters * sizeof(uint));

                for (i = 0; i < num_clusters; ++i) new_index[i] = kInvalidIndex;
                pos = 0;
                {
                    uint next_index = 0;
                    for (i = 0; i < num_blocks; ++i) {
                        HistogramLiteral histo;
                        size_t j;
                        uint best_out;
                        double best_bits;
                        HistogramLiteral.HistogramClear(&histo);
                        for (j = 0; j < block_lengths[i]; ++j) {
                            HistogramLiteral.HistogramAdd(&histo, data[pos++]);
                        }
                        best_out = (i == 0) ? histogram_symbols[0] : histogram_symbols[i - 1];
                        best_bits =
                            ClusterLiteral.BrotliHistogramBitCostDistance(&histo, &all_histograms[best_out]);
                        for (j = 0; j < num_final_clusters; ++j) {
                            double cur_bits = ClusterLiteral.BrotliHistogramBitCostDistance(
                                &histo, &all_histograms[clusters[j]]);
                            if (cur_bits < best_bits) {
                                best_bits = cur_bits;
                                best_out = clusters[j];
                            }
                        }
                        histogram_symbols[i] = best_out;
                        if (new_index[best_out] == kInvalidIndex) {
                            new_index[best_out] = next_index++;
                        }
                    }
                }
                BrotliFree(ref m, clusters);
                BrotliFree(ref m, all_histograms);

                BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, num_blocks);
                BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, num_blocks);

                {
                    uint cur_length = 0;
                    size_t block_idx = 0;
                    byte max_type = 0;
                    for (i = 0; i < num_blocks; ++i) {
                        cur_length += block_lengths[i];
                        if (i + 1 == num_blocks ||
                            histogram_symbols[i] != histogram_symbols[i + 1]) {
                            byte id = (byte) new_index[histogram_symbols[i]];
                            split->types[block_idx] = id;
                            split->lengths[block_idx] = cur_length;
                            max_type = Math.Max(max_type, id);
                            cur_length = 0;
                            ++block_idx;
                        }
                    }
                    split->num_blocks = block_idx;
                    split->num_types = (size_t) max_type + 1;
                }
                BrotliFree(ref m, new_index);
                BrotliFree(ref m, block_lengths);
                BrotliFree(ref m, histogram_symbols);
            }

            public static unsafe void SplitByteVector(ref MemoryManager m,
                byte* data, size_t length,
                size_t literals_per_histogram,
                size_t max_histograms,
                size_t sampling_stride_length,
                double block_switch_cost,
                BrotliEncoderParams* params_,
                BlockSplit* split) {
                size_t data_size = HistogramLiteral.HistogramDataSize();
                size_t num_histograms = length / literals_per_histogram + 1;
                HistogramLiteral* histograms;
                if (num_histograms > max_histograms) {
                    num_histograms = max_histograms;
                }
                if (length == 0) {
                    split->num_types = 1;
                    return;
                }
                else if (length < kMinLengthForBlockSplitting) {
                    BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, split->num_blocks + 1);
                    BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, split->num_blocks + 1);

                    split->num_types = 1;
                    split->types[split->num_blocks] = 0;
                    split->lengths[split->num_blocks] = (uint) length;
                    split->num_blocks++;
                    return;
                }
                histograms = (HistogramLiteral*) BrotliAllocate(ref m, num_histograms * sizeof(HistogramLiteral));

                /* Find good entropy codes. */
                InitialEntropyCodes(data, length,
                    sampling_stride_length,
                    num_histograms, histograms);
                RefineEntropyCodes(data, length,
                    sampling_stride_length,
                    num_histograms, histograms);
                {
                    /* Find a good path through literals with the good entropy codes. */
                    byte* block_ids = (byte*) BrotliAllocate(ref m, length * sizeof(byte));
                    size_t num_blocks = 0;
                    size_t bitmaplen = (num_histograms + 7) >> 3;
                    double* insert_cost = (double*) BrotliAllocate(ref m, data_size * num_histograms * sizeof(double));
                    double* cost = (double*) BrotliAllocate(ref m, num_histograms * sizeof(double));
                    byte* switch_signal = (byte*) BrotliAllocate(ref m, length * bitmaplen * sizeof(byte));
                    ushort* new_id = (ushort*) BrotliAllocate(ref m, num_histograms * sizeof(ushort));
                    size_t iters = params_->quality < HQ_ZOPFLIFICATION_QUALITY ? 3 : 10;
                    size_t i;

                    for (i = 0; i < iters; ++i) {
                        num_blocks = FindBlocks(data, length,
                            block_switch_cost,
                            num_histograms, histograms,
                            insert_cost, cost, switch_signal,
                            block_ids);
                        num_histograms = RemapBlockIds(block_ids, length,
                            new_id, num_histograms);
                        BuildBlockHistograms(data, length, block_ids,
                            num_histograms, histograms);
                    }
                    BrotliFree(ref m, insert_cost);
                    BrotliFree(ref m, cost);
                    BrotliFree(ref m, switch_signal);
                    BrotliFree(ref m, new_id);
                    BrotliFree(ref m, histograms);
                    ClusterBlocks(ref m, data, length, num_blocks, block_ids, split);

                    BrotliFree(ref m, block_ids);
                }
            }
        }

        private unsafe partial struct BlockSplitterCommand
        {
            private static void InitialEntropyCodes(ushort* data, size_t length,
                size_t stride,
                size_t num_histograms,
                HistogramCommand* histograms)
            {
                uint seed = 7;
                size_t block_length = length / num_histograms;
                size_t i;
                HistogramCommand.ClearHistograms(histograms, num_histograms);
                for (i = 0; i < num_histograms; ++i)
                {
                    size_t pos = length * i / num_histograms;
                    if (i != 0)
                    {
                        pos += MyRand(&seed) % block_length;
                    }
                    if (pos + stride >= length)
                    {
                        pos = length - stride - 1;
                    }
                    HistogramCommand.HistogramAddVector(&histograms[i], data + pos, stride);
                }
            }

            private static unsafe void RandomSample(uint* seed,
                ushort* data,
                size_t length,
                size_t stride,
                HistogramCommand* sample)
            {
                size_t pos = 0;
                if (stride >= length)
                {
                    pos = 0;
                    stride = length;
                }
                else
                {
                    pos = MyRand(seed) % (length - stride + 1);
                }
                HistogramCommand.HistogramAddVector(sample, data + pos, stride);
            }

            private static unsafe void RefineEntropyCodes(ushort* data, size_t length,
                size_t stride,
                size_t num_histograms,
                HistogramCommand* histograms)
            {
                size_t iters =
                    kIterMulForRefining * length / stride + kMinItersForRefining;
                uint seed = 7;
                size_t iter;
                iters = ((iters + num_histograms - 1) / num_histograms) * num_histograms;
                for (iter = 0; iter < iters; ++iter)
                {
                    HistogramCommand sample;
                    HistogramCommand.HistogramClear(&sample);
                    RandomSample(&seed, data, length, stride, &sample);
                    HistogramCommand.HistogramAddHistogram(&histograms[iter % num_histograms], &sample);
                }
            }

            /* Assigns a block id from the range [0, num_histograms) to each data element
               in data[0..length) and fills in block_id[0..length) with the assigned values.
               Returns the number of blocks, i.e. one plus the number of block switches. */
            private static unsafe size_t FindBlocks(ushort* data, size_t length,
                double block_switch_bitcost,
                size_t num_histograms,
                HistogramCommand* histograms,
                double* insert_cost,
                double* cost,
                byte* switch_signal,
                byte* block_id)
            {
                size_t data_size = HistogramCommand.HistogramDataSize();
                size_t bitmaplen = (num_histograms + 7) >> 3;
                size_t num_blocks = 1;
                size_t i;
                size_t j;
                if (num_histograms <= 1)
                {
                    for (i = 0; i < length; ++i)
                    {
                        block_id[i] = 0;
                    }
                    return 1;
                }
                memset(insert_cost, 0, sizeof(double) * data_size * num_histograms);
                for (i = 0; i < num_histograms; ++i)
                {
                    insert_cost[i] = FastLog2((uint)histograms[i].total_count_);
                }
                for (i = data_size; i != 0;)
                {
                    --i;
                    for (j = 0; j < num_histograms; ++j)
                    {
                        insert_cost[i * num_histograms + j] =
                            insert_cost[j] - BitCost(histograms[j].data_[i]);
                    }
                }
                memset(cost, 0, sizeof(double) * num_histograms);
                memset(switch_signal, 0, sizeof(byte) * length * bitmaplen);
                /* After each iteration of this loop, cost[k] will contain the difference
                   between the minimum cost of arriving at the current byte position using
                   entropy code k, and the minimum cost of arriving at the current byte
                   position. This difference is capped at the block switch cost, and if it
                   reaches block switch cost, it means that when we trace back from the last
                   position, we need to switch here. */
                for (i = 0; i < length; ++i)
                {
                    size_t byte_ix = i;
                    size_t ix = byte_ix * bitmaplen;
                    size_t insert_cost_ix = data[byte_ix] * num_histograms;
                    double min_cost = 1e99;
                    double block_switch_cost = block_switch_bitcost;
                    size_t k;
                    for (k = 0; k < num_histograms; ++k)
                    {
                        /* We are coding the symbol in data[byte_ix] with entropy code k. */
                        cost[k] += insert_cost[insert_cost_ix + k];
                        if (cost[k] < min_cost)
                        {
                            min_cost = cost[k];
                            block_id[byte_ix] = (byte)k;
                        }
                    }
                    /* More blocks for the beginning. */
                    if (byte_ix < 2000)
                    {
                        block_switch_cost *= 0.77 + 0.07 * (double)byte_ix / 2000;
                    }
                    for (k = 0; k < num_histograms; ++k)
                    {
                        cost[k] -= min_cost;
                        if (cost[k] >= block_switch_cost)
                        {
                            byte mask = (byte)(1u << (int)(k & 7));
                            cost[k] = block_switch_cost;
                            switch_signal[ix + (k >> 3)] |= mask;
                        }
                    }
                }
                {
                    /* Trace back from the last position and switch at the marked places. */
                    size_t byte_ix = length - 1;
                    size_t ix = byte_ix * bitmaplen;
                    byte cur_id = block_id[byte_ix];
                    while (byte_ix > 0)
                    {
                        byte mask = (byte)(1u << (cur_id & 7));
                        --byte_ix;
                        ix -= bitmaplen;
                        if ((switch_signal[ix + (cur_id >> 3)] & mask) != 0)
                        {
                            if (cur_id != block_id[byte_ix])
                            {
                                cur_id = block_id[byte_ix];
                                ++num_blocks;
                            }
                        }
                        block_id[byte_ix] = cur_id;
                    }
                }
                return num_blocks;
            }

            private static size_t RemapBlockIds(byte* block_ids, size_t length,
                ushort* new_id, size_t num_histograms)
            {
                const ushort kInvalidId = 256;
                ushort next_id = 0;
                size_t i;
                for (i = 0; i < num_histograms; ++i)
                {
                    new_id[i] = kInvalidId;
                }
                for (i = 0; i < length; ++i)
                {
                    if (new_id[block_ids[i]] == kInvalidId)
                    {
                        new_id[block_ids[i]] = next_id++;
                    }
                }
                for (i = 0; i < length; ++i)
                {
                    block_ids[i] = (byte)new_id[block_ids[i]];
                }
                return next_id;
            }

            private static unsafe void BuildBlockHistograms(ushort* data, size_t length,
                byte* block_ids,
                size_t num_histograms,
                HistogramCommand* histograms)
            {
                size_t i;
                HistogramCommand.ClearHistograms(histograms, num_histograms);
                for (i = 0; i < length; ++i)
                {
                    HistogramCommand.HistogramAdd(&histograms[block_ids[i]], data[i]);
                }
            }

            private static unsafe void ClusterBlocks(ref MemoryManager m,
                ushort* data, size_t length,
                size_t num_blocks,
                byte* block_ids,
                BlockSplit* split)
            {
                uint* histogram_symbols = (uint*)BrotliAllocate(ref m, num_blocks * sizeof(uint));
                uint* block_lengths = (uint*)BrotliAllocate(ref m, num_blocks * sizeof(uint));
                size_t expected_num_clusters = CLUSTERS_PER_BATCH *
                                               (num_blocks + HISTOGRAMS_PER_BATCH - 1) / HISTOGRAMS_PER_BATCH;
                size_t all_histograms_size = 0;
                size_t all_histograms_capacity = expected_num_clusters;
                HistogramCommand* all_histograms =
                    (HistogramCommand*)BrotliAllocate(ref m, all_histograms_capacity * sizeof(HistogramCommand));
                size_t cluster_size_size = 0;
                size_t cluster_size_capacity = expected_num_clusters;
                uint* cluster_size = (uint*)BrotliAllocate(ref m, cluster_size_capacity * sizeof(uint));
                size_t num_clusters = 0;
                HistogramCommand* histograms = (HistogramCommand*)BrotliAllocate(ref m,
                    Math.Min(num_blocks, HISTOGRAMS_PER_BATCH) * sizeof(HistogramCommand));
                size_t max_num_pairs =
                    HISTOGRAMS_PER_BATCH * HISTOGRAMS_PER_BATCH / 2;
                size_t pairs_capacity = max_num_pairs + 1;
                HistogramPair* pairs = (HistogramPair*)BrotliAllocate(ref m, pairs_capacity * sizeof(HistogramPair));
                size_t pos = 0;
                uint* clusters;
                size_t num_final_clusters;
                const uint kInvalidIndex = uint.MaxValue;
                uint* new_index;
                size_t i;
                uint* sizes = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(sizes, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* new_clusters = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(new_clusters, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* symbols = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(symbols, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* remap = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(remap, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));


                memset(block_lengths, 0, num_blocks * sizeof(uint));

                {
                    size_t block_idx = 0;
                    for (i = 0; i < length; ++i)
                    {
                        ++block_lengths[block_idx];
                        if (i + 1 == length || block_ids[i] != block_ids[i + 1])
                        {
                            ++block_idx;
                        }
                    }
                }

                for (i = 0; i < num_blocks; i += HISTOGRAMS_PER_BATCH)
                {
                    size_t num_to_combine =
                        Math.Min(num_blocks - i, HISTOGRAMS_PER_BATCH);
                    size_t num_new_clusters;
                    size_t j;
                    for (j = 0; j < num_to_combine; ++j)
                    {
                        size_t k;
                        HistogramCommand.HistogramClear(&histograms[j]);
                        for (k = 0; k < block_lengths[i + j]; ++k)
                        {
                            HistogramCommand.HistogramAdd(&histograms[j], data[pos++]);
                        }
                        histograms[j].bit_cost_ = BitCostCommand.BrotliPopulationCost(&histograms[j]);
                        new_clusters[j] = (uint)j;
                        symbols[j] = (uint)j;
                        sizes[j] = 1;
                    }
                    num_new_clusters = ClusterCommand.BrotliHistogramCombine(
                        histograms, sizes, symbols, new_clusters, pairs, num_to_combine,
                        num_to_combine, HISTOGRAMS_PER_BATCH, max_num_pairs);

                    BrotliEnsureCapacity(ref m, sizeof(HistogramCommand), (void**)&all_histograms, &all_histograms_capacity, all_histograms_size + num_new_clusters);
                    BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&cluster_size, &cluster_size_capacity, cluster_size_size + num_new_clusters);

                    for (j = 0; j < num_new_clusters; ++j)
                    {
                        all_histograms[all_histograms_size++] = histograms[new_clusters[j]];
                        cluster_size[cluster_size_size++] = sizes[new_clusters[j]];
                        remap[new_clusters[j]] = (uint)j;
                    }
                    for (j = 0; j < num_to_combine; ++j)
                    {
                        histogram_symbols[i + j] = (uint)num_clusters + remap[symbols[j]];
                    }
                    num_clusters += num_new_clusters;
                }
                BrotliFree(ref m, histograms);

                max_num_pairs =
                    Math.Min(64 * num_clusters, (num_clusters / 2) * num_clusters);
                if (pairs_capacity < max_num_pairs + 1)
                {
                    BrotliFree(ref m, pairs);
                    pairs = (HistogramPair*)BrotliAllocate(ref m, (max_num_pairs + 1) * sizeof(HistogramPair));
                }

                clusters = (uint*)BrotliAllocate(ref m, num_clusters * sizeof(uint));

                for (i = 0; i < num_clusters; ++i)
                {
                    clusters[i] = (uint)i;
                }
                num_final_clusters = ClusterCommand.BrotliHistogramCombine(
                    all_histograms, cluster_size, histogram_symbols, clusters, pairs,
                    num_clusters, num_blocks, BROTLI_MAX_NUMBER_OF_BLOCK_TYPES,
                    max_num_pairs);
                BrotliFree(ref m, pairs);
                BrotliFree(ref m, cluster_size);

                new_index = (uint*)BrotliAllocate(ref m, num_clusters * sizeof(uint));

                for (i = 0; i < num_clusters; ++i) new_index[i] = kInvalidIndex;
                pos = 0;
                {
                    uint next_index = 0;
                    for (i = 0; i < num_blocks; ++i)
                    {
                        HistogramCommand histo;
                        size_t j;
                        uint best_out;
                        double best_bits;
                        HistogramCommand.HistogramClear(&histo);
                        for (j = 0; j < block_lengths[i]; ++j)
                        {
                            HistogramCommand.HistogramAdd(&histo, data[pos++]);
                        }
                        best_out = (i == 0) ? histogram_symbols[0] : histogram_symbols[i - 1];
                        best_bits =
                            ClusterCommand.BrotliHistogramBitCostDistance(&histo, &all_histograms[best_out]);
                        for (j = 0; j < num_final_clusters; ++j)
                        {
                            double cur_bits = ClusterCommand.BrotliHistogramBitCostDistance(
                                &histo, &all_histograms[clusters[j]]);
                            if (cur_bits < best_bits)
                            {
                                best_bits = cur_bits;
                                best_out = clusters[j];
                            }
                        }
                        histogram_symbols[i] = best_out;
                        if (new_index[best_out] == kInvalidIndex)
                        {
                            new_index[best_out] = next_index++;
                        }
                    }
                }
                BrotliFree(ref m, clusters);
                BrotliFree(ref m, all_histograms);

                BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, num_blocks);
                BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, num_blocks);

                {
                    uint cur_length = 0;
                    size_t block_idx = 0;
                    byte max_type = 0;
                    for (i = 0; i < num_blocks; ++i)
                    {
                        cur_length += block_lengths[i];
                        if (i + 1 == num_blocks ||
                            histogram_symbols[i] != histogram_symbols[i + 1])
                        {
                            byte id = (byte)new_index[histogram_symbols[i]];
                            split->types[block_idx] = id;
                            split->lengths[block_idx] = cur_length;
                            max_type = Math.Max(max_type, id);
                            cur_length = 0;
                            ++block_idx;
                        }
                    }
                    split->num_blocks = block_idx;
                    split->num_types = (size_t)max_type + 1;
                }
                BrotliFree(ref m, new_index);
                BrotliFree(ref m, block_lengths);
                BrotliFree(ref m, histogram_symbols);
            }

            public static unsafe void SplitByteVector(ref MemoryManager m,
                ushort* data, size_t length,
                size_t literals_per_histogram,
                size_t max_histograms,
                size_t sampling_stride_length,
                double block_switch_cost,
                BrotliEncoderParams* params_,
                BlockSplit* split)
            {
                size_t data_size = HistogramCommand.HistogramDataSize();
                size_t num_histograms = length / literals_per_histogram + 1;
                HistogramCommand* histograms;
                if (num_histograms > max_histograms)
                {
                    num_histograms = max_histograms;
                }
                if (length == 0)
                {
                    split->num_types = 1;
                    return;
                }
                else if (length < kMinLengthForBlockSplitting)
                {
                    BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, split->num_blocks + 1);
                    BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, split->num_blocks + 1);


                    split->num_types = 1;
                    split->types[split->num_blocks] = 0;
                    split->lengths[split->num_blocks] = (uint)length;
                    split->num_blocks++;
                    return;
                }
                histograms = (HistogramCommand*)BrotliAllocate(ref m, num_histograms * sizeof(HistogramCommand));

                /* Find good entropy codes. */
                InitialEntropyCodes(data, length,
                    sampling_stride_length,
                    num_histograms, histograms);
                RefineEntropyCodes(data, length,
                    sampling_stride_length,
                    num_histograms, histograms);
                {
                    /* Find a good path through literals with the good entropy codes. */
                    byte* block_ids = (byte*)BrotliAllocate(ref m, length * sizeof(byte));
                    size_t num_blocks = 0;
                    size_t bitmaplen = (num_histograms + 7) >> 3;
                    double* insert_cost = (double*)BrotliAllocate(ref m, data_size * num_histograms * sizeof(double));
                    double* cost = (double*)BrotliAllocate(ref m, num_histograms * sizeof(double));
                    byte* switch_signal = (byte*)BrotliAllocate(ref m, length * bitmaplen * sizeof(byte));
                    ushort* new_id = (ushort*)BrotliAllocate(ref m, num_histograms * sizeof(ushort));
                    size_t iters = params_->quality < HQ_ZOPFLIFICATION_QUALITY ? 3 : 10;
                    size_t i;

                    for (i = 0; i < iters; ++i)
                    {
                        num_blocks = FindBlocks(data, length,
                            block_switch_cost,
                            num_histograms, histograms,
                            insert_cost, cost, switch_signal,
                            block_ids);
                        num_histograms = RemapBlockIds(block_ids, length,
                            new_id, num_histograms);
                        BuildBlockHistograms(data, length, block_ids,
                            num_histograms, histograms);
                    }
                    BrotliFree(ref m, insert_cost);
                    BrotliFree(ref m, cost);
                    BrotliFree(ref m, switch_signal);
                    BrotliFree(ref m, new_id);
                    BrotliFree(ref m, histograms);
                    ClusterBlocks(ref m, data, length, num_blocks, block_ids, split);

                    BrotliFree(ref m, block_ids);
                }
            }
        }

        private unsafe partial struct BlockSplitterDistance
        {
            private static void InitialEntropyCodes(ushort* data, size_t length,
                size_t stride,
                size_t num_histograms,
                HistogramDistance* histograms)
            {
                uint seed = 7;
                size_t block_length = length / num_histograms;
                size_t i;
                HistogramDistance.ClearHistograms(histograms, num_histograms);
                for (i = 0; i < num_histograms; ++i)
                {
                    size_t pos = length * i / num_histograms;
                    if (i != 0)
                    {
                        pos += MyRand(&seed) % block_length;
                    }
                    if (pos + stride >= length)
                    {
                        pos = length - stride - 1;
                    }
                    HistogramDistance.HistogramAddVector(&histograms[i], data + pos, stride);
                }
            }

            private static unsafe void RandomSample(uint* seed,
                ushort* data,
                size_t length,
                size_t stride,
                HistogramDistance* sample)
            {
                size_t pos = 0;
                if (stride >= length)
                {
                    pos = 0;
                    stride = length;
                }
                else
                {
                    pos = MyRand(seed) % (length - stride + 1);
                }
                HistogramDistance.HistogramAddVector(sample, data + pos, stride);
            }

            private static unsafe void RefineEntropyCodes(ushort* data, size_t length,
                size_t stride,
                size_t num_histograms,
                HistogramDistance* histograms)
            {
                size_t iters =
                    kIterMulForRefining * length / stride + kMinItersForRefining;
                uint seed = 7;
                size_t iter;
                iters = ((iters + num_histograms - 1) / num_histograms) * num_histograms;
                for (iter = 0; iter < iters; ++iter)
                {
                    HistogramDistance sample;
                    HistogramDistance.HistogramClear(&sample);
                    RandomSample(&seed, data, length, stride, &sample);
                    HistogramDistance.HistogramAddHistogram(&histograms[iter % num_histograms], &sample);
                }
            }

            /* Assigns a block id from the range [0, num_histograms) to each data element
               in data[0..length) and fills in block_id[0..length) with the assigned values.
               Returns the number of blocks, i.e. one plus the number of block switches. */
            private static unsafe size_t FindBlocks(ushort* data, size_t length,
                double block_switch_bitcost,
                size_t num_histograms,
                HistogramDistance* histograms,
                double* insert_cost,
                double* cost,
                byte* switch_signal,
                byte* block_id)
            {
                size_t data_size = HistogramDistance.HistogramDataSize();
                size_t bitmaplen = (num_histograms + 7) >> 3;
                size_t num_blocks = 1;
                size_t i;
                size_t j;
                if (num_histograms <= 1)
                {
                    for (i = 0; i < length; ++i)
                    {
                        block_id[i] = 0;
                    }
                    return 1;
                }
                memset(insert_cost, 0, sizeof(double) * data_size * num_histograms);
                for (i = 0; i < num_histograms; ++i)
                {
                    insert_cost[i] = FastLog2((uint)histograms[i].total_count_);
                }
                for (i = data_size; i != 0;)
                {
                    --i;
                    for (j = 0; j < num_histograms; ++j)
                    {
                        insert_cost[i * num_histograms + j] =
                            insert_cost[j] - BitCost(histograms[j].data_[i]);
                    }
                }
                memset(cost, 0, sizeof(double) * num_histograms);
                memset(switch_signal, 0, sizeof(byte) * length * bitmaplen);
                /* After each iteration of this loop, cost[k] will contain the difference
                   between the minimum cost of arriving at the current byte position using
                   entropy code k, and the minimum cost of arriving at the current byte
                   position. This difference is capped at the block switch cost, and if it
                   reaches block switch cost, it means that when we trace back from the last
                   position, we need to switch here. */
                for (i = 0; i < length; ++i)
                {
                    size_t byte_ix = i;
                    size_t ix = byte_ix * bitmaplen;
                    size_t insert_cost_ix = data[byte_ix] * num_histograms;
                    double min_cost = 1e99;
                    double block_switch_cost = block_switch_bitcost;
                    size_t k;
                    for (k = 0; k < num_histograms; ++k)
                    {
                        /* We are coding the symbol in data[byte_ix] with entropy code k. */
                        cost[k] += insert_cost[insert_cost_ix + k];
                        if (cost[k] < min_cost)
                        {
                            min_cost = cost[k];
                            block_id[byte_ix] = (byte)k;
                        }
                    }
                    /* More blocks for the beginning. */
                    if (byte_ix < 2000)
                    {
                        block_switch_cost *= 0.77 + 0.07 * (double)byte_ix / 2000;
                    }
                    for (k = 0; k < num_histograms; ++k)
                    {
                        cost[k] -= min_cost;
                        if (cost[k] >= block_switch_cost)
                        {
                            byte mask = (byte)(1u << (int)(k & 7));
                            cost[k] = block_switch_cost;
                            switch_signal[ix + (k >> 3)] |= mask;
                        }
                    }
                }
                {
                    /* Trace back from the last position and switch at the marked places. */
                    size_t byte_ix = length - 1;
                    size_t ix = byte_ix * bitmaplen;
                    byte cur_id = block_id[byte_ix];
                    while (byte_ix > 0)
                    {
                        byte mask = (byte)(1u << (cur_id & 7));
                        --byte_ix;
                        ix -= bitmaplen;
                        if ((switch_signal[ix + (cur_id >> 3)] & mask) != 0)
                        {
                            if (cur_id != block_id[byte_ix])
                            {
                                cur_id = block_id[byte_ix];
                                ++num_blocks;
                            }
                        }
                        block_id[byte_ix] = cur_id;
                    }
                }
                return num_blocks;
            }

            private static size_t RemapBlockIds(byte* block_ids, size_t length,
                ushort* new_id, size_t num_histograms)
            {
                const ushort kInvalidId = 256;
                ushort next_id = 0;
                size_t i;
                for (i = 0; i < num_histograms; ++i)
                {
                    new_id[i] = kInvalidId;
                }
                for (i = 0; i < length; ++i)
                {
                    if (new_id[block_ids[i]] == kInvalidId)
                    {
                        new_id[block_ids[i]] = next_id++;
                    }
                }
                for (i = 0; i < length; ++i)
                {
                    block_ids[i] = (byte)new_id[block_ids[i]];
                }
                return next_id;
            }

            private static unsafe void BuildBlockHistograms(ushort* data, size_t length,
                byte* block_ids,
                size_t num_histograms,
                HistogramDistance* histograms)
            {
                size_t i;
                HistogramDistance.ClearHistograms(histograms, num_histograms);
                for (i = 0; i < length; ++i)
                {
                    HistogramDistance.HistogramAdd(&histograms[block_ids[i]], data[i]);
                }
            }

            private static unsafe void ClusterBlocks(ref MemoryManager m,
                ushort* data, size_t length,
                size_t num_blocks,
                byte* block_ids,
                BlockSplit* split)
            {
                uint* histogram_symbols = (uint*)BrotliAllocate(ref m, num_blocks * sizeof(uint));
                uint* block_lengths = (uint*)BrotliAllocate(ref m, num_blocks * sizeof(uint));
                size_t expected_num_clusters = CLUSTERS_PER_BATCH *
                                               (num_blocks + HISTOGRAMS_PER_BATCH - 1) / HISTOGRAMS_PER_BATCH;
                size_t all_histograms_size = 0;
                size_t all_histograms_capacity = expected_num_clusters;
                HistogramDistance* all_histograms =
                    (HistogramDistance*)BrotliAllocate(ref m, all_histograms_capacity * sizeof(HistogramDistance));
                size_t cluster_size_size = 0;
                size_t cluster_size_capacity = expected_num_clusters;
                uint* cluster_size = (uint*)BrotliAllocate(ref m, cluster_size_capacity * sizeof(uint));
                size_t num_clusters = 0;
                HistogramDistance* histograms = (HistogramDistance*)BrotliAllocate(ref m,
                    Math.Min(num_blocks, HISTOGRAMS_PER_BATCH) * sizeof(HistogramDistance));
                size_t max_num_pairs =
                    HISTOGRAMS_PER_BATCH * HISTOGRAMS_PER_BATCH / 2;
                size_t pairs_capacity = max_num_pairs + 1;
                HistogramPair* pairs = (HistogramPair*)BrotliAllocate(ref m, pairs_capacity * sizeof(HistogramPair));
                size_t pos = 0;
                uint* clusters;
                size_t num_final_clusters;
                const uint kInvalidIndex = uint.MaxValue;
                uint* new_index;
                size_t i;
                uint* sizes = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(sizes, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* new_clusters = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(new_clusters, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* symbols = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(symbols, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));
                uint* remap = stackalloc uint[HISTOGRAMS_PER_BATCH];
                memset(remap, 0, HISTOGRAMS_PER_BATCH * sizeof(uint));


                memset(block_lengths, 0, num_blocks * sizeof(uint));

                {
                    size_t block_idx = 0;
                    for (i = 0; i < length; ++i)
                    {
                        ++block_lengths[block_idx];
                        if (i + 1 == length || block_ids[i] != block_ids[i + 1])
                        {
                            ++block_idx;
                        }
                    }
                }

                for (i = 0; i < num_blocks; i += HISTOGRAMS_PER_BATCH)
                {
                    size_t num_to_combine =
                        Math.Min(num_blocks - i, HISTOGRAMS_PER_BATCH);
                    size_t num_new_clusters;
                    size_t j;
                    for (j = 0; j < num_to_combine; ++j)
                    {
                        size_t k;
                        HistogramDistance.HistogramClear(&histograms[j]);
                        for (k = 0; k < block_lengths[i + j]; ++k)
                        {
                            HistogramDistance.HistogramAdd(&histograms[j], data[pos++]);
                        }
                        histograms[j].bit_cost_ = BitCostDistance.BrotliPopulationCost(&histograms[j]);
                        new_clusters[j] = (uint)j;
                        symbols[j] = (uint)j;
                        sizes[j] = 1;
                    }
                    num_new_clusters = ClusterDistance.BrotliHistogramCombine(
                        histograms, sizes, symbols, new_clusters, pairs, num_to_combine,
                        num_to_combine, HISTOGRAMS_PER_BATCH, max_num_pairs);

                    BrotliEnsureCapacity(ref m, sizeof(HistogramDistance), (void**)&all_histograms, &all_histograms_capacity, all_histograms_size + num_new_clusters);
                    BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&cluster_size, &cluster_size_capacity, cluster_size_size + num_new_clusters);

                    for (j = 0; j < num_new_clusters; ++j)
                    {
                        all_histograms[all_histograms_size++] = histograms[new_clusters[j]];
                        cluster_size[cluster_size_size++] = sizes[new_clusters[j]];
                        remap[new_clusters[j]] = (uint)j;
                    }
                    for (j = 0; j < num_to_combine; ++j)
                    {
                        histogram_symbols[i + j] = (uint)num_clusters + remap[symbols[j]];
                    }
                    num_clusters += num_new_clusters;
                }
                BrotliFree(ref m, histograms);

                max_num_pairs =
                    Math.Min(64 * num_clusters, (num_clusters / 2) * num_clusters);
                if (pairs_capacity < max_num_pairs + 1)
                {
                    BrotliFree(ref m, pairs);
                    pairs = (HistogramPair*)BrotliAllocate(ref m, (max_num_pairs + 1) * sizeof(HistogramPair));
                }

                clusters = (uint*)BrotliAllocate(ref m, num_clusters * sizeof(uint));

                for (i = 0; i < num_clusters; ++i)
                {
                    clusters[i] = (uint)i;
                }
                num_final_clusters = ClusterDistance.BrotliHistogramCombine(
                    all_histograms, cluster_size, histogram_symbols, clusters, pairs,
                    num_clusters, num_blocks, BROTLI_MAX_NUMBER_OF_BLOCK_TYPES,
                    max_num_pairs);
                BrotliFree(ref m, pairs);
                BrotliFree(ref m, cluster_size);

                new_index = (uint*)BrotliAllocate(ref m, num_clusters * sizeof(uint));

                for (i = 0; i < num_clusters; ++i) new_index[i] = kInvalidIndex;
                pos = 0;
                {
                    uint next_index = 0;
                    for (i = 0; i < num_blocks; ++i)
                    {
                        HistogramDistance histo;
                        size_t j;
                        uint best_out;
                        double best_bits;
                        HistogramDistance.HistogramClear(&histo);
                        for (j = 0; j < block_lengths[i]; ++j)
                        {
                            HistogramDistance.HistogramAdd(&histo, data[pos++]);
                        }
                        best_out = (i == 0) ? histogram_symbols[0] : histogram_symbols[i - 1];
                        best_bits =
                            ClusterDistance.BrotliHistogramBitCostDistance(&histo, &all_histograms[best_out]);
                        for (j = 0; j < num_final_clusters; ++j)
                        {
                            double cur_bits = ClusterDistance.BrotliHistogramBitCostDistance(
                                &histo, &all_histograms[clusters[j]]);
                            if (cur_bits < best_bits)
                            {
                                best_bits = cur_bits;
                                best_out = clusters[j];
                            }
                        }
                        histogram_symbols[i] = best_out;
                        if (new_index[best_out] == kInvalidIndex)
                        {
                            new_index[best_out] = next_index++;
                        }
                    }
                }
                BrotliFree(ref m, clusters);
                BrotliFree(ref m, all_histograms);

                BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, num_blocks);
                BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, num_blocks);

                {
                    uint cur_length = 0;
                    size_t block_idx = 0;
                    byte max_type = 0;
                    for (i = 0; i < num_blocks; ++i)
                    {
                        cur_length += block_lengths[i];
                        if (i + 1 == num_blocks ||
                            histogram_symbols[i] != histogram_symbols[i + 1])
                        {
                            byte id = (byte)new_index[histogram_symbols[i]];
                            split->types[block_idx] = id;
                            split->lengths[block_idx] = cur_length;
                            max_type = Math.Max(max_type, id);
                            cur_length = 0;
                            ++block_idx;
                        }
                    }
                    split->num_blocks = block_idx;
                    split->num_types = (size_t)max_type + 1;
                }
                BrotliFree(ref m, new_index);
                BrotliFree(ref m, block_lengths);
                BrotliFree(ref m, histogram_symbols);
            }

            public static unsafe void SplitByteVector(ref MemoryManager m,
                ushort* data, size_t length,
                size_t literals_per_histogram,
                size_t max_histograms,
                size_t sampling_stride_length,
                double block_switch_cost,
                BrotliEncoderParams* params_,
                BlockSplit* split)
            {
                size_t data_size = HistogramDistance.HistogramDataSize();
                size_t num_histograms = length / literals_per_histogram + 1;
                HistogramDistance* histograms;
                if (num_histograms > max_histograms)
                {
                    num_histograms = max_histograms;
                }
                if (length == 0)
                {
                    split->num_types = 1;
                    return;
                }
                else if (length < kMinLengthForBlockSplitting)
                {
                    BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, split->num_blocks + 1);
                    BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, split->num_blocks + 1);

                    split->num_types = 1;
                    split->types[split->num_blocks] = 0;
                    split->lengths[split->num_blocks] = (uint)length;
                    split->num_blocks++;
                    return;
                }
                histograms = (HistogramDistance*)BrotliAllocate(ref m, num_histograms * sizeof(HistogramDistance));

                /* Find good entropy codes. */
                InitialEntropyCodes(data, length,
                    sampling_stride_length,
                    num_histograms, histograms);
                RefineEntropyCodes(data, length,
                    sampling_stride_length,
                    num_histograms, histograms);
                {
                    /* Find a good path through literals with the good entropy codes. */
                    byte* block_ids = (byte*)BrotliAllocate(ref m, length * sizeof(byte));
                    size_t num_blocks = 0;
                    size_t bitmaplen = (num_histograms + 7) >> 3;
                    double* insert_cost = (double*)BrotliAllocate(ref m, data_size * num_histograms * sizeof(double));
                    double* cost = (double*)BrotliAllocate(ref m, num_histograms * sizeof(double));
                    byte* switch_signal = (byte*)BrotliAllocate(ref m, length * bitmaplen * sizeof(byte));
                    ushort* new_id = (ushort*)BrotliAllocate(ref m, num_histograms * sizeof(ushort));
                    size_t iters = params_->quality < HQ_ZOPFLIFICATION_QUALITY ? 3 : 10;
                    size_t i;

                    for (i = 0; i < iters; ++i)
                    {
                        num_blocks = FindBlocks(data, length,
                            block_switch_cost,
                            num_histograms, histograms,
                            insert_cost, cost, switch_signal,
                            block_ids);
                        num_histograms = RemapBlockIds(block_ids, length,
                            new_id, num_histograms);
                        BuildBlockHistograms(data, length, block_ids,
                            num_histograms, histograms);
                    }
                    BrotliFree(ref m, insert_cost);
                    BrotliFree(ref m, cost);
                    BrotliFree(ref m, switch_signal);
                    BrotliFree(ref m, new_id);
                    BrotliFree(ref m, histograms);
                    ClusterBlocks(ref m, data, length, num_blocks, block_ids, split);

                    BrotliFree(ref m, block_ids);
                }
            }
        }
    }
}