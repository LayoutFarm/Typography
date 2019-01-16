using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        /* A node of a Huffman tree. */
        [StructLayout(LayoutKind.Sequential)]
        private struct HuffmanTree {
            public uint total_count_;
            public short index_left_;
            public short index_right_or_value_;
        }

        private static unsafe void InitHuffmanTree(HuffmanTree* self, uint count,
            short left, short right) {
            self->total_count_ = count;
            self->index_left_ = left;
            self->index_right_or_value_ = right;
        }

        private static ushort BrotliReverseBits(size_t num_bits, ushort bits) {
            size_t[] kLut = {
                /* Pre-reversed 4-bit values. */
                0x0, 0x8, 0x4, 0xc, 0x2, 0xa, 0x6, 0xe,
                0x1, 0x9, 0x5, 0xd, 0x3, 0xb, 0x7, 0xf
            };
            size_t retval = kLut[bits & 0xf];
            size_t i;
            for (i = 4; i < num_bits; i += 4) {
                retval <<= 4;
                bits = (ushort) (bits >> 4);
                retval |= kLut[bits & 0xf];
            }
            retval >>= (int) ((0 - num_bits) & 0x3);
            return (ushort) retval;
        }

        private static unsafe void BrotliConvertBitDepthsToSymbols(byte* depth,
            size_t len,
            ushort* bits) {
            /* In Brotli, all bit depths are [1..15]
               0 bit depth means that the symbol does not exist. */
            ushort[] bl_count = new ushort[MAX_HUFFMAN_BITS];
            ushort[] next_code = new ushort[MAX_HUFFMAN_BITS];
            size_t i;
            int code = 0;
            for (i = 0; i < len; ++i) {
                ++bl_count[depth[i]];
            }
            bl_count[0] = 0;
            next_code[0] = 0;
            for (i = 1; i < MAX_HUFFMAN_BITS; ++i) {
                code = (code + bl_count[i - 1]) << 1;
                next_code[i] = (ushort) code;
            }
            for (i = 0; i < len; ++i) {
                if (depth[i] != 0) {
                    bits[i] = BrotliReverseBits(depth[i], next_code[depth[i]]++);
                }
            }
        }

        private static unsafe bool BrotliSetDepth(
            int p0, HuffmanTree* pool, byte* depth, int max_depth) {
            int[] stack = new int[16];
            int level = 0;
            int p = p0;
            stack[0] = -1;
            while (true) {
                if (pool[p].index_left_ >= 0) {
                    level++;
                    if (level > max_depth) return false;
                    stack[level] = pool[p].index_right_or_value_;
                    p = pool[p].index_left_;
                    continue;
                }
                else {
                    depth[pool[p].index_right_or_value_] = (byte) level;
                }
                while (level >= 0 && stack[level] == -1) level--;
                if (level < 0) return true;
                p = stack[level];
                stack[level] = -1;
            }
        }

        private static unsafe void DecideOverRleUse(byte* depth, size_t length,
            bool* use_rle_for_non_zero,
            bool* use_rle_for_zero) {
            size_t total_reps_zero = 0;
            size_t total_reps_non_zero = 0;
            size_t count_reps_zero = 1;
            size_t count_reps_non_zero = 1;
            size_t i;
            for (i = 0; i < length;) {
                byte value = depth[i];
                size_t reps = 1;
                size_t k;
                for (k = i + 1; k < length && depth[k] == value; ++k) {
                    ++reps;
                }
                if (reps >= 3 && value == 0) {
                    total_reps_zero += reps;
                    ++count_reps_zero;
                }
                if (reps >= 4 && value != 0) {
                    total_reps_non_zero += reps;
                    ++count_reps_non_zero;
                }
                i += reps;
            }
            *use_rle_for_non_zero =
                (total_reps_non_zero > count_reps_non_zero * 2);
            *use_rle_for_zero = (total_reps_zero > count_reps_zero * 2);
        }

        private static unsafe void BrotliWriteHuffmanTreeRepetitionsZeros(
            size_t repetitions,
            size_t* tree_size,
            byte* tree,
            byte* extra_bits_data) {
            if (repetitions == 11) {
                tree[*tree_size] = 0;
                extra_bits_data[*tree_size] = 0;
                ++(*tree_size);
                --repetitions;
            }
            if (repetitions < 3) {
                size_t i;
                for (i = 0; i < repetitions; ++i) {
                    tree[*tree_size] = 0;
                    extra_bits_data[*tree_size] = 0;
                    ++(*tree_size);
                }
            }
            else {
                size_t start = *tree_size;
                repetitions -= 3;
                while (true) {
                    tree[*tree_size] = BROTLI_REPEAT_ZERO_CODE_LENGTH;
                    extra_bits_data[*tree_size] = (byte) (repetitions & 0x7);
                    ++(*tree_size);
                    repetitions >>= 3;
                    if (repetitions == 0) {
                        break;
                    }
                    --repetitions;
                }
                Reverse(tree, start, *tree_size);
                Reverse(extra_bits_data, start, *tree_size);
            }
        }

        private static unsafe void BrotliWriteHuffmanTree(byte* depth,
            size_t length,
            size_t* tree_size,
            byte* tree,
            byte* extra_bits_data) {
            byte previous_value = BROTLI_INITIAL_REPEATED_CODE_LENGTH;
            size_t i;
            bool use_rle_for_non_zero = false;
            bool use_rle_for_zero = false;

            /* Throw away trailing zeros. */
            size_t new_length = length;
            for (i = 0; i < length; ++i) {
                if (depth[length - i - 1] == 0) {
                    --new_length;
                }
                else {
                    break;
                }
            }

            /* First gather statistics on if it is a good idea to do RLE. */
            if (length > 50) {
                /* Find RLE coding for longer codes.
                   Shorter codes seem not to benefit from RLE. */
                DecideOverRleUse(depth, new_length,
                    &use_rle_for_non_zero, &use_rle_for_zero);
            }

            /* Actual RLE coding. */
            for (i = 0; i < new_length;) {
                byte value = depth[i];
                size_t reps = 1;
                if ((value != 0 && use_rle_for_non_zero) ||
                    (value == 0 && use_rle_for_zero)) {
                    size_t k;
                    for (k = i + 1; k < new_length && depth[k] == value; ++k) {
                        ++reps;
                    }
                }
                if (value == 0) {
                    BrotliWriteHuffmanTreeRepetitionsZeros(
                        reps, tree_size, tree, extra_bits_data);
                }
                else {
                    BrotliWriteHuffmanTreeRepetitions(previous_value,
                        value, reps, tree_size,
                        tree, extra_bits_data);
                    previous_value = value;
                }
                i += reps;
            }
        }

        /* This function will create a Huffman tree.

       The catch here is that the tree cannot be arbitrarily deep.
       Brotli specifies a maximum depth of 15 bits for "code trees"
       and 7 bits for "code length code trees."

       count_limit is the value that is to be faked as the minimum value
       and this minimum value is raised until the tree matches the
       maximum length requirement.

       This algorithm is not of excellent performance for very long data blocks,
       especially when population counts are longer than 2**tree_limit, but
       we are not planning to use this with extremely long blocks.

       See http://en.wikipedia.org/wiki/Huffman_coding */
        private static unsafe void BrotliCreateHuffmanTree(uint* data,
            size_t length,
            int tree_limit,
            HuffmanTree* tree,
            byte* depth) {
            uint count_limit;
            HuffmanTree sentinel;
            InitHuffmanTree(&sentinel, uint.MaxValue, -1, -1);
            /* For block sizes below 64 kB, we never need to do a second iteration
               of this loop. Probably all of our block sizes will be smaller than
               that, so this loop is mostly of academic interest. If we actually
               would need this, we would be better off with the Katajainen algorithm. */
            for (count_limit = 1;; count_limit *= 2) {
                size_t n = 0;
                size_t i;
                size_t j;
                size_t k;
                for (i = length; i != 0;) {
                    --i;
                    if (data[i] != 0) {
                        uint count = Math.Max(data[i], count_limit);
                        InitHuffmanTree(&tree[n++], count, -1, (short) i);
                    }
                }

                if (n == 1) {
                    depth[tree[0].index_right_or_value_] = 1; /* Only one element. */
                    break;
                }

                SortHuffmanTreeItems(tree, n, SortHuffmanTreeEntropyEncode);

                /* The nodes are:
                   [0, n): the sorted leaf nodes that we start with.
                   [n]: we add a sentinel here.
                   [n + 1, 2n): new parent nodes are added here, starting from
                                (n+1). These are naturally in ascending order.
                   [2n]: we add a sentinel at the end as well.
                   There will be (2n+1) elements at the end. */
                tree[n] = sentinel;
                tree[n + 1] = sentinel;

                i = 0; /* Points to the next leaf node. */
                j = n + 1; /* Points to the next non-leaf node. */
                for (k = n - 1; k != 0; --k) {
                    size_t left, right;
                    if (tree[i].total_count_ <= tree[j].total_count_) {
                        left = i;
                        ++i;
                    }
                    else {
                        left = j;
                        ++j;
                    }
                    if (tree[i].total_count_ <= tree[j].total_count_) {
                        right = i;
                        ++i;
                    }
                    else {
                        right = j;
                        ++j;
                    }

                    {
                        /* The sentinel node becomes the parent node. */
                        size_t j_end = 2 * n - k;
                        tree[j_end].total_count_ =
                            tree[left].total_count_ + tree[right].total_count_;
                        tree[j_end].index_left_ = (short) left;
                        tree[j_end].index_right_or_value_ = (short) right;

                        /* Add back the last sentinel node. */
                        tree[j_end + 1] = sentinel;
                    }
                }
                if (BrotliSetDepth((int) (2 * n - 1), &tree[0], depth, tree_limit)) {
                    /* We need to pack the Huffman tree in tree_limit bits. If this was not
                       successful, add fake entities to the lowest values and retry. */
                    break;
                }
            }
        }

        private static unsafe void Reverse(byte* v, size_t start, size_t end) {
            --end;
            while (start < end) {
                byte tmp = v[start];
                v[start] = v[end];
                v[end] = tmp;
                ++start;
                --end;
            }
        }

        private static unsafe void BrotliWriteHuffmanTreeRepetitions(
            byte previous_value,
            byte value,
            size_t repetitions,
            size_t* tree_size,
            byte* tree,
            byte* extra_bits_data) {
            if (previous_value != value) {
                tree[*tree_size] = value;
                extra_bits_data[*tree_size] = 0;
                ++(*tree_size);
                --repetitions;
            }
            if (repetitions == 7) {
                tree[*tree_size] = value;
                extra_bits_data[*tree_size] = 0;
                ++(*tree_size);
                --repetitions;
            }
            if (repetitions < 3) {
                size_t i;
                for (i = 0; i < repetitions; ++i) {
                    tree[*tree_size] = value;
                    extra_bits_data[*tree_size] = 0;
                    ++(*tree_size);
                }
            }
            else {
                size_t start = *tree_size;
                repetitions -= 3;
                while (true) {
                    tree[*tree_size] = BROTLI_REPEAT_PREVIOUS_CODE_LENGTH;
                    extra_bits_data[*tree_size] = (byte) (repetitions & 0x3);
                    ++(*tree_size);
                    repetitions >>= 2;
                    if (repetitions == 0) {
                        break;
                    }
                    --repetitions;
                }
                Reverse(tree, start, *tree_size);
                Reverse(extra_bits_data, start, *tree_size);
            }
        }

        private static unsafe void BrotliOptimizeHuffmanCountsForRle(size_t length, uint* counts,
            byte* good_for_rle) {
            size_t nonzero_count = 0;
            size_t stride;
            size_t limit;
            size_t sum;
            size_t streak_limit = 1240;
            /* Let's make the Huffman code more compatible with RLE encoding. */
            size_t i;
            for (i = 0; i < length; i++) {
                if (counts[i] != 0) {
                    ++nonzero_count;
                }
            }
            if (nonzero_count < 16) {
                return;
            }
            while (length != 0 && counts[length - 1] == 0) {
                --length;
            }
            if (length == 0) {
                return; /* All zeros. */
            }
            /* Now counts[0..length - 1] does not have trailing zeros. */
            {
                size_t nonzeros = 0;
                uint smallest_nonzero = 1 << 30;
                for (i = 0; i < length; ++i) {
                    if (counts[i] != 0) {
                        ++nonzeros;
                        if (smallest_nonzero > counts[i]) {
                            smallest_nonzero = counts[i];
                        }
                    }
                }
                if (nonzeros < 5) {
                    /* Small histogram will model it well. */
                    return;
                }
                if (smallest_nonzero < 4) {
                    size_t zeros = length - nonzeros;
                    if (zeros < 6) {
                        for (i = 1; i < length - 1; ++i) {
                            if (counts[i - 1] != 0 && counts[i] == 0 && counts[i + 1] != 0) {
                                counts[i] = 1;
                            }
                        }
                    }
                }
                if (nonzeros < 28) {
                    return;
                }
            }
            /* 2) Let's mark all population counts that already can be encoded
               with an RLE code. */
            memset(good_for_rle, 0, length);
            {
                /* Let's not spoil any of the existing good RLE codes.
                   Mark any seq of 0's that is longer as 5 as a good_for_rle.
                   Mark any seq of non-0's that is longer as 7 as a good_for_rle. */
                uint symbol = counts[0];
                size_t step = 0;
                for (i = 0; i <= length; ++i) {
                    if (i == length || counts[i] != symbol) {
                        if ((symbol == 0 && step >= 5) ||
                            (symbol != 0 && step >= 7)) {
                            size_t k;
                            for (k = 0; k < step; ++k) {
                                good_for_rle[i - k - 1] = 1;
                            }
                        }
                        step = 1;
                        if (i != length) {
                            symbol = counts[i];
                        }
                    }
                    else {
                        ++step;
                    }
                }
            }
            /* 3) Let's replace those population counts that lead to more RLE codes.
               Math here is in 24.8 fixed point representation. */
            stride = 0;
            limit = 256 * (counts[0] + counts[1] + counts[2]) / 3 + 420;
            sum = 0;
            for (i = 0; i <= length; ++i) {
                if (i == length || good_for_rle[i] != 0 ||
                    (i != 0 && good_for_rle[i - 1] != 0) ||
                    (256 * counts[i] - limit + streak_limit) >= 2 * streak_limit) {
                    if (stride >= 4 || (stride >= 3 && sum == 0)) {
                        size_t k;
                        /* The stride must end, collapse what we have, if we have enough (4). */
                        size_t count = (sum + stride / 2) / stride;
                        if (count == 0) {
                            count = 1;
                        }
                        if (sum == 0) {
                            /* Don't make an all zeros stride to be upgraded to ones. */
                            count = 0;
                        }
                        for (k = 0; k < stride; ++k) {
                            /* We don't want to change value at counts[i],
                               that is already belonging to the next stride. Thus - 1. */
                            counts[i - k - 1] = (uint) count;
                        }
                    }
                    stride = 0;
                    sum = 0;
                    if (i < length - 2) {
                        /* All interesting strides have a count of at least 4, */
                        /* at least when non-zeros. */
                        limit = 256 * (counts[i] + counts[i + 1] + counts[i + 2]) / 3 + 420;
                    }
                    else if (i < length) {
                        limit = 256 * counts[i];
                    }
                    else {
                        limit = 0;
                    }
                }
                ++stride;
                if (i != length) {
                    sum += counts[i];
                    if (stride >= 4) {
                        limit = (256 * sum + stride / 2) / stride;
                    }
                    if (stride == 4) {
                        limit += 120;
                    }
                }
            }
        }

        /* Sort the root nodes, least popular first. */
        private static unsafe bool SortHuffmanTreeEntropyEncode(
            HuffmanTree* v0, HuffmanTree* v1) {
            if (v0->total_count_ != v1->total_count_) {
                return v0->total_count_ < v1->total_count_;
            }
            return v0->index_right_or_value_ > v1->index_right_or_value_;
        }

        private unsafe delegate bool HuffmanTreeComparator(HuffmanTree* v0, HuffmanTree* v1);

        private static unsafe void SortHuffmanTreeItems(HuffmanTree* items, size_t n,
            HuffmanTreeComparator comparator) {
            size_t[] gaps = {132, 57, 23, 10, 4, 1};
            if (n < 13) {
                /* Insertion sort. */
                size_t i;
                for (i = 1; i < n; ++i) {
                    HuffmanTree tmp = items[i];
                    size_t k = i;
                    size_t j = i - 1;
                    while (comparator(&tmp, &items[j])) {
                        items[k] = items[j];
                        k = j;
                        if (j-- == 0) break;
                    }
                    items[k] = tmp;
                }
            }
            else {
                /* Shell sort. */
                int g = n < 57 ? 2 : 0;
                for (; g < 6; ++g) {
                    size_t gap = gaps[g];
                    size_t i;
                    for (i = gap; i < n; ++i) {
                        size_t j = i;
                        HuffmanTree tmp = items[i];
                        for (; j >= gap && comparator(&tmp, &items[j - gap]); j -= gap) {
                            items[j] = items[j - gap];
                        }
                        items[j] = tmp;
                    }
                }
            }
        }
    }
}