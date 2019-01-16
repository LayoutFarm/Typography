using System.Runtime.CompilerServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using reg_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        /* Returns reverse(num >> BROTLI_REVERSE_BITS_BASE, BROTLI_REVERSE_BITS_MAX),
           where reverse(value, len) is the bit-wise reversal of the len least
           significant bits of value. */
        private static reg_t BrotliReverseBits(reg_t num) {
            return (uint) kReverseBits[num];
        }

        /* Stores code in table[0], table[step], table[2*step], ..., table[end] */
        /* Assumes that end is an integer multiple of step */
        private static unsafe void ReplicateValue(HuffmanCode* table,
            int step, int end,
            HuffmanCode code) {
            do {
                end -= step;
                table[end] = code;
            } while (end > 0);
        }

        /* Returns the table width of the next 2nd level table. count is the histogram
           of bit lengths for the remaining symbols, len is the code length of the next
           processed symbol */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe int NextTableBitSize(ushort* count,
            int len, int root_bits) {
            var l = len; // JIT ETW (Inlinee writes to an argument)
            var left = 1 << (l - root_bits);
            while (l < BROTLI_HUFFMAN_MAX_CODE_LENGTH) {
                left -= count[l];
                if (left <= 0) break;
                ++l;
                left <<= 1;
            }
            return l - root_bits;
        }

        private static unsafe void BrotliBuildCodeLengthsHuffmanTable(HuffmanCode* table,
            byte* code_lengths,
            ushort* count) {
            HuffmanCode code; /* current table entry */
            int symbol; /* symbol index in original or sorted table */
            reg_t key; /* prefix code */
            reg_t key_step; /* prefix code addend */
            int step; /* step size to replicate values in current table */
            int table_size; /* size of current table */
            var sorted = new int[BROTLI_CODE_LENGTH_CODES]; /* symbols sorted by code length */
            /* offsets in sorted table for each length */
            var offset = new int[BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH + 1];
            int bits;
            int bits_count;

            /* generate offsets into sorted symbol table by code length */
            symbol = -1;
            bits = 1;
            for (var i = 1; i <= 4; i *= 2) {
                if ((BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH & i) != 0) {
                    for (var x = 0; x < i; x++) {
                        symbol += count[bits];
                        offset[bits] = symbol;
                        bits++;
                    }
                }
            }

            /* Symbols with code length 0 are placed after all other symbols. */
            offset[0] = BROTLI_CODE_LENGTH_CODES - 1;

            /* sort symbols by length, by symbol order within each length */
            symbol = BROTLI_CODE_LENGTH_CODES;
            do {
                for (var i = 1; i <= 4; i *= 2) {
                    if ((6 & i) != 0) {
                        for (var x = 0; x < i; x++) {
                            symbol--;
                            sorted[offset[code_lengths[symbol]]--] = symbol;
                        }
                    }
                }
            } while (symbol != 0);

            table_size = 1 << BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH;

            /* Special case: all symbols but one have 0 code length. */
            if (offset[0] == 0) {
                code.bits = 0;
                code.value = (ushort) sorted[0];
                for (key = 0; key < (reg_t) table_size; ++key) {
                    table[key] = code;
                }
                return;
            }

            /* fill in table */
            key = 0;
            key_step = BROTLI_REVERSE_BITS_LOWEST;
            symbol = 0;
            bits = 1;
            step = 2;
            do {
                code.bits = (byte) bits;
                for (bits_count = count[bits]; bits_count != 0; --bits_count) {
                    code.value = (ushort) sorted[symbol++];
                    ReplicateValue(&table[BrotliReverseBits(key)], step, table_size, code);
                    key += key_step;
                }
                step <<= 1;
                key_step >>= 1;
            } while (++bits <= BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH);
        }

        private static unsafe uint BrotliBuildHuffmanTable(HuffmanCode* root_table,
            int root_bits,
            ushort* symbol_lists,
            ushort* count) {
            HuffmanCode code; /* current table entry */
            HuffmanCode* table; /* next available space in table */
            int len; /* current code length */
            int symbol; /* symbol index in original or sorted table */
            reg_t key; /* prefix code */
            reg_t key_step; /* prefix code addend */
            reg_t sub_key; /* 2nd level table prefix code */
            reg_t sub_key_step; /* 2nd level table prefix code addend */
            int step; /* step size to replicate values in current table */
            int table_bits; /* key length of current table */
            int table_size; /* size of current table */
            int total_size; /* sum of root table size and 2nd level table sizes */
            var max_length = -1;
            int bits;
            int bits_count;

            while (symbol_lists[max_length] == 0xFFFF) max_length--;
            max_length += BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1;

            table = root_table;
            table_bits = root_bits;
            table_size = 1 << table_bits;
            total_size = table_size;

            /* fill in root table */
            /* let's reduce the table size to a smaller size if possible, and */
            /* create the repetitions by memcpy if possible in the coming loop */
            if (table_bits > max_length) {
                table_bits = max_length;
                table_size = 1 << table_bits;
            }
            key = 0;
            key_step = BROTLI_REVERSE_BITS_LOWEST;
            bits = 1;
            step = 2;
            do {
                code.bits = (byte) bits;
                symbol = bits - (BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1);
                for (bits_count = count[bits]; bits_count != 0; --bits_count) {
                    symbol = symbol_lists[symbol];
                    code.value = (ushort) symbol;
                    ReplicateValue(&table[BrotliReverseBits(key)], step, table_size, code);
                    key += key_step;
                }
                step <<= 1;
                key_step >>= 1;
            } while (++bits <= table_bits);

            /* if root_bits != table_bits we only created one fraction of the */
            /* table, and we need to replicate it now. */
            while (total_size != table_size) {
                memcpy(&table[table_size], &table[0],
                    (size_t) table_size * sizeof(HuffmanCode));
                table_size <<= 1;
            }

            /* fill in 2nd level tables and add pointers to root table */
            key_step = BROTLI_REVERSE_BITS_LOWEST >> (root_bits - 1);
            sub_key = (BROTLI_REVERSE_BITS_LOWEST << 1);
            sub_key_step = BROTLI_REVERSE_BITS_LOWEST;
            for (len = root_bits + 1, step = 2; len <= max_length; ++len) {
                symbol = len - (BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1);
                for (; count[len] != 0; --count[len]) {
                    if (sub_key == (BROTLI_REVERSE_BITS_LOWEST << 1)) {
                        table += table_size;
                        table_bits = NextTableBitSize(count, len, root_bits);
                        table_size = 1 << table_bits;
                        total_size += table_size;
                        sub_key = BrotliReverseBits(key);
                        key += key_step;
                        root_table[sub_key].bits = (byte) (table_bits + root_bits);
                        root_table[sub_key].value =
                            (ushort) ((size_t) (table - root_table) - sub_key);
                        sub_key = 0;
                    }
                    code.bits = (byte) (len - root_bits);
                    symbol = symbol_lists[symbol];
                    code.value = (ushort) symbol;
                    ReplicateValue(
                        &table[BrotliReverseBits(sub_key)], step, table_size, code);
                    sub_key += sub_key_step;
                }
                step <<= 1;
                sub_key_step >>= 1;
            }
            return (uint) total_size;
        }

        private static unsafe uint BrotliBuildSimpleHuffmanTable(HuffmanCode* table,
            int root_bits,
            ushort* val,
            uint num_symbols) {
            uint table_size = 1;
            var goal_size = 1U << root_bits;
            switch (num_symbols) {
                case 0:
                    table[0].bits = 0;
                    table[0].value = val[0];
                    break;
                case 1:
                    table[0].bits = 1;
                    table[1].bits = 1;
                    if (val[1] > val[0]) {
                        table[0].value = val[0];
                        table[1].value = val[1];
                    }
                    else {
                        table[0].value = val[1];
                        table[1].value = val[0];
                    }
                    table_size = 2;
                    break;
                case 2:
                    table[0].bits = 1;
                    table[0].value = val[0];
                    table[2].bits = 1;
                    table[2].value = val[0];
                    if (val[2] > val[1]) {
                        table[1].value = val[1];
                        table[3].value = val[2];
                    }
                    else {
                        table[1].value = val[2];
                        table[3].value = val[1];
                    }
                    table[1].bits = 2;
                    table[3].bits = 2;
                    table_size = 4;
                    break;
                case 3: {
                    int i, k;
                    for (i = 0; i < 3; ++i) {
                        for (k = i + 1; k < 4; ++k) {
                            if (val[k] < val[i]) {
                                var t = val[k];
                                val[k] = val[i];
                                val[i] = t;
                            }
                        }
                    }
                    for (i = 0; i < 4; ++i) {
                        table[i].bits = 2;
                    }
                    table[0].value = val[0];
                    table[2].value = val[1];
                    table[1].value = val[2];
                    table[3].value = val[3];
                    table_size = 4;
                    break;
                }
                case 4: {
                    int i;
                    if (val[3] < val[2]) {
                        var t = val[3];
                        val[3] = val[2];
                        val[2] = t;
                    }
                    for (i = 0; i < 7; ++i) {
                        table[i].value = val[0];
                        table[i].bits = (byte) (1 + (i & 1));
                    }
                    table[1].value = val[1];
                    table[3].value = val[2];
                    table[5].value = val[1];
                    table[7].value = val[3];
                    table[3].bits = 3;
                    table[7].bits = 3;
                    table_size = 8;
                    break;
                }
            }
            while (table_size != goal_size) {
                memcpy(&table[table_size], &table[0],
                    (size_t) table_size * sizeof(HuffmanCode));
                table_size <<= 1;
            }
            return goal_size;
        }
    }
}