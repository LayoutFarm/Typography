using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct HistogramPair
        {
            public uint idx1;
            public uint idx2;
            public double cost_combo;
            public double cost_diff;
        }

        private static unsafe bool HistogramPairIsLess(
            HistogramPair* p1, HistogramPair* p2)
        {
            if (p1->cost_diff != p2->cost_diff)
            {
                return (p1->cost_diff > p2->cost_diff);
            }
            return ((p1->idx2 - p1->idx1) > (p2->idx2 - p2->idx1));
        }

        /* Returns entropy reduction of the context map when we combine two clusters. */
        private static unsafe double ClusterCostDiff(size_t size_a, size_t size_b)
        {
            size_t size_c = size_a + size_b;
            return (double)size_a * FastLog2(size_a) +
                   (double)size_b * FastLog2(size_b) -
                   (double)size_c * FastLog2(size_c);
        }
    }
}