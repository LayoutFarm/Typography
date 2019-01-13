using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        private static unsafe double ShannonEntropy(uint* population,
            size_t size, size_t* total)
        {
            size_t sum = 0;
            double retval = 0;
            uint* population_end = population + size;
            size_t p;
            if ((size & 1) != 0)
            {
                p = *population++;
                sum += p;
                retval -= (double)p * FastLog2(p);
            }
            while (population < population_end)
            {
                p = *population++;
                sum += p;
                retval -= (double)p * FastLog2(p);
                p = *population++;
                sum += p;
                retval -= (double)p * FastLog2(p);
            }
            if (sum != 0) retval += (double)sum * FastLog2(sum);
            *total = sum;
            return retval;
        }

        private static unsafe double BitsEntropy(
            uint* population, size_t size)
        {
            size_t sum;
            double retval = ShannonEntropy(population, size, &sum);
            if (retval < sum)
            {
                /* At least one bit per literal is needed. */
                retval = (double)sum;
            }
            return retval;
        }
    }
}