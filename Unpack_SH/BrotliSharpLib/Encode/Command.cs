using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {

        private static readonly uint[] kInsBase = {
            0, 1, 2, 3, 4, 5, 6, 8, 10, 14, 18, 26, 34, 50,
            66, 98, 130, 194, 322, 578, 1090, 2114, 6210, 22594
        };

        private static readonly uint[] kInsExtra = {
            0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4,
            5, 5, 6, 7, 8, 9, 10, 12, 14, 24
        };

        private static readonly uint[] kCopyBase = {
            2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 18, 22, 30,
            38, 54,  70, 102, 134, 198, 326,   582, 1094,  2118
        };

        private static readonly uint[] kCopyExtra = {
            0, 0, 0, 0, 0, 0, 0, 0,  1,  1,  2,  2,  3,  3,
            4,  4,   5,   5,   6,   7,   8,     9,   10,    24
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct Command {
            public uint insert_len_;

            /* Stores copy_len in low 24 bits and copy_len XOR copy_code in high 8 bit. */
            public uint copy_len_;

            public uint dist_extra_;
            public ushort cmd_prefix_;
            public ushort dist_prefix_;
        }

        private static ushort GetInsertLengthCode(size_t insertlen)
        {
            if (insertlen < 6)
            {
                return (ushort)insertlen;
            }
            else if (insertlen < 130)
            {
                uint nbits = Log2FloorNonZero(insertlen - 2) - 1u;
                return (ushort)((nbits << 1) + ((insertlen - 2) >> (int) nbits) + 2);
            }
            else if (insertlen < 2114)
            {
                return (ushort)(Log2FloorNonZero(insertlen - 66) + 10);
            }
            else if (insertlen < 6210)
            {
                return 21;
            }
            else if (insertlen < 22594)
            {
                return 22;
            }
            else
            {
                return 23;
            }
        }

        private static unsafe ushort GetCopyLengthCode(size_t copylen)
        {
            if (copylen < 10)
            {
                return (ushort)(copylen - 2);
            }
            else if (copylen < 134)
            {
                uint nbits = Log2FloorNonZero(copylen - 6) - 1u;
                return (ushort)((nbits << 1) + ((copylen - 6) >> (int) nbits) + 4);
            }
            else if (copylen < 2118)
            {
                return (ushort)(Log2FloorNonZero(copylen - 70) + 12);
            }
            else
            {
                return 23;
            }
        }

        private static ushort CombineLengthCodes(
            ushort inscode, ushort copycode, bool use_last_distance)
        {
            ushort bits64 =
                (ushort)((copycode & 0x7u) | ((inscode & 0x7u) << 3));
            if (use_last_distance && inscode < 8 && copycode < 16)
            {
                return (ushort) ((copycode < 8) ? bits64 : (bits64 | 64));
            }
            else
            {
                /* Specification: 5 Encoding of ... (last table) */
                /* offset = 2 * index, where index is in range [0..8] */
                int offset = 2 * ((copycode >> 3) + 3 * (inscode >> 3));
                /* All values in specification are K * 64,
                   where   K = [2, 3, 6, 4, 5, 8, 7, 9, 10],
                       i + 1 = [1, 2, 3, 4, 5, 6, 7, 8,  9],
                   K - i - 1 = [1, 1, 3, 0, 0, 2, 0, 1,  2] = D.
                   All values in D require only 2 bits to encode.
                   Magic ant is shifted 6 bits left, to avoid final multiplication. */
                offset = (offset << 5) + 0x40 + ((0x520D40 >> offset) & 0xC0);
                return (ushort) ((ushort)offset | bits64);
            }
        }

        /* distance_code is e.g. 0 for same-as-last short code, or 16 for offset 1. */
        private static unsafe void InitCommand(Command* self, size_t insertlen,
            size_t copylen, size_t copylen_code, size_t distance_code)
        {
            self->insert_len_ = (uint)insertlen;
            self->copy_len_ = (uint)(copylen | ((copylen_code ^ copylen) << 24));
            /* The distance prefix and extra bits are stored in this Command as if
               npostfix and ndirect were 0, they are only recomputed later after the
               clustering if needed. */
            PrefixEncodeCopyDistance(
                distance_code, 0, 0, &self->dist_prefix_, &self->dist_extra_);
            GetLengthCode(
                insertlen, copylen_code, (self->dist_prefix_ == 0),
                &self->cmd_prefix_);
        }

        private static unsafe void GetLengthCode(size_t insertlen, size_t copylen,
            bool use_last_distance,
            ushort* code)
        {
            ushort inscode = GetInsertLengthCode(insertlen);
            ushort copycode = GetCopyLengthCode(copylen);
            *code = CombineLengthCodes(inscode, copycode, use_last_distance);
        }

        private static unsafe uint CommandRestoreDistanceCode(Command* self)
        {
            if (self->dist_prefix_ < BROTLI_NUM_DISTANCE_SHORT_CODES)
            {
                return self->dist_prefix_;
            }
            else
            {
                uint nbits = self->dist_extra_ >> 24;
                uint extra = self->dist_extra_ & 0xffffff;
                /* It is assumed that the distance was first encoded with NPOSTFIX = 0 and
                   NDIRECT = 0, so the code itself is of this form:
                     BROTLI_NUM_DISTANCE_SHORT_CODES + 2 * (nbits - 1) + prefix_bit
                   Therefore, the following expression results in (2 + prefix_bit). */
                uint prefix =
                    self->dist_prefix_ + 4u - BROTLI_NUM_DISTANCE_SHORT_CODES - 2u * nbits;
                /* Subtract 4 for offset (Chapter 4.) and
                   increase by BROTLI_NUM_DISTANCE_SHORT_CODES - 1  */
                return (prefix << (int) nbits) + extra + BROTLI_NUM_DISTANCE_SHORT_CODES - 4u;
            }
        }

        private static unsafe uint CommandCopyLenCode(Command* self)
        {
            return (self->copy_len_ & 0xFFFFFF) ^ (self->copy_len_ >> 24);
        }

        private static unsafe void InitInsertCommand(Command* self, size_t insertlen)
        {
            self->insert_len_ = (uint)insertlen;
            self->copy_len_ = 4 << 24;
            self->dist_extra_ = 0;
            self->dist_prefix_ = BROTLI_NUM_DISTANCE_SHORT_CODES;
            GetLengthCode(insertlen, 4, false, &self->cmd_prefix_);
        }

        private static unsafe uint CommandDistanceContext(Command* self)
        {
            uint r = (uint) (self->cmd_prefix_ >> 6);
            uint c = (uint) (self->cmd_prefix_ & 7);
            if ((r == 0 || r == 2 || r == 4 || r == 7) && (c <= 2))
            {
                return c;
            }
            return 3;
        }

        private static unsafe uint CommandCopyLen(Command* self) {
            return self->copy_len_ & 0xFFFFFF;
        }

        private static uint GetInsertExtra(ushort inscode) {
            return kInsExtra[inscode];
        }

        private static uint GetCopyExtra(ushort copycode) {
            return kCopyExtra[copycode];
        }

        private static uint GetInsertBase(ushort inscode)
        {
            return kInsBase[inscode];
        }

        private static uint GetCopyBase(ushort copycode)
        {
            return kCopyBase[copycode];
        }
    }
}