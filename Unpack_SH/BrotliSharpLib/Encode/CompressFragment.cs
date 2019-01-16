using System;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private class CompressFragment {
            private static readonly size_t MAX_DISTANCE = BROTLI_MAX_BACKWARD_LIMIT(18);

            private static unsafe uint Hash(byte* p, size_t shift) {
                ulong h = (*(ulong*) (p) << 24) * kHashMul32;
                return (uint) (h >> (int) shift);
            }

            private static unsafe bool IsMatch(byte* p1, byte* p2) {
                return (
                    *(uint*) (p1) == *(uint*) (p2) &&
                    p1[4] == p2[4]);
            }

            /* REQUIRES: len <= 1 << 24. */
            private static unsafe void BrotliStoreMetaBlockHeader(
                size_t len, bool is_uncompressed, size_t* storage_ix,
                byte* storage) {
                size_t nibbles = 6;
                /* ISLAST */
                BrotliWriteBits(1, 0, storage_ix, storage);
                if (len <= (1U << 16)) {
                    nibbles = 4;
                }
                else if (len <= (1U << 20)) {
                    nibbles = 5;
                }
                BrotliWriteBits(2, nibbles - 4, storage_ix, storage);
                BrotliWriteBits(nibbles * 4, len - 1, storage_ix, storage);
                /* ISUNCOMPRESSED */
                BrotliWriteBits(1, is_uncompressed ? 1U : 0U, storage_ix, storage);
            }

            private static unsafe void RewindBitPosition(size_t new_storage_ix,
                size_t* storage_ix, byte* storage) {
                size_t bitpos = new_storage_ix & 7;
                size_t mask = (1u << (int) bitpos) - 1;
                storage[new_storage_ix >> 3] &= (byte) mask;
                *storage_ix = new_storage_ix;
            }

            private static unsafe void EmitUncompressedMetaBlock(byte* begin, byte* end,
                size_t storage_ix_start,
                size_t* storage_ix, byte* storage) {
                size_t len = (size_t) (end - begin);
                RewindBitPosition(storage_ix_start, storage_ix, storage);
                BrotliStoreMetaBlockHeader(len, true, storage_ix, storage);
                *storage_ix = (*storage_ix + 7u) & ~7u;
                memcpy(&storage[*storage_ix >> 3], begin, len);
                *storage_ix += len << 3;
                storage[*storage_ix >> 3] = 0;
            }

            /* Builds a literal prefix code into "depths" and "bits" based on the statistics
               of the "input" string and stores it into the bit stream.
               Note that the prefix code here is built from the pre-LZ77 input, therefore
               we can only approximate the statistics of the actual literal stream.
               Moreover, for long inputs we build a histogram from a sample of the input
               and thus have to assign a non-zero depth for each literal.
               Returns estimated compression ratio millibytes/char for encoding given input
               with generated code. */
            private static unsafe size_t BuildAndStoreLiteralPrefixCode(ref MemoryManager m,
                byte* input,
                size_t input_size,
                byte* depths,
                ushort* bits,
                size_t* storage_ix,
                byte* storage) {
                uint* histogram = stackalloc uint[256];
                size_t histogram_total;
                size_t i;
                if (input_size < (1 << 15)) {
                    for (i = 0; i < input_size; ++i) {
                        ++histogram[input[i]];
                    }
                    histogram_total = input_size;
                    for (i = 0; i < 256; ++i) {
                        /* We weigh the first 11 samples with weight 3 to account for the
                            balancing effect of the LZ77 phase on the histogram. */
                        uint adjust = 2 * Math.Min(histogram[i], 11u);
                        histogram[i] += adjust;
                        histogram_total += adjust;
                    }
                }
                else {
                    size_t kSampleRate = 29;
                    for (i = 0; i < input_size; i += kSampleRate) {
                        ++histogram[input[i]];
                    }
                    histogram_total = (input_size + kSampleRate - 1) / kSampleRate;
                    for (i = 0; i < 256; ++i) {
                        /* We add 1 to each population count to avoid 0 bit depths (since this is
                            only a sample and we don't know if the symbol appears or not), and we
                            weigh the first 11 samples with weight 3 to account for the balancing
                            effect of the LZ77 phase on the histogram (more frequent symbols are
                            more likely to be in backward references instead as literals). */
                        uint adjust = 1 + 2 * Math.Min(histogram[i], 11u);
                        histogram[i] += adjust;
                        histogram_total += adjust;
                    }
                }
                BrotliBuildAndStoreHuffmanTreeFast(ref m, histogram, histogram_total,
                    /* max_bits = */ 8,
                    depths, bits, storage_ix, storage);

                {
                    size_t literal_ratio = 0;
                    for (i = 0; i < 256; ++i) {
                        if (histogram[i] != 0) literal_ratio += histogram[i] * depths[i];
                    }
                    /* Estimated encoding ratio, millibytes per symbol. */
                    return (literal_ratio * 125) / histogram_total;
                }
            }

            /* REQUIRES: insertlen < 6210 */
            private static unsafe void EmitInsertLen(size_t insertlen,
                byte* depth,
                ushort* bits,
                uint* histo,
                size_t* storage_ix,
                byte* storage) {
                if (insertlen < 6) {
                    size_t code = insertlen + 40;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    ++histo[code];
                }
                else if (insertlen < 130) {
                    size_t tail = insertlen - 2;
                    uint nbits = Log2FloorNonZero(tail) - 1u;
                    size_t prefix = tail >> (int) nbits;
                    size_t inscode = (nbits << 1) + prefix + 42;
                    BrotliWriteBits(depth[inscode], bits[inscode], storage_ix, storage);
                    BrotliWriteBits(nbits, tail - (prefix << (int) nbits), storage_ix, storage);
                    ++histo[inscode];
                }
                else if (insertlen < 2114) {
                    size_t tail = insertlen - 66;
                    uint nbits = Log2FloorNonZero(tail);
                    size_t code = nbits + 50;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(nbits, tail - ((size_t) 1 << (int) nbits), storage_ix, storage);
                    ++histo[code];
                }
                else {
                    BrotliWriteBits(depth[61], bits[61], storage_ix, storage);
                    BrotliWriteBits(12, insertlen - 2114, storage_ix, storage);
                    ++histo[21];
                }
            }

            private const int MIN_RATIO = 980;

            /* Acceptable loss for uncompressible speedup is 2% */
            private static unsafe bool ShouldUseUncompressedMode(
                byte* metablock_start, byte* next_emit,
                size_t insertlen, size_t literal_ratio) {
                size_t compressed = (size_t) (next_emit - metablock_start);
                if (compressed * 50 > insertlen) {
                    return false;
                }
                else {
                    return (literal_ratio > MIN_RATIO);
                }
            }

            private static unsafe void EmitLongInsertLen(size_t insertlen,
                byte* depth,
                ushort* bits,
                uint* histo,
                size_t* storage_ix,
                byte* storage) {
                if (insertlen < 22594) {
                    BrotliWriteBits(depth[62], bits[62], storage_ix, storage);
                    BrotliWriteBits(14, insertlen - 6210, storage_ix, storage);
                    ++histo[22];
                }
                else {
                    BrotliWriteBits(depth[63], bits[63], storage_ix, storage);
                    BrotliWriteBits(24, insertlen - 22594, storage_ix, storage);
                    ++histo[23];
                }
            }

            private static unsafe void EmitDistance(size_t distance,
                byte* depth,
                ushort* bits,
                uint* histo,
                size_t* storage_ix, byte* storage) {
                size_t d = distance + 3;
                uint nbits = Log2FloorNonZero(d) - 1u;
                size_t prefix = (d >> (int) nbits) & 1;
                size_t offset = (2 + prefix) << (int) nbits;
                size_t distcode = 2 * (nbits - 1) + prefix + 80;
                BrotliWriteBits(depth[distcode], bits[distcode], storage_ix, storage);
                BrotliWriteBits(nbits, d - offset, storage_ix, storage);
                ++histo[distcode];
            }

            private static unsafe void EmitLiterals(byte* input, size_t len,
                byte* depth,
                ushort* bits,
                size_t* storage_ix, byte* storage) {
                size_t j;
                for (j = 0; j < len; j++) {
                    byte lit = input[j];
                    BrotliWriteBits(depth[lit], bits[lit], storage_ix, storage);
                }
            }

            private static unsafe void EmitCopyLenLastDistance(size_t copylen,
                byte* depth,
                ushort* bits,
                uint* histo,
                size_t* storage_ix,
                byte* storage) {
                if (copylen < 12) {
                    BrotliWriteBits(depth[copylen - 4], bits[copylen - 4], storage_ix, storage);
                    ++histo[copylen - 4];
                }
                else if (copylen < 72) {
                    size_t tail = copylen - 8;
                    uint nbits = Log2FloorNonZero(tail) - 1;
                    size_t prefix = tail >> (int) nbits;
                    size_t code = (nbits << 1) + prefix + 4;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(nbits, tail - (prefix << (int) nbits), storage_ix, storage);
                    ++histo[code];
                }
                else if (copylen < 136) {
                    size_t tail = copylen - 8;
                    size_t code = (tail >> 5) + 30;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(5, tail & 31, storage_ix, storage);
                    BrotliWriteBits(depth[64], bits[64], storage_ix, storage);
                    ++histo[code];
                    ++histo[64];
                }
                else if (copylen < 2120) {
                    size_t tail = copylen - 72;
                    uint nbits = Log2FloorNonZero(tail);
                    size_t code = nbits + 28;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(nbits, tail - ((size_t) 1 << (int) nbits), storage_ix, storage);
                    BrotliWriteBits(depth[64], bits[64], storage_ix, storage);
                    ++histo[code];
                    ++histo[64];
                }
                else {
                    BrotliWriteBits(depth[39], bits[39], storage_ix, storage);
                    BrotliWriteBits(24, copylen - 2120, storage_ix, storage);
                    BrotliWriteBits(depth[64], bits[64], storage_ix, storage);
                    ++histo[47];
                    ++histo[64];
                }
            }

            private static unsafe uint HashBytesAtOffset(
                ulong v, int offset, size_t shift) {
                {
                    ulong h = ((v >> (8 * offset)) << 24) * kHashMul32;
                    return (uint) (h >> (int) shift);
                }
            }

            private static unsafe void EmitCopyLen(size_t copylen,
                byte* depth,
                ushort* bits,
                uint* histo,
                size_t* storage_ix,
                byte* storage) {
                if (copylen < 10) {
                    BrotliWriteBits(
                        depth[copylen + 14], bits[copylen + 14], storage_ix, storage);
                    ++histo[copylen + 14];
                }
                else if (copylen < 134) {
                    size_t tail = copylen - 6;
                    uint nbits = Log2FloorNonZero(tail) - 1u;
                    size_t prefix = tail >> (int) nbits;
                    size_t code = (nbits << 1) + prefix + 20;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(nbits, tail - (prefix << (int) nbits), storage_ix, storage);
                    ++histo[code];
                }
                else if (copylen < 2118) {
                    size_t tail = copylen - 70;
                    uint nbits = Log2FloorNonZero(tail);
                    size_t code = nbits + 28;
                    BrotliWriteBits(depth[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(nbits, tail - ((size_t) 1 << (int) nbits), storage_ix, storage);
                    ++histo[code];
                }
                else {
                    BrotliWriteBits(depth[39], bits[39], storage_ix, storage);
                    BrotliWriteBits(24, copylen - 2118, storage_ix, storage);
                    ++histo[47];
                }
            }

            private static unsafe bool ShouldMergeBlock(
                byte* data, size_t len, byte* depths) {
                size_t[] histo = new size_t[256];
                size_t kSampleRate = 43;
                size_t i;
                for (i = 0; i < len; i += kSampleRate) {
                    ++histo[data[i]];
                }
                {
                    size_t total = (len + kSampleRate - 1) / kSampleRate;
                    double r = (FastLog2(total) + 0.5) * (double) total + 200;
                    for (i = 0; i < 256; ++i) {
                        r -= (double) histo[i] * (depths[i] + FastLog2(histo[i]));
                    }
                    return (r >= 0.0);
                }
            }

            private static unsafe void UpdateBits(size_t n_bits, uint bits, size_t pos,
                byte* array) {
                while (n_bits > 0) {
                    size_t byte_pos = pos >> 3;
                    size_t n_unchanged_bits = pos & 7;
                    size_t n_changed_bits = Math.Min(n_bits, 8 - n_unchanged_bits);
                    size_t total_bits = n_unchanged_bits + n_changed_bits;
                    uint mask =
                        (~((1u << (int) total_bits) - 1u)) | ((1u << (int) n_unchanged_bits) - 1u);
                    uint unchanged_bits = array[byte_pos] & mask;
                    uint changed_bits = bits & ((1u << (int) n_changed_bits) - 1u);
                    array[byte_pos] =
                        (byte) ((changed_bits << (int) n_unchanged_bits) | unchanged_bits);
                    n_bits -= n_changed_bits;
                    bits >>= (int) n_changed_bits;
                    pos += n_changed_bits;
                }
            }

            /* Builds a command and distance prefix code (each 64 symbols) into "depth" and
               "bits" based on "histogram" and stores it into the bit stream. */
            private static unsafe void BuildAndStoreCommandPrefixCode(uint* histogram,
                byte* depth, ushort* bits, size_t* storage_ix,
                byte* storage) {
                /* Tree size for building a tree over 64 symbols is 2 * 64 + 1. */
                HuffmanTree* tree = stackalloc HuffmanTree[129];
                byte* cmd_depth = stackalloc byte[BROTLI_NUM_COMMAND_SYMBOLS];
                memset(cmd_depth, 0, BROTLI_NUM_COMMAND_SYMBOLS);
                ushort* cmd_bits = stackalloc ushort[64];

                BrotliCreateHuffmanTree(histogram, 64, 15, tree, depth);
                BrotliCreateHuffmanTree(&histogram[64], 64, 14, tree, &depth[64]);
                /* We have to jump through a few hoops here in order to compute
                   the command bits because the symbols are in a different order than in
                   the full alphabet. This looks complicated, but having the symbols
                   in this order in the command bits saves a few branches in the Emit*
                   functions. */
                memcpy(cmd_depth, depth, 24);
                memcpy(cmd_depth + 24, depth + 40, 8);
                memcpy(cmd_depth + 32, depth + 24, 8);
                memcpy(cmd_depth + 40, depth + 48, 8);
                memcpy(cmd_depth + 48, depth + 32, 8);
                memcpy(cmd_depth + 56, depth + 56, 8);
                BrotliConvertBitDepthsToSymbols(cmd_depth, 64, cmd_bits);
                memcpy(bits, cmd_bits, 48);
                memcpy(bits + 24, cmd_bits + 32, 16);
                memcpy(bits + 32, cmd_bits + 48, 16);
                memcpy(bits + 40, cmd_bits + 24, 16);
                memcpy(bits + 48, cmd_bits + 40, 16);
                memcpy(bits + 56, cmd_bits + 56, 16);
                BrotliConvertBitDepthsToSymbols(&depth[64], 64, &bits[64]);
                {
                    /* Create the bit length array for the full command alphabet. */
                    size_t i;
                    memset(cmd_depth, 0, 64); /* only 64 first values were used */
                    memcpy(cmd_depth, depth, 8);
                    memcpy(cmd_depth + 64, depth + 8, 8);
                    memcpy(cmd_depth + 128, depth + 16, 8);
                    memcpy(cmd_depth + 192, depth + 24, 8);
                    memcpy(cmd_depth + 384, depth + 32, 8);
                    for (i = 0; i < 8; ++i) {
                        cmd_depth[128 + 8 * i] = depth[40 + i];
                        cmd_depth[256 + 8 * i] = depth[48 + i];
                        cmd_depth[448 + 8 * i] = depth[56 + i];
                    }
                    BrotliStoreHuffmanTree(
                        cmd_depth, BROTLI_NUM_COMMAND_SYMBOLS, tree, storage_ix, storage);
                }
                BrotliStoreHuffmanTree(&depth[64], 64, tree, storage_ix, storage);
            }

            private static unsafe void BrotliCompressFragmentFastImpl(
                ref MemoryManager m, byte* input, size_t input_size,
                bool is_last, int* table, size_t table_bits, byte* cmd_depth,
                ushort* cmd_bits, size_t* cmd_code_numbits, byte* cmd_code,
                size_t* storage_ix, byte* storage) {
                uint* cmd_histo = stackalloc uint[128];
                byte* ip_end;

                /* "next_emit" is a pointer to the first byte that is not covered by a
                   previous copy. Bytes between "next_emit" and the start of the next copy or
                   the end of the input will be emitted as literal bytes. */
                byte* next_emit = input;
                /* Save the start of the first block for position and distance computations.
                */
                byte* base_ip = input;

                size_t kFirstBlockSize = 3 << 15;
                size_t kMergeBlockSize = 1 << 16;

                size_t kInputMarginBytes = BROTLI_WINDOW_GAP;
                size_t kMinMatchLen = 5;

                byte* metablock_start = input;
                size_t block_size = Math.Min(input_size, kFirstBlockSize);
                size_t total_block_size = block_size;
                /* Save the bit position of the MLEN field of the meta-block header, so that
                   we can update it later if we decide to extend this meta-block. */
                size_t mlen_storage_ix = *storage_ix + 3;

                byte* lit_depth = stackalloc byte[256];
                ushort* lit_bits = stackalloc ushort[256];

                size_t literal_ratio;

                byte* ip;
                int last_distance;

                size_t shift = 64u - table_bits;

                BrotliStoreMetaBlockHeader(block_size, false, storage_ix, storage);
                /* No block splits, no contexts. */
                BrotliWriteBits(13, 0, storage_ix, storage);

                literal_ratio = BuildAndStoreLiteralPrefixCode(
                    ref m, input, block_size, lit_depth, lit_bits, storage_ix, storage);

                {
                    /* Store the pre-compressed command and distance prefix codes. */
                    size_t i;
                    for (i = 0; i + 7 < *cmd_code_numbits; i += 8) {
                        BrotliWriteBits(8, cmd_code[i >> 3], storage_ix, storage);
                    }
                }
                BrotliWriteBits(*cmd_code_numbits & 7, cmd_code[*cmd_code_numbits >> 3],
                    storage_ix, storage);

                emit_commands:
                /* Initialize the command and distance histograms. We will gather
                   statistics of command and distance codes during the processing
                   of this block and use it to update the command and distance
                   prefix codes for the next block. */
                fixed (uint* kchs = kCmdHistoSeed)
                    memcpy(cmd_histo, kchs, kCmdHistoSeed.Length * sizeof(uint));

                /* "ip" is the input pointer. */
                ip = input;
                last_distance = -1;
                ip_end = input + block_size;

                if (block_size >= kInputMarginBytes) {
                    /* For the last block, we need to keep a 16 bytes margin so that we can be
                       sure that all distances are at most window size - 16.
                       For all other blocks, we only need to keep a margin of 5 bytes so that
                       we don't go over the block size with a copy. */
                    size_t len_limit = Math.Min(block_size - kMinMatchLen,
                        input_size - kInputMarginBytes);
                    byte* ip_limit = input + len_limit;

                    uint next_hash;
                    for (next_hash = Hash(++ip, shift);;) {
                        /* Step 1: Scan forward in the input looking for a 5-byte-long match.
                           If we get close to exhausting the input then goto emit_remainder.
    
                           Heuristic match skipping: If 32 bytes are scanned with no matches
                           found, start looking only at every other byte. If 32 more bytes are
                           scanned, look at every third byte, etc.. When a match is found,
                           immediately go back to looking at every byte. This is a small loss
                           (~5% performance, ~0.1% density) for compressible data due to more
                           bookkeeping, but for non-compressible data (such as JPEG) it's a huge
                           win since the compressor quickly "realizes" the data is incompressible
                           and doesn't bother looking for matches everywhere.
    
                           The "skip" variable keeps track of how many bytes there are since the
                           last match; dividing it by 32 (i.e. right-shifting by five) gives the
                           number of bytes to move ahead for each iteration. */
                        uint skip = 32;

                        byte* next_ip = ip;
                        byte* candidate;
                        trawl:
                        do {
                            uint hash = next_hash;
                            uint bytes_between_hash_lookups = skip++ >> 5;
                            ip = next_ip;
                            next_ip = ip + bytes_between_hash_lookups;
                            if (next_ip > ip_limit) {
                                goto emit_remainder;
                            }
                            next_hash = Hash(next_ip, shift);
                            candidate = ip - last_distance;
                            if (IsMatch(ip, candidate)) {
                                if (candidate < ip) {
                                    table[hash] = (int) (ip - base_ip);
                                    break;
                                }
                            }
                            candidate = base_ip + table[hash];

                            table[hash] = (int) (ip - base_ip);
                        } while (!IsMatch(ip, candidate));

                        /* Check copy distance. If candidate is not feasible, continue search.
                           Checking is done outside of hot loop to reduce overhead. */
                        if (ip - candidate > MAX_DISTANCE) goto trawl;

                        /* Step 2: Emit the found match together with the literal bytes from
                           "next_emit" to the bit stream, and then see if we can find a next match
                           immediately afterwards. Repeat until we find no match for the input
                           without emitting some literal bytes. */

                        {
                            /* We have a 5-byte match at ip, and we need to emit bytes in
                               [next_emit, ip). */
                            byte* base_ = ip;
                            size_t matched = 5 + FindMatchLengthWithLimit(
                                                 candidate + 5, ip + 5, (size_t) (ip_end - ip) - 5);
                            int distance = (int) (base_ - candidate); /* > 0 */
                            size_t insert = (size_t) (base_ - next_emit);
                            ip += matched;
                            if (insert < 6210) {
                                EmitInsertLen(insert, cmd_depth, cmd_bits, cmd_histo,
                                    storage_ix, storage);
                            }
                            else if (ShouldUseUncompressedMode(metablock_start, next_emit, insert,
                                literal_ratio)) {
                                EmitUncompressedMetaBlock(metablock_start, base_, mlen_storage_ix - 3,
                                    storage_ix, storage);
                                input_size -= (size_t) (base_ - input);
                                input = base_;
                                next_emit = input;
                                goto next_block;
                            }
                            else {
                                EmitLongInsertLen(insert, cmd_depth, cmd_bits, cmd_histo,
                                    storage_ix, storage);
                            }
                            EmitLiterals(next_emit, insert, lit_depth, lit_bits,
                                storage_ix, storage);
                            if (distance == last_distance) {
                                BrotliWriteBits(cmd_depth[64], cmd_bits[64], storage_ix, storage);
                                ++cmd_histo[64];
                            }
                            else {
                                EmitDistance((size_t) distance, cmd_depth, cmd_bits,
                                    cmd_histo, storage_ix, storage);
                                last_distance = distance;
                            }
                            EmitCopyLenLastDistance(matched, cmd_depth, cmd_bits, cmd_histo,
                                storage_ix, storage);

                            next_emit = ip;
                            if (ip >= ip_limit) {
                                goto emit_remainder;
                            }
                            /* We could immediately start working at ip now, but to improve
                               compression we first update "table" with the hashes of some positions
                               within the last copy. */
                            {
                                ulong input_bytes = *(ulong*) (ip - 3);
                                uint prev_hash = HashBytesAtOffset(input_bytes, 0, shift);
                                uint cur_hash = HashBytesAtOffset(input_bytes, 3, shift);
                                table[prev_hash] = (int) (ip - base_ip - 3);
                                prev_hash = HashBytesAtOffset(input_bytes, 1, shift);
                                table[prev_hash] = (int) (ip - base_ip - 2);
                                prev_hash = HashBytesAtOffset(input_bytes, 2, shift);
                                table[prev_hash] = (int) (ip - base_ip - 1);

                                candidate = base_ip + table[cur_hash];
                                table[cur_hash] = (int) (ip - base_ip);
                            }
                        }

                        while (IsMatch(ip, candidate)) {
                            /* We have a 5-byte match at ip, and no need to emit any literal bytes
                               prior to ip. */
                            byte* base_ = ip;
                            size_t matched = 5 + FindMatchLengthWithLimit(
                                                 candidate + 5, ip + 5, (size_t) (ip_end - ip) - 5);
                            if (ip - candidate > MAX_DISTANCE) break;
                            ip += matched;
                            last_distance = (int) (base_ - candidate); /* > 0 */
                            EmitCopyLen(matched, cmd_depth, cmd_bits, cmd_histo,
                                storage_ix, storage);
                            EmitDistance((size_t) last_distance, cmd_depth, cmd_bits,
                                cmd_histo, storage_ix, storage);

                            next_emit = ip;
                            if (ip >= ip_limit) {
                                goto emit_remainder;
                            }
                            /* We could immediately start working at ip now, but to improve
                               compression we first update "table" with the hashes of some positions
                               within the last copy. */
                            {
                                ulong input_bytes = *(ulong*) (ip - 3);
                                uint prev_hash = HashBytesAtOffset(input_bytes, 0, shift);
                                uint cur_hash = HashBytesAtOffset(input_bytes, 3, shift);
                                table[prev_hash] = (int) (ip - base_ip - 3);
                                prev_hash = HashBytesAtOffset(input_bytes, 1, shift);
                                table[prev_hash] = (int) (ip - base_ip - 2);
                                prev_hash = HashBytesAtOffset(input_bytes, 2, shift);
                                table[prev_hash] = (int) (ip - base_ip - 1);

                                candidate = base_ip + table[cur_hash];
                                table[cur_hash] = (int) (ip - base_ip);
                            }
                        }

                        next_hash = Hash(++ip, shift);
                    }
                }

                emit_remainder:
                input += block_size;
                input_size -= block_size;
                block_size = Math.Min(input_size, kMergeBlockSize);

                /* Decide if we want to continue this meta-block instead of emitting the
                   last insert-only command. */
                if (input_size > 0 &&
                    total_block_size + block_size <= (1 << 20) &&
                    ShouldMergeBlock(input, block_size, lit_depth)) {
                    /* Update the size of the current meta-block and continue emitting commands.
                       We can do this because the current size and the new size both have 5
                       nibbles. */
                    total_block_size += block_size;
                    UpdateBits(20, (uint) (total_block_size - 1), mlen_storage_ix, storage);
                    goto emit_commands;
                }

                /* Emit the remaining bytes as literals. */
                if (next_emit < ip_end) {
                    size_t insert = (size_t) (ip_end - next_emit);
                    if (insert < 6210) {
                        EmitInsertLen(insert, cmd_depth, cmd_bits, cmd_histo,
                            storage_ix, storage);
                        EmitLiterals(next_emit, insert, lit_depth, lit_bits, storage_ix, storage);
                    }
                    else if (ShouldUseUncompressedMode(metablock_start, next_emit, insert,
                        literal_ratio)) {
                        EmitUncompressedMetaBlock(metablock_start, ip_end, mlen_storage_ix - 3,
                            storage_ix, storage);
                    }
                    else {
                        EmitLongInsertLen(insert, cmd_depth, cmd_bits, cmd_histo,
                            storage_ix, storage);
                        EmitLiterals(next_emit, insert, lit_depth, lit_bits,
                            storage_ix, storage);
                    }
                }
                next_emit = ip_end;

                next_block:
                /* If we have more data, write a new meta-block header and prefix codes and
                   then continue emitting commands. */
                if (input_size > 0) {
                    metablock_start = input;
                    block_size = Math.Min(input_size, kFirstBlockSize);
                    total_block_size = block_size;
                    /* Save the bit position of the MLEN field of the meta-block header, so that
                       we can update it later if we decide to extend this meta-block. */
                    mlen_storage_ix = *storage_ix + 3;
                    BrotliStoreMetaBlockHeader(block_size, false, storage_ix, storage);
                    /* No block splits, no contexts. */
                    BrotliWriteBits(13, 0, storage_ix, storage);
                    literal_ratio = BuildAndStoreLiteralPrefixCode(
                        ref m, input, block_size, lit_depth, lit_bits, storage_ix, storage);
                    BuildAndStoreCommandPrefixCode(cmd_histo, cmd_depth, cmd_bits,
                        storage_ix, storage);
                    goto emit_commands;
                }

                if (!is_last) {
                    /* If this is not the last block, update the command and distance prefix
                       codes for the next block and store the compressed forms. */
                    cmd_code[0] = 0;
                    *cmd_code_numbits = 0;
                    BuildAndStoreCommandPrefixCode(cmd_histo, cmd_depth, cmd_bits,
                        cmd_code_numbits, cmd_code);
                }
            }

            public static unsafe void BrotliCompressFragmentFast(
                ref MemoryManager m, byte* input, size_t input_size,
                bool is_last, int* table, size_t table_size, byte* cmd_depth,
                ushort* cmd_bits, size_t* cmd_code_numbits, byte* cmd_code,
                size_t* storage_ix, byte* storage) {
                size_t initial_storage_ix = *storage_ix;
                size_t table_bits = Log2FloorNonZero(table_size);

                if (input_size == 0) {
                    BrotliWriteBits(1, 1, storage_ix, storage); /* islast */
                    BrotliWriteBits(1, 1, storage_ix, storage); /* isempty */
                    *storage_ix = (*storage_ix + 7u) & ~7u;
                    return;
                }

                switch ((int) table_bits) {
                    case 9:
                    case 11:
                    case 13:
                    case 15:
                        BrotliCompressFragmentFastImpl(
                            ref m, input, input_size, is_last, table, table_bits,
                            cmd_depth, cmd_bits, cmd_code_numbits, cmd_code, storage_ix, storage);
                        break;
                }

                /* If output is larger than single uncompressed block, rewrite it. */
                if (*storage_ix - initial_storage_ix > 31 + (input_size << 3)) {
                    EmitUncompressedMetaBlock(input, input + input_size, initial_storage_ix,
                        storage_ix, storage);
                }

                if (is_last) {
                    BrotliWriteBits(1, 1, storage_ix, storage); /* islast */
                    BrotliWriteBits(1, 1, storage_ix, storage); /* isempty */
                    *storage_ix = (*storage_ix + 7u) & ~7u;
                }
            }
        }

        private static unsafe void BrotliCompressFragmentFast(
            ref MemoryManager m, byte* input, size_t input_size,
            bool is_last, int* table, size_t table_size, byte* cmd_depth,
            ushort* cmd_bits, size_t* cmd_code_numbits, byte* cmd_code,
            size_t* storage_ix, byte* storage) {
            CompressFragment.BrotliCompressFragmentFast(ref m, input, input_size,
                is_last, table, table_size, cmd_depth, cmd_bits,
                cmd_code_numbits, cmd_code, storage_ix, storage);
        }
    }
}