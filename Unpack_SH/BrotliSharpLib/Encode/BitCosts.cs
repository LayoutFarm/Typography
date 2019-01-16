using System;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        private unsafe class BitCostLiteral {
            public static double BrotliPopulationCost(HistogramLiteral* histogram) {
                const double kOneSymbolHistogramCost = 12;
                const double kTwoSymbolHistogramCost = 20;
                const double kThreeSymbolHistogramCost = 28;
                const double kFourSymbolHistogramCost = 37;

                size_t data_size = HistogramLiteral.HistogramDataSize();
                int count = 0;
                size_t* s = stackalloc size_t[5];
                double bits = 0.0;
                size_t i;
                if (histogram->total_count_ == 0)
                {
                    return kOneSymbolHistogramCost;
                }
                for (i = 0; i < data_size; ++i)
                {
                    if (histogram->data_[i] > 0)
                    {
                        s[count] = i;
                        ++count;
                        if (count > 4) break;
                    }
                }
                if (count == 1)
                {
                    return kOneSymbolHistogramCost;
                }
                if (count == 2)
                {
                    return (kTwoSymbolHistogramCost + (double)histogram->total_count_);
                }
                if (count == 3)
                {
                    uint histo0 = histogram->data_[s[0]];
                    uint histo1 = histogram->data_[s[1]];
                    uint histo2 = histogram->data_[s[2]];
                    uint histomax =
                        Math.Max(histo0, Math.Max(histo1, histo2));
                    return (kThreeSymbolHistogramCost +
                            2 * (histo0 + histo1 + histo2) - histomax);
                }
                if (count == 4)
                {
                    uint* histo = stackalloc uint[4];
                    uint h23;
                    uint histomax;
                    for (i = 0; i < 4; ++i)
                    {
                        histo[i] = histogram->data_[s[i]];
                    }
                    /* Sort */
                    for (i = 0; i < 4; ++i)
                    {
                        size_t j;
                        for (j = i + 1; j < 4; ++j)
                        {
                            if (histo[j] > histo[i]) {
                                uint tmp = histo[j];
                                histo[j] = histo[i];
                                histo[i] = tmp;
                            }
                        }
                    }
                    h23 = histo[2] + histo[3];
                    histomax = Math.Max(h23, histo[0]);
                    return (kFourSymbolHistogramCost +
                            3 * h23 + 2 * (histo[0] + histo[1]) - histomax);
                }

                {
                    /* In this loop we compute the entropy of the histogram and simultaneously
                       build a simplified histogram of the code length codes where we use the
                       zero repeat code 17, but we don't use the non-zero repeat code 16. */
                    size_t max_depth = 1;
                    uint* depth_histo = stackalloc uint[BROTLI_CODE_LENGTH_CODES];
                    memset(depth_histo, 0, BROTLI_CODE_LENGTH_CODES * sizeof(uint));
                    double log2total = FastLog2(histogram->total_count_);
                    for (i = 0; i < data_size;)
                    {
                        if (histogram->data_[i] > 0)
                        {
                            /* Compute -log2(P(symbol)) = -log2(count(symbol)/total_count) =
                                                        = log2(total_count) - log2(count(symbol)) */
                            double log2p = log2total - FastLog2(histogram->data_[i]);
                            /* Approximate the bit depth by round(-log2(P(symbol))) */
                            size_t depth = (size_t)(log2p + 0.5);
                            bits += histogram->data_[i] * log2p;
                            if (depth > 15)
                            {
                                depth = 15;
                            }
                            if (depth > max_depth)
                            {
                                max_depth = depth;
                            }
                            ++depth_histo[depth];
                            ++i;
                        }
                        else
                        {
                            /* Compute the run length of zeros and add the appropriate number of 0
                               and 17 code length codes to the code length code histogram. */
                            uint reps = 1;
                            size_t k;
                            for (k = i + 1; k < data_size && histogram->data_[k] == 0; ++k)
                            {
                                ++reps;
                            }
                            i += reps;
                            if (i == data_size)
                            {
                                /* Don't add any cost for the last zero run, since these are encoded
                                   only implicitly. */
                                break;
                            }
                            if (reps < 3)
                            {
                                depth_histo[0] += reps;
                            }
                            else
                            {
                                reps -= 2;
                                while (reps > 0)
                                {
                                    ++depth_histo[BROTLI_REPEAT_ZERO_CODE_LENGTH];
                                    /* Add the 3 extra bits for the 17 code length code. */
                                    bits += 3;
                                    reps >>= 3;
                                }
                            }
                        }
                    }
                    /* Add the estimated encoding cost of the code length code histogram. */
                    bits += (double)(18 + 2 * max_depth);
                    /* Add the entropy of the code length code histogram. */
                    bits += BitsEntropy(depth_histo, BROTLI_CODE_LENGTH_CODES);
                }
                return bits;
            }
        }

        private unsafe class BitCostCommand
        {
            public static double BrotliPopulationCost(HistogramCommand* histogram)
            {
                const double kOneSymbolHistogramCost = 12;
                const double kTwoSymbolHistogramCost = 20;
                const double kThreeSymbolHistogramCost = 28;
                const double kFourSymbolHistogramCost = 37;

                size_t data_size = HistogramCommand.HistogramDataSize();
                int count = 0;
                size_t* s = stackalloc size_t[5];
                double bits = 0.0;
                size_t i;
                if (histogram->total_count_ == 0)
                {
                    return kOneSymbolHistogramCost;
                }
                for (i = 0; i < data_size; ++i)
                {
                    if (histogram->data_[i] > 0)
                    {
                        s[count] = i;
                        ++count;
                        if (count > 4) break;
                    }
                }
                if (count == 1)
                {
                    return kOneSymbolHistogramCost;
                }
                if (count == 2)
                {
                    return (kTwoSymbolHistogramCost + (double)histogram->total_count_);
                }
                if (count == 3)
                {
                    uint histo0 = histogram->data_[s[0]];
                    uint histo1 = histogram->data_[s[1]];
                    uint histo2 = histogram->data_[s[2]];
                    uint histomax =
                        Math.Max(histo0, Math.Max(histo1, histo2));
                    return (kThreeSymbolHistogramCost +
                            2 * (histo0 + histo1 + histo2) - histomax);
                }
                if (count == 4)
                {
                    uint* histo = stackalloc uint[4];
                    uint h23;
                    uint histomax;
                    for (i = 0; i < 4; ++i)
                    {
                        histo[i] = histogram->data_[s[i]];
                    }
                    /* Sort */
                    for (i = 0; i < 4; ++i)
                    {
                        size_t j;
                        for (j = i + 1; j < 4; ++j)
                        {
                            if (histo[j] > histo[i])
                            {
                                uint tmp = histo[j];
                                histo[j] = histo[i];
                                histo[i] = tmp;
                            }
                        }
                    }
                    h23 = histo[2] + histo[3];
                    histomax = Math.Max(h23, histo[0]);
                    return (kFourSymbolHistogramCost +
                            3 * h23 + 2 * (histo[0] + histo[1]) - histomax);
                }

                {
                    /* In this loop we compute the entropy of the histogram and simultaneously
                       build a simplified histogram of the code length codes where we use the
                       zero repeat code 17, but we don't use the non-zero repeat code 16. */
                    size_t max_depth = 1;
                    uint* depth_histo = stackalloc uint[BROTLI_CODE_LENGTH_CODES];
                    memset(depth_histo, 0, BROTLI_CODE_LENGTH_CODES * sizeof(uint));
                    double log2total = FastLog2(histogram->total_count_);
                    for (i = 0; i < data_size;)
                    {
                        if (histogram->data_[i] > 0)
                        {
                            /* Compute -log2(P(symbol)) = -log2(count(symbol)/total_count) =
                                                        = log2(total_count) - log2(count(symbol)) */
                            double log2p = log2total - FastLog2(histogram->data_[i]);
                            /* Approximate the bit depth by round(-log2(P(symbol))) */
                            size_t depth = (size_t)(log2p + 0.5);
                            bits += histogram->data_[i] * log2p;
                            if (depth > 15)
                            {
                                depth = 15;
                            }
                            if (depth > max_depth)
                            {
                                max_depth = depth;
                            }
                            ++depth_histo[depth];
                            ++i;
                        }
                        else
                        {
                            /* Compute the run length of zeros and add the appropriate number of 0
                               and 17 code length codes to the code length code histogram. */
                            uint reps = 1;
                            size_t k;
                            for (k = i + 1; k < data_size && histogram->data_[k] == 0; ++k)
                            {
                                ++reps;
                            }
                            i += reps;
                            if (i == data_size)
                            {
                                /* Don't add any cost for the last zero run, since these are encoded
                                   only implicitly. */
                                break;
                            }
                            if (reps < 3)
                            {
                                depth_histo[0] += reps;
                            }
                            else
                            {
                                reps -= 2;
                                while (reps > 0)
                                {
                                    ++depth_histo[BROTLI_REPEAT_ZERO_CODE_LENGTH];
                                    /* Add the 3 extra bits for the 17 code length code. */
                                    bits += 3;
                                    reps >>= 3;
                                }
                            }
                        }
                    }
                    /* Add the estimated encoding cost of the code length code histogram. */
                    bits += (double)(18 + 2 * max_depth);
                    /* Add the entropy of the code length code histogram. */
                    bits += BitsEntropy(depth_histo, BROTLI_CODE_LENGTH_CODES);
                }
                return bits;
            }
        }

        private unsafe class BitCostDistance
        {
            public static double BrotliPopulationCost(HistogramDistance* histogram)
            {
                const double kOneSymbolHistogramCost = 12;
                const double kTwoSymbolHistogramCost = 20;
                const double kThreeSymbolHistogramCost = 28;
                const double kFourSymbolHistogramCost = 37;

                size_t data_size = HistogramDistance.HistogramDataSize();
                int count = 0;
                size_t* s = stackalloc size_t[5];
                double bits = 0.0;
                size_t i;
                if (histogram->total_count_ == 0)
                {
                    return kOneSymbolHistogramCost;
                }
                for (i = 0; i < data_size; ++i)
                {
                    if (histogram->data_[i] > 0)
                    {
                        s[count] = i;
                        ++count;
                        if (count > 4) break;
                    }
                }
                if (count == 1)
                {
                    return kOneSymbolHistogramCost;
                }
                if (count == 2)
                {
                    return (kTwoSymbolHistogramCost + (double)histogram->total_count_);
                }
                if (count == 3)
                {
                    uint histo0 = histogram->data_[s[0]];
                    uint histo1 = histogram->data_[s[1]];
                    uint histo2 = histogram->data_[s[2]];
                    uint histomax =
                        Math.Max(histo0, Math.Max(histo1, histo2));
                    return (kThreeSymbolHistogramCost +
                            2 * (histo0 + histo1 + histo2) - histomax);
                }
                if (count == 4)
                {
                    uint* histo = stackalloc uint[4];
                    uint h23;
                    uint histomax;
                    for (i = 0; i < 4; ++i)
                    {
                        histo[i] = histogram->data_[s[i]];
                    }
                    /* Sort */
                    for (i = 0; i < 4; ++i)
                    {
                        size_t j;
                        for (j = i + 1; j < 4; ++j)
                        {
                            if (histo[j] > histo[i])
                            {
                                uint tmp = histo[j];
                                histo[j] = histo[i];
                                histo[i] = tmp;
                            }
                        }
                    }
                    h23 = histo[2] + histo[3];
                    histomax = Math.Max(h23, histo[0]);
                    return (kFourSymbolHistogramCost +
                            3 * h23 + 2 * (histo[0] + histo[1]) - histomax);
                }

                {
                    /* In this loop we compute the entropy of the histogram and simultaneously
                       build a simplified histogram of the code length codes where we use the
                       zero repeat code 17, but we don't use the non-zero repeat code 16. */
                    size_t max_depth = 1;
                    uint* depth_histo = stackalloc uint[BROTLI_CODE_LENGTH_CODES];
                    memset(depth_histo, 0, BROTLI_CODE_LENGTH_CODES * sizeof(uint));
                    double log2total = FastLog2(histogram->total_count_);
                    for (i = 0; i < data_size;)
                    {
                        if (histogram->data_[i] > 0)
                        {
                            /* Compute -log2(P(symbol)) = -log2(count(symbol)/total_count) =
                                                        = log2(total_count) - log2(count(symbol)) */
                            double log2p = log2total - FastLog2(histogram->data_[i]);
                            /* Approximate the bit depth by round(-log2(P(symbol))) */
                            size_t depth = (size_t)(log2p + 0.5);
                            bits += histogram->data_[i] * log2p;
                            if (depth > 15)
                            {
                                depth = 15;
                            }
                            if (depth > max_depth)
                            {
                                max_depth = depth;
                            }
                            ++depth_histo[depth];
                            ++i;
                        }
                        else
                        {
                            /* Compute the run length of zeros and add the appropriate number of 0
                               and 17 code length codes to the code length code histogram. */
                            uint reps = 1;
                            size_t k;
                            for (k = i + 1; k < data_size && histogram->data_[k] == 0; ++k)
                            {
                                ++reps;
                            }
                            i += reps;
                            if (i == data_size)
                            {
                                /* Don't add any cost for the last zero run, since these are encoded
                                   only implicitly. */
                                break;
                            }
                            if (reps < 3)
                            {
                                depth_histo[0] += reps;
                            }
                            else
                            {
                                reps -= 2;
                                while (reps > 0)
                                {
                                    ++depth_histo[BROTLI_REPEAT_ZERO_CODE_LENGTH];
                                    /* Add the 3 extra bits for the 17 code length code. */
                                    bits += 3;
                                    reps >>= 3;
                                }
                            }
                        }
                    }
                    /* Add the estimated encoding cost of the code length code histogram. */
                    bits += (double)(18 + 2 * max_depth);
                    /* Add the entropy of the code length code histogram. */
                    bits += BitsEntropy(depth_histo, BROTLI_CODE_LENGTH_CODES);
                }
                return bits;
            }
        }
    }
}