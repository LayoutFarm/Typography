using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        // Use preprocessor directives to optimise away some of the IL to encourage 
        // inlining by JIT compiler
#if X64
        private const bool Is64Bit = true;
        private const int WordSize = 64;
#elif X86
        private const bool Is64Bit = false;
        private const int WordSize = 32;
#else
        private static readonly bool Is64Bit = IntPtr.Size == 8;
        private static readonly int WordSize = IntPtr.Size * 8;
#endif

        private static readonly size_t wsize = IntPtr.Size;
        private static readonly size_t wmask = IntPtr.Size - 1;

        // https://opensource.apple.com/source/ntp/ntp-12/ntp/libntp/memmove.c
        private static unsafe void memmove(void* destination, void* source, size_t length)
        {
            byte* src = (byte*)source;
            byte* dst = (byte*)destination;

            // Check that buffers do not overlap, if so do memcpy
            if (dst - src >= length && src - dst >= length)
            {
                memcpy(destination, source, length);
                return;
            }

            if (dst < src)
            {
                /*
                 * Copy forward.
                 */
                size_t t = (int)src; /* only need low bits */
                if (((t | (int)dst) & wmask) != 0)
                {
                    /*
			        * Try to align operands.  This cannot be done
			        * unless the low bits match.
			        */
                    if (((t ^ (int)dst) & wmask) != 0 || length < wsize)
                        t = length;
                    else
                        t = wsize - (t & wmask);
                    length -= t;
                    do
                    {
                        *dst++ = *src++;
                    } while (--t != 0);
                }
                /*
                 * Copy whole words, then mop up any trailing bytes.
                 */
                t = length / wsize;
                if (t != 0)
                {
                    do
                    {
                        *(size_t*)dst = *(size_t*)src;
                        src += wsize;
                        dst += wsize;
                    } while (--t != 0);
                }
                t = length & wmask;
                if (t != 0)
                {
                    do
                    {
                        *dst++ = *src++;
                    } while (--t != 0);
                }
            }
            else
            {
                /*
                 * Copy backwards.  Otherwise essentially the same.
                 * Alignment works as before, except that it takes
                 * (t&wmask) bytes to align, not wsize-(t&wmask).
                 */
                src += length;
                dst += length;
                size_t t = (int)src;
                if (((t | (int)dst) & wmask) != 0)
                {
                    if (((t ^ (int)dst) & wmask) != 0 || length <= wsize)
                        t = length;
                    else
                        t &= wmask;
                    length -= t;
                    do
                    {
                        *--dst = *--src;
                    } while (--t != 0);
                }
                t = length / wsize;
                if (t != 0)
                {
                    do
                    {
                        src -= wsize;
                        dst -= wsize;
                        *(size_t*)dst = *(size_t*)src;
                    } while (t-- != 0);
                }
                t = length & wmask;
                if (t != 0)
                {
                    do
                    {
                        *--dst = *--src;
                    } while (--t != 0);
                }
            }
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Buffer.cs
        private static unsafe void memcpy(void* destination, void* source, size_t length) {
#if !(NET20 || NET35 || NET40)
            System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(destination, source, length);
#else
            // This is portable version of memcpy. It mirrors what the hand optimized assembly versions of memcpy typically do.
            //
            // Ideally, we would just use the cpblk IL instruction here. Unfortunately, cpblk IL instruction is not as efficient as
            // possible yet and so we have this implementation here for now.

            // Note: It's important that this switch handles lengths at least up to 22.
            // See notes below near the main loop for why.

            // The switch will be very fast since it can be implemented using a jump
            // table in assembly. See http://stackoverflow.com/a/449297/4077294 for more info.
            var dest = (byte*) destination;
            var src = (byte*) source;
            var len = (uint) length;
            switch (len) {
                case 0:
                    return;
                case 1:
                    *dest = *src;
                    return;
                case 2:
                    *(short*) dest = *(short*) src;
                    return;
                case 3:
                    *(short*) dest = *(short*) src;
                    *(dest + 2) = *(src + 2);
                    return;
                case 4:
                    *(int*) dest = *(int*) src;
                    return;
                case 5:
                    *(int*) dest = *(int*) src;
                    *(dest + 4) = *(src + 4);
                    return;
                case 6:
                    *(int*) dest = *(int*) src;
                    *(short*) (dest + 4) = *(short*) (src + 4);
                    return;
                case 7:
                    *(int*) dest = *(int*) src;
                    *(short*) (dest + 4) = *(short*) (src + 4);
                    *(dest + 6) = *(src + 6);
                    return;
                case 8:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    return;
                case 9:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(dest + 8) = *(src + 8);
                    return;
                case 10:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(short*) (dest + 8) = *(short*) (src + 8);
                    return;
                case 11:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(short*) (dest + 8) = *(short*) (src + 8);
                    *(dest + 10) = *(src + 10);
                    return;
                case 12:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(int*) (dest + 8) = *(int*) (src + 8);
                    return;
                case 13:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(int*) (dest + 8) = *(int*) (src + 8);
                    *(dest + 12) = *(src + 12);
                    return;
                case 14:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(int*) (dest + 8) = *(int*) (src + 8);
                    *(short*) (dest + 12) = *(short*) (src + 12);
                    return;
                case 15:
                    if (Is64Bit)
                        *(long*) dest = *(long*) src;
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                    }
                    *(int*) (dest + 8) = *(int*) (src + 8);
                    *(short*) (dest + 12) = *(short*) (src + 12);
                    *(dest + 14) = *(src + 14);
                    return;
                case 16:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    return;
                case 17:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    *(dest + 16) = *(src + 16);
                    return;
                case 18:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    *(short*) (dest + 16) = *(short*) (src + 16);
                    return;
                case 19:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    *(short*) (dest + 16) = *(short*) (src + 16);
                    *(dest + 18) = *(src + 18);
                    return;
                case 20:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    *(int*) (dest + 16) = *(int*) (src + 16);
                    return;
                case 21:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    *(int*) (dest + 16) = *(int*) (src + 16);
                    *(dest + 20) = *(src + 20);
                    return;
                case 22:
                    if (Is64Bit) {
                        *(long*) dest = *(long*) src;
                        *(long*) (dest + 8) = *(long*) (src + 8);
                    }
                    else {
                        *(int*) dest = *(int*) src;
                        *(int*) (dest + 4) = *(int*) (src + 4);
                        *(int*) (dest + 8) = *(int*) (src + 8);
                        *(int*) (dest + 12) = *(int*) (src + 12);
                    }
                    *(int*) (dest + 16) = *(int*) (src + 16);
                    *(short*) (dest + 20) = *(short*) (src + 20);
                    return;
            }

            size_t i = 0; // byte offset at which we're copying

            if (((int) dest & 3) != 0) {
                if (((int) dest & 1) != 0) {
                    *(dest + i) = *(src + i);
                    i += 1;
                    if (((int) dest & 2) != 0)
                        goto IntAligned;
                }
                *(short*) (dest + i) = *(short*) (src + i);
                i += 2;
            }

            IntAligned:

            if (Is64Bit) {
                // On 64-bit IntPtr.Size == 8, so we want to advance to the next 8-aligned address. If
                // (int)dest % 8 is 0, 5, 6, or 7, we will already have advanced by 0, 3, 2, or 1
                // bytes to the next aligned address (respectively), so do nothing. On the other hand,
                // if it is 1, 2, 3, or 4 we will want to copy-and-advance another 4 bytes until
                // we're aligned.
                // The thing 1, 2, 3, and 4 have in common that the others don't is that if you
                // subtract one from them, their 3rd lsb will not be set. Hence, the below check.

                if ((((int) dest - 1) & 4) == 0) {
                    *(int*) (dest + i) = *(int*) (src + i);
                    i += 4;
                }
            } // BIT64

            var end = len - 16;
            len -= i; // lower 4 bits of len represent how many bytes are left *after* the unrolled loop

            // This is separated out into a different variable, so the i + 16 addition can be
            // performed at the start of the pipeline and the loop condition does not have
            // a dependency on the writes.
            uint counter;

            do {
                counter = i + 16;

                // This loop looks very costly since there appear to be a bunch of temporary values
                // being created with the adds, but the jit (for x86 anyways) will convert each of
                // these to use memory addressing operands.

                // So the only cost is a bit of code size, which is made up for by the fact that
                // we save on writes to dest/src.

                if (Is64Bit) {
                    *(long*) (dest + i) = *(long*) (src + i);
                    *(long*) (dest + i + 8) = *(long*) (src + i + 8);
                }
                else {
                    *(int*) (dest + i) = *(int*) (src + i);
                    *(int*) (dest + i + 4) = *(int*) (src + i + 4);
                    *(int*) (dest + i + 8) = *(int*) (src + i + 8);
                    *(int*) (dest + i + 12) = *(int*) (src + i + 12);
                }

                i = counter;

                // See notes above for why this wasn't used instead
                // i += 16;
            } while (counter <= end);

            if ((len & 8) != 0) {
                if (Is64Bit)
                    *(long*) (dest + i) = *(long*) (src + i);
                else {
                    *(int*) (dest + i) = *(int*) (src + i);
                    *(int*) (dest + i + 4) = *(int*) (src + i + 4);
                }
                i += 8;
            }
            if ((len & 4) != 0) {
                *(int*) (dest + i) = *(int*) (src + i);
                i += 4;
            }
            if ((len & 2) != 0) {
                *(short*) (dest + i) = *(short*) (src + i);
                i += 2;
            }
            if ((len & 1) != 0) {
                *(dest + i) = *(src + i);
                // We're not using i after this, so not needed
                // i += 1;
            }
#endif
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void memmove16(byte* dst, byte* src) {
            if (Is64Bit) {
                *(long*) dst = *(long*) src;
                *(long*) (dst + 8) = *(long*) (src + 8);
            }
            else {
                *(int*) dst = *(int*) src;
                *(int*) (dst + 4) = *(int*) (src + 4);
                *(int*) (dst + 8) = *(int*) (src + 8);
                *(int*) (dst + 12) = *(int*) (src + 12);
            }
        }

        // https://github.com/Smattr/memset
        private static unsafe void* memset(void* ptr, int value, size_t num) {
#if !(NET20 || NET35 || NET40)
            System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(ptr, (byte)value, num);
            return ptr;
#else
            size_t x = value & 0xff;
            var pp = (byte*) ptr;
            var xx = (byte) (value & 0xff);
            int i;
            for (i = 3; 1 << i < WordSize; ++i)
                x |= x << (1 << i);
            var bytes_per_word = 1 << (i - 3);

            /* Prologue. */
            while ((((uint) pp & (bytes_per_word - 1)) != 0) && (num-- != 0))
                *pp++ = xx;
            var tail = num & (bytes_per_word - 1);
            var p = (size_t*) pp;

            /* Main loop. */
            num >>= i - 3;
            while (num-- != 0)
                *p++ = x;

            /* Epilogue. */
            pp = (byte*) p;
            while (tail-- != 0)
                *pp++ = xx;

            return ptr;
#endif
        }

        [DebuggerDisplay("{Value}")]
        internal unsafe struct SizeT {
            private void* Value;

            public SizeT(void* v) {
                Value = v;
            }

            public static implicit operator UIntPtr(size_t s) {
                return (UIntPtr) s.Value;
            }

            public static implicit operator size_t(UIntPtr s) {
                return new size_t(s.ToPointer());
            }

            public static implicit operator ulong(size_t s) {
                return (ulong) s.Value;
            }

            public static implicit operator uint(size_t s) {
                return (uint) s.Value;
            }

            public static explicit operator ushort(size_t s) {
                return (ushort) s.Value;
            }

            public static explicit operator short(size_t s) {
                return (short) s.Value;
            }

            public static explicit operator long(size_t s) {
                return (long) s.Value;
            }

            public static explicit operator int(size_t s) {
                return (int) s.Value;
            }

            public static explicit operator byte(size_t s) {
                return (byte) s.Value;
            }

            public static explicit operator sbyte(size_t s) {
                return (sbyte) s.Value;
            }

            public static explicit operator void*(size_t s) {
                return s.Value;
            }

            public static explicit operator float(size_t s) {
                return Is64Bit ? (ulong)s : (uint)s;
            }

            public static explicit operator double(size_t s) {
                return Is64Bit ? (ulong)s : (uint)s;
            }

            public static implicit operator size_t(ushort i) {
                return new size_t((void*)i);
            }

            public static implicit operator size_t(byte i) {
                return new size_t((void*)i);
            }

            public static implicit operator size_t(int i) {
                return new size_t((void*) i);
            }

            public static implicit operator size_t(uint i) {
                return new size_t((void*) i);
            }

            public static implicit operator size_t(long i) {
                return new size_t((void*) i);
            }

            public static implicit operator size_t(ulong i) {
                return new size_t((void*) i);
            }

            public static explicit operator size_t(void* p) {
                return new size_t(p);
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static explicit operator size_t(double p) {
                return new size_t(Is64Bit ? (void*) (ulong) p : (void*) (uint) p);
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static explicit operator size_t(float p) {
                return new size_t(Is64Bit ? (void*)(ulong)p : (void*)(uint)p);
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator +(size_t a, size_t b) {
                return Is64Bit
                    ? new size_t((byte*) a + (ulong) b)
                    : new size_t((byte*) a + b);
            }

            public static size_t operator +(size_t a, int b) {
                return new size_t((byte*) a + b);
            }

            public static size_t operator +(size_t a, uint b) {
                return new size_t((byte*) a + b);
            }

            public static size_t operator +(size_t a, long b) {
                return new size_t((byte*) a + b);
            }

            public static size_t operator +(size_t a, ulong b) {
                return new size_t((byte*) a + b);
            }

            public static bool operator >(size_t a, size_t b) {
                return a.Value > b.Value;
            }

            public static bool operator >=(size_t a, size_t b) {
                return a.Value >= b.Value;
            }

            public static bool operator <=(size_t a, size_t b) {
                return a.Value <= b.Value;
            }

            public static bool operator <(size_t a, size_t b) {
                return a.Value < b.Value;
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator -(size_t a, size_t b) {
                return Is64Bit
                    ? new size_t((byte*) a - (ulong) b)
                    : new size_t((byte*) a - b);
            }

            public static size_t operator -(size_t a, int b) {
                return new size_t((byte*) a.Value - b);
            }

            public static size_t operator -(size_t a, uint b) {
                return new size_t((byte*) a.Value - b);
            }

            public static size_t operator -(size_t a, long b) {
                return new size_t((byte*) a.Value - b);
            }

            public static size_t operator -(size_t a, ulong b) {
                return new size_t((byte*) a.Value - b);
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator /(size_t a, size_t b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a / (ulong) b) : (void*) ((uint) a / (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator /(size_t a, int b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a / (uint) b) : (void*) ((uint) a / (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator /(size_t a, uint b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a / b) : (void*) ((uint) a / b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator /(size_t a, long b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a / (ulong) b) : (void*) ((uint) a / (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator /(size_t a, ulong b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a / b) : (void*) ((uint) a / (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator *(size_t a, size_t b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a * (ulong) b) : (void*) ((uint) a * (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator *(size_t a, int b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a * (ulong) b) : (void*) ((uint) a * (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator *(size_t a, uint b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a * b) : (void*) ((uint) a * b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator *(size_t a, long b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a * (ulong) b) : (void*) ((uint) a * (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator *(size_t a, ulong b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a * b) : (void*) ((uint) a * (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator %(size_t a, size_t b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a % (ulong) b) : (void*) ((uint) a % (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator %(size_t a, int b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a % (ulong) b) : (void*) ((uint) a % (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

            public static size_t operator %(size_t a, uint b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a % b) : (void*) ((uint) a % b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator %(size_t a, long b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a % (ulong) b) : (void*) ((uint) a % (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator %(size_t a, ulong b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a % b) : (void*) ((uint) a % (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator &(size_t a, size_t b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a & (ulong) b) : (void*) ((uint) a & (uint) b));
            }

            public static size_t operator &(size_t a, int b) {
                return new size_t((void*) ((uint)a & (uint) b));
            }

            public static size_t operator &(size_t a, uint b) {
                return new size_t((void*)((uint)a & b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator &(size_t a, long b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a & (uint) b) : (void*) ((uint) a & (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator &(size_t a, ulong b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a & b) : (void*) ((uint) a & b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator |(size_t a, size_t b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a | (ulong) b) : (void*) ((uint) a | (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator |(size_t a, int b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a | (uint) b) : (void*) ((uint) a | (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator |(size_t a, uint b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a | b) : (void*) ((uint) a | b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator |(size_t a, long b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a | (uint) b) : (void*) ((uint) a | (uint) b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator |(size_t a, ulong b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a | b) : (void*) ((uint) a | b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator >>(size_t a, int b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a >> b) : (void*) ((uint) a >> b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator <<(size_t a, int b) {
                return new size_t(Is64Bit ? (void*) ((ulong) a << b) : (void*) ((uint) a << b));
            }

#if AGGRESSIVE_INLINING
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public static size_t operator ~(size_t a)
            {
                return new size_t(Is64Bit ? (void*)(~(ulong) a) : (void*)(~(uint)a));
            }

            public override string ToString()
            {
                return Is64Bit ? ((ulong)Value).ToString() : ((uint)Value).ToString();
            }
        }
    }
}