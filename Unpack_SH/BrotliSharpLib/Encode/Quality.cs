using System;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        /* Returns hash-table size for quality levels 0 and 1. */
        private static size_t MaxHashTableSize(int quality) {
            return quality == FAST_ONE_PASS_COMPRESSION_QUALITY ? 1 << 15 : 1 << 17;
        }

        private static unsafe size_t MaxZopfliLen(BrotliEncoderParams* params_)
        {
            return params_->quality <= 10 ?
                MAX_ZOPFLI_LEN_QUALITY_10 :
                MAX_ZOPFLI_LEN_QUALITY_11;
        }

        private static unsafe size_t MaxMetablockSize(
            BrotliEncoderParams* params_)
        {
            int bits =
                Math.Min(ComputeRbBits(params_), BROTLI_MAX_INPUT_BLOCK_BITS);
            return (size_t)1 << bits;
        }

        /* When searching for backward references and have not seen matches for a long
           time, we can skip some match lookups. Unsuccessful match lookups are very
           expensive and this kind of a heuristic speeds up compression quite a lot.
           At first 8 byte strides are taken and every second byte is put to hasher.
           After 4x more literals stride by 16 bytes, every put 4-th byte to hasher.
           Applied only to qualities 2 to 9. */
        private static unsafe size_t LiteralSpreeLengthForSparseSearch(
            BrotliEncoderParams* params_)
        {
            return params_->quality < 9 ? 64 : 512;
        }

        /* Number of best candidates to evaluate to expand Zopfli chain. */
        private static unsafe size_t MaxZopfliCandidates(
            BrotliEncoderParams* params_)
        {
            return params_->quality <= 10 ? 1 : 5;
        }

        private static unsafe void SanitizeParams(BrotliEncoderParams* params_) {
            params_->quality = Math.Min(BROTLI_MAX_QUALITY,
                Math.Max(BROTLI_MIN_QUALITY, params_->quality));
            if (params_->lgwin < BROTLI_MIN_WINDOW_BITS) {
                params_->lgwin = BROTLI_MIN_WINDOW_BITS;
            }
            else if (params_->lgwin > BROTLI_MAX_WINDOW_BITS) {
                params_->lgwin = BROTLI_MAX_WINDOW_BITS;
            }
        }

        /* Returns optimized lg_block value. */
        private static unsafe int ComputeLgBlock(BrotliEncoderParams* params_) {
            int lgblock = params_->lgblock;
            if (params_->quality == FAST_ONE_PASS_COMPRESSION_QUALITY ||
                params_->quality == FAST_TWO_PASS_COMPRESSION_QUALITY) {
                lgblock = params_->lgwin;
            }
            else if (params_->quality < MIN_QUALITY_FOR_BLOCK_SPLIT) {
                lgblock = 14;
            }
            else if (lgblock == 0) {
                lgblock = 16;
                if (params_->quality >= 9 && params_->lgwin > lgblock) {
                    lgblock = Math.Min(18, params_->lgwin);
                }
            }
            else {
                lgblock = Math.Min(BROTLI_MAX_INPUT_BLOCK_BITS,
                    Math.Max(BROTLI_MIN_INPUT_BLOCK_BITS, lgblock));
            }
            return lgblock;
        }

        /* Returns log2 of the size of main ring buffer area.
           Allocate at least lgwin + 1 bits for the ring buffer so that the newly
           added block fits there completely and we still get lgwin bits and at least
           read_block_size_bits + 1 bits because the copy tail length needs to be
           smaller than ring-buffer size. */
        private static unsafe int ComputeRbBits(BrotliEncoderParams* params_) {
            return 1 + Math.Max(params_->lgwin, params_->lgblock);
        }

        static unsafe void ChooseHasher(BrotliEncoderParams* params_,
            BrotliHasherParams* hparams) {
            if (params_->quality > 9) {
                hparams->type = 10;
            }
            else if (params_->quality == 4 && params_->size_hint >= (1 << 20)) {
                hparams->type = 54;
            }
            else if (params_->quality < 5) {
                hparams->type = params_->quality;
            }
            else if (params_->lgwin <= 16) {
                hparams->type = params_->quality < 7 ? 40 : params_->quality < 9 ? 41 : 42;
            }
            else if (params_->size_hint >= (1 << 20) && params_->lgwin >= 19) {
                hparams->type = 6;
                hparams->block_bits = params_->quality - 1;
                hparams->bucket_bits = 15;
                hparams->hash_len = 5;
                hparams->num_last_distances_to_check =
                    params_->quality < 7 ? 4 : params_->quality < 9 ? 10 : 16;
            }
            else {
                hparams->type = 5;
                hparams->block_bits = params_->quality - 1;
                hparams->bucket_bits = params_->quality < 7 ? 14 : 15;
                hparams->num_last_distances_to_check =
                    params_->quality < 7 ? 4 : params_->quality < 9 ? 10 : 16;
            }
        }
    }
}