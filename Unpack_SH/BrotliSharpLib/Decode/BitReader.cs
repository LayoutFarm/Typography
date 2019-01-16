using System;
using System.Runtime.CompilerServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using reg_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private static uint BitMask(uint n) {
            return kBitMask[n];
        }

        private static unsafe void BrotliInitBitReader(BrotliBitReader* br) {
            br->val_ = 0;
            br->bit_pos_ = (uint) (IntPtr.Size << 3);
        }

        private static unsafe bool BrotliWarmupBitReader(BrotliBitReader* br) {
            size_t aligned_read_mask = (IntPtr.Size >> 1) - 1;
            /* Fixing alignment after unaligned BrotliFillWindow would result accumulator
               overflow. If unalignment is caused by BrotliSafeReadBits, then there is
               enough space in accumulator to fix alignment. */
            if (!BROTLI_ALIGNED_READ) {
                aligned_read_mask = 0;
            }
            if (BrotliGetAvailableBits(br) == 0) {
                if (!BrotliPullByte(br)) {
                    return false;
                }
            }

            while ((((size_t) br->next_in) & aligned_read_mask) != 0) {
                if (!BrotliPullByte(br)) {
                    /* If we consumed all the input, we don't care about the alignment. */
                    return true;
                }
            }
            return true;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void BrotliBitReaderSaveState(
            BrotliBitReader* from, BrotliBitReader* to) {
            to->val_ = from->val_;
            to->bit_pos_ = from->bit_pos_;
            to->next_in = from->next_in;
            to->avail_in = from->avail_in;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void BrotliBitReaderRestoreState(
            BrotliBitReader* to, BrotliBitReader* from) {
            to->val_ = from->val_;
            to->bit_pos_ = from->bit_pos_;
            to->next_in = from->next_in;
            to->avail_in = from->avail_in;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe uint BrotliGetAvailableBits(BrotliBitReader* br) {
            return (uint) ((Is64Bit ? 64 : 32) - br->bit_pos_);
        }

        /* Returns amount of unread bytes the bit reader still has buffered from the
        BrotliInput, including whole bytes in br->val_. */
        private static unsafe size_t BrotliGetRemainingBytes(BrotliBitReader* br) {
            return br->avail_in + (BrotliGetAvailableBits(br) >> 3);
        }

        /* Checks if there is at least |num| bytes left in the input ring-buffer
           (excluding the bits remaining in br->val_). */
        private static unsafe bool BrotliCheckInputAmount(
            BrotliBitReader* br, size_t num) {
            return br->avail_in >= num;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe ushort BrotliLoad16LE(byte* bIn) {
            if (BROTLI_LITTLE_ENDIAN) {
                return *(ushort*) bIn;
            }
            if (BROTLI_BIG_ENDIAN) {
                ushort value = *((ushort*)bIn);
                return (ushort)(((value & 0xFFU) << 8) | ((value & 0xFF00U) >> 8));
            }
            return (ushort)(bIn[0] | (bIn[1] << 8));
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe uint BrotliLoad32LE(byte* bIn) {
            if (BROTLI_LITTLE_ENDIAN) {
                return *(uint*) bIn;
            }
            if (BROTLI_BIG_ENDIAN) {
                uint value = *((uint*) bIn);
                return ((value & 0xFFU) << 24) | ((value & 0xFF00U) << 8) |
                       ((value & 0xFF0000U) >> 8) | ((value & 0xFF000000U) >> 24);
            }
            else {
                uint value = (uint) (*(bIn++));
                value |= (uint) (*(bIn++)) << 8;
                value |= (uint) (*(bIn++)) << 16;
                value |= (uint) (*(bIn++)) << 24;
                return value;
            }
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe ulong BrotliLoad64LE(byte* bIn) {
            if (BROTLI_LITTLE_ENDIAN) {
                return *(ulong*) bIn;
            }
            if (BROTLI_BIG_ENDIAN) {
                ulong value = *((ulong*)bIn);
                return
                    ((value & 0xFFU) << 56) |
                    ((value & 0xFF00U) << 40) |
                    ((value & 0xFF0000U) << 24) |
                    ((value & 0xFF000000U) << 8) |
                    ((value & 0xFF00000000U) >> 8) |
                    ((value & 0xFF0000000000U) >> 24) |
                    ((value & 0xFF000000000000U) >> 40) |
                    ((value & 0xFF00000000000000U) >> 56);
            }
            else {
                ulong value = (ulong)(*(bIn++));
                value |= (ulong)(*(bIn++)) << 8;
                value |= (ulong)(*(bIn++)) << 16;
                value |= (ulong)(*(bIn++)) << 24;
                value |= (ulong)(*(bIn++)) << 32;
                value |= (ulong)(*(bIn++)) << 40;
                value |= (ulong)(*(bIn++)) << 48;
                value |= (ulong)(*(bIn++)) << 56;
                return value;
            }
        }

        /* Guarantees that there are at least n_bits + 1 bits in accumulator.
           Precondition: accumulator contains at least 1 bit.
           n_bits should be in the range [1..24] for regular build. For portable
           non-64-bit little-endian build only 16 bits are safe to request. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void BrotliFillBitWindow(
            BrotliBitReader* br, uint n_bits) {
            if (Is64Bit) {
                if (!BROTLI_ALIGNED_READ && n_bits != 0 && (n_bits <= 8)) {
                    if (br->bit_pos_ >= 56) {
                        br->val_ >>= 56;
                        br->bit_pos_ ^= 56; /* here same as -= 56 because of the if condition */
                        br->val_ |= BrotliLoad64LE(br->next_in) << 8;
                        br->avail_in -= 7;
                        br->next_in += 7;
                    }
                }
                else if (!BROTLI_ALIGNED_READ && n_bits != 0 && (n_bits <= 16)) {
                    if (br->bit_pos_ >= 48) {
                        br->val_ >>= 48;
                        br->bit_pos_ ^= 48; /* here same as -= 48 because of the if condition */
                        br->val_ |= BrotliLoad64LE(br->next_in) << 16;
                        br->avail_in -= 6;
                        br->next_in += 6;
                    }
                }
                else {
                    if (br->bit_pos_ >= 32) {
                        br->val_ >>= 32;
                        br->bit_pos_ ^= 32; /* here same as -= 32 because of the if condition */
                        br->val_ |= ((ulong) BrotliLoad32LE(br->next_in)) << 32;
                        br->avail_in -= BROTLI_SHORT_FILL_BIT_WINDOW_READ;
                        br->next_in += BROTLI_SHORT_FILL_BIT_WINDOW_READ;
                    }
                }
            }
            else {
                if (!BROTLI_ALIGNED_READ && n_bits != 0 && (n_bits <= 8)) {
                    if (br->bit_pos_ >= 24) {
                        br->val_ >>= 24;
                        br->bit_pos_ ^= 24; /* here same as -= 24 because of the if condition */
                        br->val_ |= BrotliLoad32LE(br->next_in) << 8;
                        br->avail_in -= 3;
                        br->next_in += 3;
                    }
                }
                else {
                    if (br->bit_pos_ >= 16) {
                        br->val_ >>= 16;
                        br->bit_pos_ ^= 16; /* here same as -= 16 because of the if condition */
                        br->val_ |= ((uint) BrotliLoad16LE(br->next_in)) << 16;
                        br->avail_in -= BROTLI_SHORT_FILL_BIT_WINDOW_READ;
                        br->next_in += BROTLI_SHORT_FILL_BIT_WINDOW_READ;
                    }
                }
            }
        }

        /* Mostly like BrotliFillBitWindow, but guarantees only 16 bits and reads no
           more than BROTLI_SHORT_FILL_BIT_WINDOW_READ bytes of input. */
        private static unsafe void BrotliFillBitWindow16(BrotliBitReader* br) {
            BrotliFillBitWindow(br, 17);
        }

        /* Pulls one byte of input to accumulator. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool BrotliPullByte(BrotliBitReader* br) {
            if (br->avail_in == 0) {
                return false;
            }
            br->val_ >>= 8;
            if (Is64Bit) {
                br->val_ |= ((ulong) *br->next_in) << 56;
            }
            else {
                br->val_ |= ((uint) *br->next_in) << 24;
            }
            br->bit_pos_ -= 8;
            --br->avail_in;
            ++br->next_in;
            return true;
        }

        /* Returns currently available bits.
        The number of valid bits could be calculated by BrotliGetAvailableBits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe reg_t BrotliGetBitsUnmasked(BrotliBitReader* br) {
            return br->val_ >> (int) br->bit_pos_;
        }

        /* Like BrotliGetBits, but does not mask the result.
        The result contains at least 16 valid bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe uint BrotliGet16BitsUnmasked(
            BrotliBitReader* br) {
            BrotliFillBitWindow(br, 16);
            return BrotliGetBitsUnmasked(br);
        }

        /* Returns the specified number of bits from |br| without advancing bit pos. */
        private static unsafe uint BrotliGetBits(
            BrotliBitReader* br, uint n_bits) {
            BrotliFillBitWindow(br, n_bits);
            return (uint) (BrotliGetBitsUnmasked(br) & BitMask(n_bits));
        }

        /* Tries to peek the specified amount of bits. Returns 0, if there is not
           enough input. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool BrotliSafeGetBits(
            BrotliBitReader* br, uint n_bits, uint* val) {
            while (BrotliGetAvailableBits(br) < n_bits) {
                if (!BrotliPullByte(br)) {
                    return false;
                }
            }
            *val = (uint) (BrotliGetBitsUnmasked(br) & BitMask(n_bits));
            return true;
        }

        /* Advances the bit pos by n_bits. */
        private static unsafe void BrotliDropBits(
            BrotliBitReader* br, uint n_bits) {
            br->bit_pos_ += n_bits;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void BrotliBitReaderUnload(BrotliBitReader* br) {
            var unused_bytes = BrotliGetAvailableBits(br) >> 3;
            var unused_bits = unused_bytes << 3;
            br->avail_in += unused_bytes;
            br->next_in -= unused_bytes;
            if (unused_bits == IntPtr.Size << 3) {
                br->val_ = 0;
            }
            else {
                br->val_ <<= (int) unused_bits;
            }
            br->bit_pos_ += unused_bits;
        }

        /* Reads the specified number of bits from |br| and advances the bit pos.
           Precondition: accumulator MUST contain at least n_bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void BrotliTakeBits(
            BrotliBitReader* br, uint n_bits, uint* val) {
            *val = BrotliGetBitsUnmasked(br) & BitMask(n_bits);
            BrotliDropBits(br, n_bits);
        }

        /* Reads the specified number of bits from |br| and advances the bit pos.
        Assumes that there is enough input to perform BrotliFillBitWindow. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe uint BrotliReadBits(
            BrotliBitReader* br, uint n_bits) {
            if (Is64Bit || (n_bits <= 16)) {
                uint val;
                BrotliFillBitWindow(br, n_bits);
                BrotliTakeBits(br, n_bits, &val);
                return val;
            }
            else {
                uint low_val;
                uint high_val;
                BrotliFillBitWindow(br, 16);
                BrotliTakeBits(br, 16, &low_val);
                BrotliFillBitWindow(br, 8);
                BrotliTakeBits(br, n_bits - 16, &high_val);
                return low_val | (high_val << 16);
            }
        }

        /* Tries to read the specified amount of bits. Returns 0, if there is not
           enough input. n_bits MUST be positive. */
        private static unsafe bool BrotliSafeReadBits(
            BrotliBitReader* br, uint n_bits, uint* val) {
            while (BrotliGetAvailableBits(br) < n_bits) {
                if (!BrotliPullByte(br)) {
                    return false;
                }
            }
            BrotliTakeBits(br, n_bits, val);
            return true;
        }

        /* Advances the bit reader position to the next byte boundary and verifies
           that any skipped bits are set to zero. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool BrotliJumpToByteBoundary(BrotliBitReader* br) {
            var pad_bits_count = BrotliGetAvailableBits(br) & 0x7;
            uint pad_bits = 0;
            if (pad_bits_count != 0) {
                BrotliTakeBits(br, pad_bits_count, &pad_bits);
            }
            return pad_bits == 0;
        }

        /* Copies remaining input bytes stored in the bit reader to the output. Value
           num may not be larger than BrotliGetRemainingBytes. The bit reader must be
           warmed up again after this. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void BrotliCopyBytes(byte* dest,
            BrotliBitReader* br, size_t num) {
            while (BrotliGetAvailableBits(br) >= 8 && num > 0) {
                *dest = (byte) BrotliGetBitsUnmasked(br);
                BrotliDropBits(br, 8);
                ++dest;
                --num;
            }
            memcpy(dest, br->next_in, num);
            br->avail_in -= num;
            br->next_in += num;
        }
    }
}