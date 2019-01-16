using System;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private unsafe class ClusterLiteral {
            public static void BrotliCompareAndPushToQueue(
                HistogramLiteral* out_, uint* cluster_size, uint idx1,
                uint idx2, size_t max_num_pairs, HistogramPair* pairs,
                size_t* num_pairs) {
                bool is_good_pair = false;
                HistogramPair p = new HistogramPair();
                if (idx1 == idx2) {
                    return;
                }
                if (idx2 < idx1) {
                    uint t = idx2;
                    idx2 = idx1;
                    idx1 = t;
                }
                p.idx1 = idx1;
                p.idx2 = idx2;
                p.cost_diff = 0.5 * ClusterCostDiff(cluster_size[idx1], cluster_size[idx2]);
                p.cost_diff -= out_[idx1].bit_cost_;
                p.cost_diff -= out_[idx2].bit_cost_;

                if (out_[idx1].total_count_ == 0) {
                    p.cost_combo = out_[idx2].bit_cost_;
                    is_good_pair = true;
                }
                else if (out_[idx2].total_count_ == 0) {
                    p.cost_combo = out_[idx1].bit_cost_;
                    is_good_pair = true;
                }
                else {
                    double threshold = *num_pairs == 0 ? 1e99 : Math.Max(0.0, pairs[0].cost_diff);
                    HistogramLiteral combo = out_[idx1];
                    double cost_combo;
                    HistogramLiteral.HistogramAddHistogram(&combo, &out_[idx2]);
                    cost_combo = BitCostLiteral.BrotliPopulationCost(&combo);
                    if (cost_combo < threshold - p.cost_diff) {
                        p.cost_combo = cost_combo;
                        is_good_pair = true;
                    }
                }
                if (is_good_pair) {
                    p.cost_diff += p.cost_combo;
                    if (*num_pairs > 0 && HistogramPairIsLess(&pairs[0], &p)) {
                        /* Replace the top of the queue if needed. */
                        if (*num_pairs < max_num_pairs) {
                            pairs[*num_pairs] = pairs[0];
                            ++(*num_pairs);
                        }
                        pairs[0] = p;
                    }
                    else if (*num_pairs < max_num_pairs) {
                        pairs[*num_pairs] = p;
                        ++(*num_pairs);
                    }
                }
            }

            public static size_t BrotliHistogramCombine(HistogramLiteral* out_,
                uint* cluster_size,
                uint* symbols,
                uint* clusters,
                HistogramPair* pairs,
                size_t num_clusters,
                size_t symbols_size,
                size_t max_clusters,
                size_t max_num_pairs) {
                double cost_diff_threshold = 0.0;
                size_t min_cluster_size = 1;
                size_t num_pairs = 0;

                {
                    /* We maintain a vector of histogram pairs, with the property that the pair
                       with the maximum bit cost reduction is the first. */
                    size_t idx1;
                    for (idx1 = 0; idx1 < num_clusters; ++idx1) {
                        size_t idx2;
                        for (idx2 = idx1 + 1; idx2 < num_clusters; ++idx2) {
                            BrotliCompareAndPushToQueue(out_, cluster_size, clusters[idx1],
                                clusters[idx2], max_num_pairs, &pairs[0], &num_pairs);
                        }
                    }
                }

                while (num_clusters > min_cluster_size) {
                    uint best_idx1;
                    uint best_idx2;
                    size_t i;
                    if (pairs[0].cost_diff >= cost_diff_threshold) {
                        cost_diff_threshold = 1e99;
                        min_cluster_size = max_clusters;
                        continue;
                    }
                    /* Take the best pair from the top of heap. */
                    best_idx1 = pairs[0].idx1;
                    best_idx2 = pairs[0].idx2;
                    HistogramLiteral.HistogramAddHistogram(&out_[best_idx1], &out_[best_idx2]);
                    out_[best_idx1].bit_cost_ = pairs[0].cost_combo;
                    cluster_size[best_idx1] += cluster_size[best_idx2];
                    for (i = 0; i < symbols_size; ++i) {
                        if (symbols[i] == best_idx2) {
                            symbols[i] = best_idx1;
                        }
                    }
                    for (i = 0; i < num_clusters; ++i) {
                        if (clusters[i] == best_idx2) {
                            memmove(&clusters[i], &clusters[i + 1],
                                (num_clusters - i - 1) * sizeof(uint));
                            break;
                        }
                    }
                    --num_clusters;
                    {
                        /* Remove pairs intersecting the just combined best pair. */
                        size_t copy_to_idx = 0;
                        for (i = 0; i < num_pairs; ++i) {
                            HistogramPair* p = &pairs[i];
                            if (p->idx1 == best_idx1 || p->idx2 == best_idx1 ||
                                p->idx1 == best_idx2 || p->idx2 == best_idx2) {
                                /* Remove invalid pair from the queue. */
                                continue;
                            }
                            if (HistogramPairIsLess(&pairs[0], p)) {
                                /* Replace the top of the queue if needed. */
                                HistogramPair front = pairs[0];
                                pairs[0] = *p;
                                pairs[copy_to_idx] = front;
                            }
                            else {
                                pairs[copy_to_idx] = *p;
                            }
                            ++copy_to_idx;
                        }
                        num_pairs = copy_to_idx;
                    }

                    /* Push new pairs formed with the combined histogram to the heap. */
                    for (i = 0; i < num_clusters; ++i) {
                        BrotliCompareAndPushToQueue(out_, cluster_size, best_idx1, clusters[i],
                            max_num_pairs, &pairs[0], &num_pairs);
                    }
                }
                return num_clusters;
            }

            /* What is the bit cost of moving histogram from cur_symbol to candidate. */
            public static double BrotliHistogramBitCostDistance(
                HistogramLiteral* histogram, HistogramLiteral* candidate) {
                if (histogram->total_count_ == 0) {
                    return 0.0;
                }
                else {
                    HistogramLiteral tmp = *histogram;
                    HistogramLiteral.HistogramAddHistogram(&tmp, candidate);
                    return BitCostLiteral.BrotliPopulationCost(&tmp) - candidate->bit_cost_;
                }
            }

            /* Find the best 'out_' histogram for each of the 'in' histograms.
               When called, clusters[0..num_clusters) contains the unique values from
               symbols[0..in_size), but this property is not preserved in this function.
               Note: we assume that out_[]->bit_cost_ is already up-to-date. */
            public static void BrotliHistogramRemap(HistogramLiteral* in_,
                size_t in_size, uint* clusters, size_t num_clusters,
                HistogramLiteral* out_, uint* symbols) {
                size_t i;
                for (i = 0; i < in_size; ++i) {
                    uint best_out = i == 0 ? symbols[0] : symbols[i - 1];
                    double best_bits =
                        BrotliHistogramBitCostDistance(&in_[i], &out_[best_out]);
                    size_t j;
                    for (j = 0; j < num_clusters; ++j) {
                        double cur_bits =
                            BrotliHistogramBitCostDistance(&in_[i], &out_[clusters[j]]);
                        if (cur_bits < best_bits) {
                            best_bits = cur_bits;
                            best_out = clusters[j];
                        }
                    }
                    symbols[i] = best_out;
                }

                /* Recompute each out_ based on raw and symbols. */
                for (i = 0; i < num_clusters; ++i) {
                    HistogramLiteral.HistogramClear(&out_[clusters[i]]);
                }
                for (i = 0; i < in_size; ++i) {
                    HistogramLiteral.HistogramAddHistogram(&out_[symbols[i]], &in_[i]);
                }
            }

            /* Reorders elements of the out_[0..length) array and changes values in
               symbols[0..length) array in the following way:
                 * when called, symbols[] contains indexes into out_[], and has N unique
                   values (possibly N < length)
                 * on return, symbols'[i] = f(symbols[i]) and
                              out_'[symbols'[i]] = out_[symbols[i]], for each 0 <= i < length,
                   where f is a bijection between the range of symbols[] and [0..N), and
                   the first occurrences of values in symbols'[i] come in consecutive
                   increasing order.
               Returns N, the number of unique values in symbols[]. */
            public static size_t BrotliHistogramReindex(ref MemoryManager m,
                HistogramLiteral* out_, uint* symbols, size_t length) {
                const uint kInvalidIndex = uint.MaxValue;
                uint* new_index = (uint*) BrotliAllocate(ref m, length * sizeof(uint));
                uint next_index;
                HistogramLiteral* tmp;
                size_t i;
                for (i = 0; i < length; ++i) {
                    new_index[i] = kInvalidIndex;
                }
                next_index = 0;
                for (i = 0; i < length; ++i) {
                    if (new_index[symbols[i]] == kInvalidIndex) {
                        new_index[symbols[i]] = next_index;
                        ++next_index;
                    }
                }
                /* TODO: by using idea of "cycle-sort" we can avoid allocation of
                   tmp and reduce the number of copying by the factor of 2. */
                tmp = (HistogramLiteral*) BrotliAllocate(ref m, next_index * sizeof(HistogramLiteral));
                next_index = 0;
                for (i = 0; i < length; ++i) {
                    if (new_index[symbols[i]] == next_index) {
                        tmp[next_index] = out_[symbols[i]];
                        ++next_index;
                    }
                    symbols[i] = new_index[symbols[i]];
                }
                BrotliFree(ref m, new_index);
                for (i = 0; i < next_index; ++i) {
                    out_[i] = tmp[i];
                }
                BrotliFree(ref m, tmp);
                return next_index;
            }

            public static void BrotliClusterHistograms(
                ref MemoryManager m, HistogramLiteral* in_, size_t in_size,
                size_t max_histograms, HistogramLiteral* out_, size_t* out_size,
                uint* histogram_symbols) {
                uint* cluster_size = (uint*) BrotliAllocate(ref m, in_size * sizeof(uint));
                uint* clusters = (uint*) BrotliAllocate(ref m, in_size * sizeof(uint));
                size_t num_clusters = 0;
                size_t max_input_histograms = 64;
                size_t pairs_capacity = max_input_histograms * max_input_histograms / 2;
                /* For the first pass of clustering, we allow all pairs. */
                HistogramPair* pairs =
                    (HistogramPair*) BrotliAllocate(ref m, (pairs_capacity + 1) * sizeof(HistogramPair));
                size_t i;
                for (i = 0; i < in_size; ++i) {
                    cluster_size[i] = 1;
                }

                for (i = 0; i < in_size; ++i) {
                    out_[i] = in_[i];
                    out_[i].bit_cost_ = BitCostLiteral.BrotliPopulationCost(&in_[i]);
                    histogram_symbols[i] = (uint) i;
                }

                for (i = 0; i < in_size; i += max_input_histograms) {
                    size_t num_to_combine =
                        Math.Min(in_size - i, max_input_histograms);
                    size_t num_new_clusters;
                    size_t j;
                    for (j = 0; j < num_to_combine; ++j) {
                        clusters[num_clusters + j] = (uint) (i + j);
                    }
                    num_new_clusters =
                        BrotliHistogramCombine(out_, cluster_size,
                            &histogram_symbols[i],
                            &clusters[num_clusters], pairs,
                            num_to_combine, num_to_combine,
                            max_histograms, pairs_capacity);
                    num_clusters += num_new_clusters;
                }

                {
                    /* For the second pass, we limit the total number of histogram pairs.
                       After this limit is reached, we only keep searching for the best pair. */
                    size_t max_num_pairs = Math.Min(
                        64 * num_clusters, (num_clusters / 2) * num_clusters);
                    BrotliEnsureCapacity(ref m, sizeof(HistogramPair), (void**)&pairs, &pairs_capacity, max_num_pairs + 1);

                    /* Collapse similar histograms. */
                    num_clusters = BrotliHistogramCombine(out_, cluster_size,
                        histogram_symbols, clusters,
                        pairs, num_clusters, in_size,
                        max_histograms, max_num_pairs);
                }
                BrotliFree(ref m, pairs);
                BrotliFree(ref m, cluster_size);
                /* Find the optimal map from original histograms to the final ones. */
                BrotliHistogramRemap(in_, in_size, clusters, num_clusters,
                    out_, histogram_symbols);
                BrotliFree(ref m, clusters);
                /* Convert the context map to a canonical form. */
                *out_size = BrotliHistogramReindex(ref m, out_, histogram_symbols, in_size);
            }
        }

        private unsafe class ClusterCommand
        {
            static void BrotliCompareAndPushToQueue(
                HistogramCommand* out_, uint* cluster_size, uint idx1,
                uint idx2, size_t max_num_pairs, HistogramPair* pairs,
                size_t* num_pairs)
            {
                bool is_good_pair = false;
                HistogramPair p = new HistogramPair();
                if (idx1 == idx2)
                {
                    return;
                }
                if (idx2 < idx1)
                {
                    uint t = idx2;
                    idx2 = idx1;
                    idx1 = t;
                }
                p.idx1 = idx1;
                p.idx2 = idx2;
                p.cost_diff = 0.5 * ClusterCostDiff(cluster_size[idx1], cluster_size[idx2]);
                p.cost_diff -= out_[idx1].bit_cost_;
                p.cost_diff -= out_[idx2].bit_cost_;

                if (out_[idx1].total_count_ == 0)
                {
                    p.cost_combo = out_[idx2].bit_cost_;
                    is_good_pair = true;
                }
                else if (out_[idx2].total_count_ == 0)
                {
                    p.cost_combo = out_[idx1].bit_cost_;
                    is_good_pair = true;
                }
                else
                {
                    double threshold = *num_pairs == 0 ? 1e99 : Math.Max(0.0, pairs[0].cost_diff);
                    HistogramCommand combo = out_[idx1];
                    double cost_combo;
                    HistogramCommand.HistogramAddHistogram(&combo, &out_[idx2]);
                    cost_combo = BitCostCommand.BrotliPopulationCost(&combo);
                    if (cost_combo < threshold - p.cost_diff)
                    {
                        p.cost_combo = cost_combo;
                        is_good_pair = true;
                    }
                }
                if (is_good_pair)
                {
                    p.cost_diff += p.cost_combo;
                    if (*num_pairs > 0 && HistogramPairIsLess(&pairs[0], &p))
                    {
                        /* Replace the top of the queue if needed. */
                        if (*num_pairs < max_num_pairs)
                        {
                            pairs[*num_pairs] = pairs[0];
                            ++(*num_pairs);
                        }
                        pairs[0] = p;
                    }
                    else if (*num_pairs < max_num_pairs)
                    {
                        pairs[*num_pairs] = p;
                        ++(*num_pairs);
                    }
                }
            }

            public static size_t BrotliHistogramCombine(HistogramCommand* out_,
                uint* cluster_size,
                uint* symbols,
                uint* clusters,
                HistogramPair* pairs,
                size_t num_clusters,
                size_t symbols_size,
                size_t max_clusters,
                size_t max_num_pairs)
            {
                double cost_diff_threshold = 0.0;
                size_t min_cluster_size = 1;
                size_t num_pairs = 0;

                {
                    /* We maintain a vector of histogram pairs, with the property that the pair
                       with the maximum bit cost reduction is the first. */
                    size_t idx1;
                    for (idx1 = 0; idx1 < num_clusters; ++idx1)
                    {
                        size_t idx2;
                        for (idx2 = idx1 + 1; idx2 < num_clusters; ++idx2)
                        {
                            BrotliCompareAndPushToQueue(out_, cluster_size, clusters[idx1],
                                clusters[idx2], max_num_pairs, &pairs[0], &num_pairs);
                        }
                    }
                }

                while (num_clusters > min_cluster_size)
                {
                    uint best_idx1;
                    uint best_idx2;
                    size_t i;
                    if (pairs[0].cost_diff >= cost_diff_threshold)
                    {
                        cost_diff_threshold = 1e99;
                        min_cluster_size = max_clusters;
                        continue;
                    }
                    /* Take the best pair from the top of heap. */
                    best_idx1 = pairs[0].idx1;
                    best_idx2 = pairs[0].idx2;
                    HistogramCommand.HistogramAddHistogram(&out_[best_idx1], &out_[best_idx2]);
                    out_[best_idx1].bit_cost_ = pairs[0].cost_combo;
                    cluster_size[best_idx1] += cluster_size[best_idx2];
                    for (i = 0; i < symbols_size; ++i)
                    {
                        if (symbols[i] == best_idx2)
                        {
                            symbols[i] = best_idx1;
                        }
                    }
                    for (i = 0; i < num_clusters; ++i)
                    {
                        if (clusters[i] == best_idx2)
                        {
                            memmove(&clusters[i], &clusters[i + 1],
                                (num_clusters - i - 1) * sizeof(uint));
                            break;
                        }
                    }
                    --num_clusters;
                    {
                        /* Remove pairs intersecting the just combined best pair. */
                        size_t copy_to_idx = 0;
                        for (i = 0; i < num_pairs; ++i)
                        {
                            HistogramPair* p = &pairs[i];
                            if (p->idx1 == best_idx1 || p->idx2 == best_idx1 ||
                                p->idx1 == best_idx2 || p->idx2 == best_idx2)
                            {
                                /* Remove invalid pair from the queue. */
                                continue;
                            }
                            if (HistogramPairIsLess(&pairs[0], p))
                            {
                                /* Replace the top of the queue if needed. */
                                HistogramPair front = pairs[0];
                                pairs[0] = *p;
                                pairs[copy_to_idx] = front;
                            }
                            else
                            {
                                pairs[copy_to_idx] = *p;
                            }
                            ++copy_to_idx;
                        }
                        num_pairs = copy_to_idx;
                    }

                    /* Push new pairs formed with the combined histogram to the heap. */
                    for (i = 0; i < num_clusters; ++i)
                    {
                        BrotliCompareAndPushToQueue(out_, cluster_size, best_idx1, clusters[i],
                            max_num_pairs, &pairs[0], &num_pairs);
                    }
                }
                return num_clusters;
            }

            /* What is the bit cost of moving histogram from cur_symbol to candidate. */
            public static double BrotliHistogramBitCostDistance(
                HistogramCommand* histogram, HistogramCommand* candidate)
            {
                if (histogram->total_count_ == 0)
                {
                    return 0.0;
                }
                else
                {
                    HistogramCommand tmp = *histogram;
                    HistogramCommand.HistogramAddHistogram(&tmp, candidate);
                    return BitCostCommand.BrotliPopulationCost(&tmp) - candidate->bit_cost_;
                }
            }

            /* Find the best 'out_' histogram for each of the 'in' histograms.
               When called, clusters[0..num_clusters) contains the unique values from
               symbols[0..in_size), but this property is not preserved in this function.
               Note: we assume that out_[]->bit_cost_ is already up-to-date. */
            public static void BrotliHistogramRemap(HistogramCommand* in_,
                size_t in_size, uint* clusters, size_t num_clusters,
                HistogramCommand* out_, uint* symbols)
            {
                size_t i;
                for (i = 0; i < in_size; ++i)
                {
                    uint best_out = i == 0 ? symbols[0] : symbols[i - 1];
                    double best_bits =
                        BrotliHistogramBitCostDistance(&in_[i], &out_[best_out]);
                    size_t j;
                    for (j = 0; j < num_clusters; ++j)
                    {
                        double cur_bits =
                            BrotliHistogramBitCostDistance(&in_[i], &out_[clusters[j]]);
                        if (cur_bits < best_bits)
                        {
                            best_bits = cur_bits;
                            best_out = clusters[j];
                        }
                    }
                    symbols[i] = best_out;
                }

                /* Recompute each out_ based on raw and symbols. */
                for (i = 0; i < num_clusters; ++i)
                {
                    HistogramCommand.HistogramClear(&out_[clusters[i]]);
                }
                for (i = 0; i < in_size; ++i)
                {
                    HistogramCommand.HistogramAddHistogram(&out_[symbols[i]], &in_[i]);
                }
            }

            /* Reorders elements of the out_[0..length) array and changes values in
               symbols[0..length) array in the following way:
                 * when called, symbols[] contains indexes into out_[], and has N unique
                   values (possibly N < length)
                 * on return, symbols'[i] = f(symbols[i]) and
                              out_'[symbols'[i]] = out_[symbols[i]], for each 0 <= i < length,
                   where f is a bijection between the range of symbols[] and [0..N), and
                   the first occurrences of values in symbols'[i] come in consecutive
                   increasing order.
               Returns N, the number of unique values in symbols[]. */
            public static size_t BrotliHistogramReindex(ref MemoryManager m,
                HistogramCommand* out_, uint* symbols, size_t length)
            {
                const uint kInvalidIndex = uint.MaxValue;
                uint* new_index = (uint*)BrotliAllocate(ref m, length * sizeof(uint));
                uint next_index;
                HistogramCommand* tmp;
                size_t i;
                for (i = 0; i < length; ++i)
                {
                    new_index[i] = kInvalidIndex;
                }
                next_index = 0;
                for (i = 0; i < length; ++i)
                {
                    if (new_index[symbols[i]] == kInvalidIndex)
                    {
                        new_index[symbols[i]] = next_index;
                        ++next_index;
                    }
                }
                /* TODO: by using idea of "cycle-sort" we can avoid allocation of
                   tmp and reduce the number of copying by the factor of 2. */
                tmp = (HistogramCommand*)BrotliAllocate(ref m, next_index * sizeof(HistogramCommand));
                next_index = 0;
                for (i = 0; i < length; ++i)
                {
                    if (new_index[symbols[i]] == next_index)
                    {
                        tmp[next_index] = out_[symbols[i]];
                        ++next_index;
                    }
                    symbols[i] = new_index[symbols[i]];
                }
                BrotliFree(ref m, new_index);
                for (i = 0; i < next_index; ++i)
                {
                    out_[i] = tmp[i];
                }
                BrotliFree(ref m, tmp);
                return next_index;
            }

            public static void BrotliClusterHistograms(
                ref MemoryManager m, HistogramCommand* in_, size_t in_size,
                size_t max_histograms, HistogramCommand* out_, size_t* out_size,
                uint* histogram_symbols)
            {
                uint* cluster_size = (uint*)BrotliAllocate(ref m, in_size * sizeof(uint));
                uint* clusters = (uint*)BrotliAllocate(ref m, in_size * sizeof(uint));
                size_t num_clusters = 0;
                size_t max_input_histograms = 64;
                size_t pairs_capacity = max_input_histograms * max_input_histograms / 2;
                /* For the first pass of clustering, we allow all pairs. */
                HistogramPair* pairs =
                    (HistogramPair*)BrotliAllocate(ref m, (pairs_capacity + 1) * sizeof(HistogramPair));
                size_t i;
                for (i = 0; i < in_size; ++i)
                {
                    cluster_size[i] = 1;
                }

                for (i = 0; i < in_size; ++i)
                {
                    out_[i] = in_[i];
                    out_[i].bit_cost_ = BitCostCommand.BrotliPopulationCost(&in_[i]);
                    histogram_symbols[i] = (uint)i;
                }

                for (i = 0; i < in_size; i += max_input_histograms)
                {
                    size_t num_to_combine =
                        Math.Min(in_size - i, max_input_histograms);
                    size_t num_new_clusters;
                    size_t j;
                    for (j = 0; j < num_to_combine; ++j)
                    {
                        clusters[num_clusters + j] = (uint)(i + j);
                    }
                    num_new_clusters =
                        BrotliHistogramCombine(out_, cluster_size,
                            &histogram_symbols[i],
                            &clusters[num_clusters], pairs,
                            num_to_combine, num_to_combine,
                            max_histograms, pairs_capacity);
                    num_clusters += num_new_clusters;
                }

                {
                    /* For the second pass, we limit the total number of histogram pairs.
                       After this limit is reached, we only keep searching for the best pair. */
                    size_t max_num_pairs = Math.Min(
                        64 * num_clusters, (num_clusters / 2) * num_clusters);
                    BrotliEnsureCapacity(ref m, sizeof(HistogramPair), (void**)&pairs, &pairs_capacity, max_num_pairs + 1);

                    /* Collapse similar histograms. */
                    num_clusters = BrotliHistogramCombine(out_, cluster_size,
                        histogram_symbols, clusters,
                        pairs, num_clusters, in_size,
                        max_histograms, max_num_pairs);
                }
                BrotliFree(ref m, pairs);
                BrotliFree(ref m, cluster_size);
                /* Find the optimal map from original histograms to the final ones. */
                BrotliHistogramRemap(in_, in_size, clusters, num_clusters,
                    out_, histogram_symbols);
                BrotliFree(ref m, clusters);
                /* Convert the context map to a canonical form. */
                *out_size = BrotliHistogramReindex(ref m, out_, histogram_symbols, in_size);
            }
        }

        private unsafe class ClusterDistance
        {
            static void BrotliCompareAndPushToQueue(
                HistogramDistance* out_, uint* cluster_size, uint idx1,
                uint idx2, size_t max_num_pairs, HistogramPair* pairs,
                size_t* num_pairs)
            {
                bool is_good_pair = false;
                HistogramPair p = new HistogramPair();
                if (idx1 == idx2)
                {
                    return;
                }
                if (idx2 < idx1)
                {
                    uint t = idx2;
                    idx2 = idx1;
                    idx1 = t;
                }
                p.idx1 = idx1;
                p.idx2 = idx2;
                p.cost_diff = 0.5 * ClusterCostDiff(cluster_size[idx1], cluster_size[idx2]);
                p.cost_diff -= out_[idx1].bit_cost_;
                p.cost_diff -= out_[idx2].bit_cost_;

                if (out_[idx1].total_count_ == 0)
                {
                    p.cost_combo = out_[idx2].bit_cost_;
                    is_good_pair = true;
                }
                else if (out_[idx2].total_count_ == 0)
                {
                    p.cost_combo = out_[idx1].bit_cost_;
                    is_good_pair = true;
                }
                else
                {
                    double threshold = *num_pairs == 0 ? 1e99 : Math.Max(0.0, pairs[0].cost_diff);
                    HistogramDistance combo = out_[idx1];
                    double cost_combo;
                    HistogramDistance.HistogramAddHistogram(&combo, &out_[idx2]);
                    cost_combo = BitCostDistance.BrotliPopulationCost(&combo);
                    if (cost_combo < threshold - p.cost_diff)
                    {
                        p.cost_combo = cost_combo;
                        is_good_pair = true;
                    }
                }
                if (is_good_pair)
                {
                    p.cost_diff += p.cost_combo;
                    if (*num_pairs > 0 && HistogramPairIsLess(&pairs[0], &p))
                    {
                        /* Replace the top of the queue if needed. */
                        if (*num_pairs < max_num_pairs)
                        {
                            pairs[*num_pairs] = pairs[0];
                            ++(*num_pairs);
                        }
                        pairs[0] = p;
                    }
                    else if (*num_pairs < max_num_pairs)
                    {
                        pairs[*num_pairs] = p;
                        ++(*num_pairs);
                    }
                }
            }

            public static size_t BrotliHistogramCombine(HistogramDistance* out_,
                uint* cluster_size,
                uint* symbols,
                uint* clusters,
                HistogramPair* pairs,
                size_t num_clusters,
                size_t symbols_size,
                size_t max_clusters,
                size_t max_num_pairs)
            {
                double cost_diff_threshold = 0.0;
                size_t min_cluster_size = 1;
                size_t num_pairs = 0;

                {
                    /* We maintain a vector of histogram pairs, with the property that the pair
                       with the maximum bit cost reduction is the first. */
                    size_t idx1;
                    for (idx1 = 0; idx1 < num_clusters; ++idx1)
                    {
                        size_t idx2;
                        for (idx2 = idx1 + 1; idx2 < num_clusters; ++idx2)
                        {
                            BrotliCompareAndPushToQueue(out_, cluster_size, clusters[idx1],
                                clusters[idx2], max_num_pairs, &pairs[0], &num_pairs);
                        }
                    }
                }

                while (num_clusters > min_cluster_size)
                {
                    uint best_idx1;
                    uint best_idx2;
                    size_t i;
                    if (pairs[0].cost_diff >= cost_diff_threshold)
                    {
                        cost_diff_threshold = 1e99;
                        min_cluster_size = max_clusters;
                        continue;
                    }
                    /* Take the best pair from the top of heap. */
                    best_idx1 = pairs[0].idx1;
                    best_idx2 = pairs[0].idx2;
                    HistogramDistance.HistogramAddHistogram(&out_[best_idx1], &out_[best_idx2]);
                    out_[best_idx1].bit_cost_ = pairs[0].cost_combo;
                    cluster_size[best_idx1] += cluster_size[best_idx2];
                    for (i = 0; i < symbols_size; ++i)
                    {
                        if (symbols[i] == best_idx2)
                        {
                            symbols[i] = best_idx1;
                        }
                    }
                    for (i = 0; i < num_clusters; ++i)
                    {
                        if (clusters[i] == best_idx2)
                        {
                            memmove(&clusters[i], &clusters[i + 1],
                                (num_clusters - i - 1) * sizeof(uint));
                            break;
                        }
                    }
                    --num_clusters;
                    {
                        /* Remove pairs intersecting the just combined best pair. */
                        size_t copy_to_idx = 0;
                        for (i = 0; i < num_pairs; ++i)
                        {
                            HistogramPair* p = &pairs[i];
                            if (p->idx1 == best_idx1 || p->idx2 == best_idx1 ||
                                p->idx1 == best_idx2 || p->idx2 == best_idx2)
                            {
                                /* Remove invalid pair from the queue. */
                                continue;
                            }
                            if (HistogramPairIsLess(&pairs[0], p))
                            {
                                /* Replace the top of the queue if needed. */
                                HistogramPair front = pairs[0];
                                pairs[0] = *p;
                                pairs[copy_to_idx] = front;
                            }
                            else
                            {
                                pairs[copy_to_idx] = *p;
                            }
                            ++copy_to_idx;
                        }
                        num_pairs = copy_to_idx;
                    }

                    /* Push new pairs formed with the combined histogram to the heap. */
                    for (i = 0; i < num_clusters; ++i)
                    {
                        BrotliCompareAndPushToQueue(out_, cluster_size, best_idx1, clusters[i],
                            max_num_pairs, &pairs[0], &num_pairs);
                    }
                }
                return num_clusters;
            }

            /* What is the bit cost of moving histogram from cur_symbol to candidate. */
            public static double BrotliHistogramBitCostDistance(
                HistogramDistance* histogram, HistogramDistance* candidate)
            {
                if (histogram->total_count_ == 0)
                {
                    return 0.0;
                }
                else
                {
                    HistogramDistance tmp = *histogram;
                    HistogramDistance.HistogramAddHistogram(&tmp, candidate);
                    return BitCostDistance.BrotliPopulationCost(&tmp) - candidate->bit_cost_;
                }
            }

            /* Find the best 'out_' histogram for each of the 'in' histograms.
               When called, clusters[0..num_clusters) contains the unique values from
               symbols[0..in_size), but this property is not preserved in this function.
               Note: we assume that out_[]->bit_cost_ is already up-to-date. */
            public static void BrotliHistogramRemap(HistogramDistance* in_,
                size_t in_size, uint* clusters, size_t num_clusters,
                HistogramDistance* out_, uint* symbols)
            {
                size_t i;
                for (i = 0; i < in_size; ++i)
                {
                    uint best_out = i == 0 ? symbols[0] : symbols[i - 1];
                    double best_bits =
                        BrotliHistogramBitCostDistance(&in_[i], &out_[best_out]);
                    size_t j;
                    for (j = 0; j < num_clusters; ++j)
                    {
                        double cur_bits =
                            BrotliHistogramBitCostDistance(&in_[i], &out_[clusters[j]]);
                        if (cur_bits < best_bits)
                        {
                            best_bits = cur_bits;
                            best_out = clusters[j];
                        }
                    }
                    symbols[i] = best_out;
                }

                /* Recompute each out_ based on raw and symbols. */
                for (i = 0; i < num_clusters; ++i)
                {
                    HistogramDistance.HistogramClear(&out_[clusters[i]]);
                }
                for (i = 0; i < in_size; ++i)
                {
                    HistogramDistance.HistogramAddHistogram(&out_[symbols[i]], &in_[i]);
                }
            }

            /* Reorders elements of the out_[0..length) array and changes values in
               symbols[0..length) array in the following way:
                 * when called, symbols[] contains indexes into out_[], and has N unique
                   values (possibly N < length)
                 * on return, symbols'[i] = f(symbols[i]) and
                              out_'[symbols'[i]] = out_[symbols[i]], for each 0 <= i < length,
                   where f is a bijection between the range of symbols[] and [0..N), and
                   the first occurrences of values in symbols'[i] come in consecutive
                   increasing order.
               Returns N, the number of unique values in symbols[]. */
            public static size_t BrotliHistogramReindex(ref MemoryManager m,
                HistogramDistance* out_, uint* symbols, size_t length)
            {
                const uint kInvalidIndex = uint.MaxValue;
                uint* new_index = (uint*)BrotliAllocate(ref m, length * sizeof(uint));
                uint next_index;
                HistogramDistance* tmp;
                size_t i;
                for (i = 0; i < length; ++i)
                {
                    new_index[i] = kInvalidIndex;
                }
                next_index = 0;
                for (i = 0; i < length; ++i)
                {
                    if (new_index[symbols[i]] == kInvalidIndex)
                    {
                        new_index[symbols[i]] = next_index;
                        ++next_index;
                    }
                }
                /* TODO: by using idea of "cycle-sort" we can avoid allocation of
                   tmp and reduce the number of copying by the factor of 2. */
                tmp = (HistogramDistance*)BrotliAllocate(ref m, next_index * sizeof(HistogramDistance));
                next_index = 0;
                for (i = 0; i < length; ++i)
                {
                    if (new_index[symbols[i]] == next_index)
                    {
                        tmp[next_index] = out_[symbols[i]];
                        ++next_index;
                    }
                    symbols[i] = new_index[symbols[i]];
                }
                BrotliFree(ref m, new_index);
                for (i = 0; i < next_index; ++i)
                {
                    out_[i] = tmp[i];
                }
                BrotliFree(ref m, tmp);
                return next_index;
            }

            public static void BrotliClusterHistograms(
                ref MemoryManager m, HistogramDistance* in_, size_t in_size,
                size_t max_histograms, HistogramDistance* out_, size_t* out_size,
                uint* histogram_symbols)
            {
                uint* cluster_size = (uint*)BrotliAllocate(ref m, in_size * sizeof(uint));
                uint* clusters = (uint*)BrotliAllocate(ref m, in_size * sizeof(uint));
                size_t num_clusters = 0;
                size_t max_input_histograms = 64;
                size_t pairs_capacity = max_input_histograms * max_input_histograms / 2;
                /* For the first pass of clustering, we allow all pairs. */
                HistogramPair* pairs =
                    (HistogramPair*)BrotliAllocate(ref m, (pairs_capacity + 1) * sizeof(HistogramPair));
                size_t i;
                for (i = 0; i < in_size; ++i)
                {
                    cluster_size[i] = 1;
                }

                for (i = 0; i < in_size; ++i)
                {
                    out_[i] = in_[i];
                    out_[i].bit_cost_ = BitCostDistance.BrotliPopulationCost(&in_[i]);
                    histogram_symbols[i] = (uint)i;
                }

                for (i = 0; i < in_size; i += max_input_histograms)
                {
                    size_t num_to_combine =
                        Math.Min(in_size - i, max_input_histograms);
                    size_t num_new_clusters;
                    size_t j;
                    for (j = 0; j < num_to_combine; ++j)
                    {
                        clusters[num_clusters + j] = (uint)(i + j);
                    }
                    num_new_clusters =
                        BrotliHistogramCombine(out_, cluster_size,
                            &histogram_symbols[i],
                            &clusters[num_clusters], pairs,
                            num_to_combine, num_to_combine,
                            max_histograms, pairs_capacity);
                    num_clusters += num_new_clusters;
                }

                {
                    /* For the second pass, we limit the total number of histogram pairs.
                       After this limit is reached, we only keep searching for the best pair. */
                    size_t max_num_pairs = Math.Min(
                        64 * num_clusters, (num_clusters / 2) * num_clusters);
                    BrotliEnsureCapacity(ref m, sizeof(HistogramPair), (void**)&pairs, &pairs_capacity, max_num_pairs + 1);

                    /* Collapse similar histograms. */
                    num_clusters = BrotliHistogramCombine(out_, cluster_size,
                        histogram_symbols, clusters,
                        pairs, num_clusters, in_size,
                        max_histograms, max_num_pairs);
                }
                BrotliFree(ref m, pairs);
                BrotliFree(ref m, cluster_size);
                /* Find the optimal map from original histograms to the final ones. */
                BrotliHistogramRemap(in_, in_size, clusters, num_clusters,
                    out_, histogram_symbols);
                BrotliFree(ref m, clusters);
                /* Convert the context map to a canonical form. */
                *out_size = BrotliHistogramReindex(ref m, out_, histogram_symbols, in_size);
            }
        }
    }
}