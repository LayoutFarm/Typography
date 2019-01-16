using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct RingBuffer {
            /* Size of the ring-buffer is (1 << window_bits) + tail_size_. */
            public uint size_;

            public uint mask_;
            public uint tail_size_;
            public uint total_size_;

            public uint cur_size_;

            /* Position to write in the ring buffer. */
            public uint pos_;

            /* The actual ring buffer containing the copy of the last two bytes, the data,
               and the copy of the beginning as a tail. */
            public byte* data_;

            /* The start of the ring-buffer. */
            public byte* buffer_;
        }

        private static unsafe void RingBufferInit(ref RingBuffer rb) {
            rb.cur_size_ = 0;
            rb.pos_ = 0;
            rb.data_ = null;
            rb.buffer_ = null;
        }

        private static unsafe void RingBufferSetup(
            BrotliEncoderParams* params_, RingBuffer* rb) {
            int window_bits = ComputeRbBits(params_);
            int tail_bits = params_->lgblock;
            *(uint*) &rb->size_ = 1u << window_bits;
            *(uint*) &rb->mask_ = (1u << window_bits) - 1;
            *(uint*) &rb->tail_size_ = 1u << tail_bits;
            *(uint*) &rb->total_size_ = rb->size_ + rb->tail_size_;
        }

        private static unsafe void RingBufferFree(ref MemoryManager m, RingBuffer* rb)
        {
            BrotliFree(ref m, rb->data_);
        }

        /* Allocates or re-allocates data_ to the given length + plus some slack
           region before and after. Fills the slack regions with zeros. */
        private static unsafe void RingBufferInitBuffer(
            ref MemoryManager m, uint buflen, RingBuffer* rb) {
            size_t kSlackForEightByteHashingEverywhere = 7;
            byte* new_data = (byte*) BrotliAllocate(ref m,
                (2 + buflen + kSlackForEightByteHashingEverywhere) * sizeof(uint));
            size_t i;
            if (rb->data_ != null) {
                memcpy(new_data, rb->data_,
                    2 + rb->cur_size_ + kSlackForEightByteHashingEverywhere);
                BrotliFree(ref m, rb->data_);
            }
            rb->data_ = new_data;
            rb->cur_size_ = buflen;
            rb->buffer_ = rb->data_ + 2;
            rb->buffer_[-2] = rb->buffer_[-1] = 0;
            for (i = 0; i < kSlackForEightByteHashingEverywhere; ++i) {
                rb->buffer_[rb->cur_size_ + i] = 0;
            }
        }

        private static unsafe void RingBufferWriteTail(
            byte* bytes, size_t n, RingBuffer* rb) {
            size_t masked_pos = rb->pos_ & rb->mask_;
            if (masked_pos < rb->tail_size_) {
                /* Just fill the tail buffer with the beginning data. */
                size_t p = rb->size_ + masked_pos;
                memcpy(&rb->buffer_[p], bytes,
                    Math.Min(n, rb->tail_size_ - masked_pos));
            }
        }

        /* Push bytes into the ring buffer. */
        private static unsafe void RingBufferWrite(
            ref MemoryManager m, byte* bytes, size_t n, RingBuffer* rb) {
            if (rb->pos_ == 0 && n < rb->tail_size_) {
                /* Special case for the first write: to process the first block, we don't
                   need to allocate the whole ring-buffer and we don't need the tail
                   either. However, we do this memory usage optimization only if the
                   first write is less than the tail size, which is also the input block
                   size, otherwise it is likely that other blocks will follow and we
                   will need to reallocate to the full size anyway. */
                rb->pos_ = (uint) n;
                RingBufferInitBuffer(ref m, rb->pos_, rb);
                memcpy(rb->buffer_, bytes, n);
                return;
            }
            if (rb->cur_size_ < rb->total_size_) {
                /* Lazily allocate the full buffer. */
                RingBufferInitBuffer(ref m, rb->total_size_, rb);
                /* Initialize the last two bytes to zero, so that we don't have to worry
                   later when we copy the last two bytes to the first two positions. */
                rb->buffer_[rb->size_ - 2] = 0;
                rb->buffer_[rb->size_ - 1] = 0;
            }
            {
                size_t masked_pos = rb->pos_ & rb->mask_;
                /* The length of the writes is limited so that we do not need to worry
                   about a write */
                RingBufferWriteTail(bytes, n, rb);
                if (masked_pos + n <= rb->size_) {
                    /* A single write fits. */
                    memcpy(&rb->buffer_[masked_pos], bytes, n);
                }
                else {
                    /* Split into two writes.
                       Copy into the end of the buffer, including the tail buffer. */
                    memcpy(&rb->buffer_[masked_pos], bytes,
                        Math.Min(n, rb->total_size_ - masked_pos));
                    /* Copy into the beginning of the buffer */
                    memcpy(&rb->buffer_[0], bytes + (rb->size_ - masked_pos),
                        n - (rb->size_ - masked_pos));
                }
            }
            rb->buffer_[-2] = rb->buffer_[rb->size_ - 2];
            rb->buffer_[-1] = rb->buffer_[rb->size_ - 1];
            rb->pos_ += (uint) n;
            if (rb->pos_ > (1u << 30)) {
                /* Wrap, but preserve not-a-first-lap feature. */
                rb->pos_ = (rb->pos_ & ((1u << 30) - 1)) | (1u << 30);
            }
        }
    }
}