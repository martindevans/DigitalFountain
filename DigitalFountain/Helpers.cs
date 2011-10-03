using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalFountain
{
    internal static class Helpers
    {
        private static int CalculateBlockSetCount(Random rand, int blockCount)
        {
            var r = rand.NextDouble();

            float probOne = 1 / ((float)blockCount);
            if (r < probOne)
                return 1;
            else
                r -= probOne;

            for (int k = 2; k <= blockCount; k++)
            {
                double kPrecise = (double)k;
                double probability = 1 / (kPrecise * (kPrecise - 1));
                if (r <= probability)
                    return k;
                else
                    r -= probability;
            }

            throw new InvalidOperationException("Probability function did not sum to one");
        }

        internal static void CalculateBlockSet(int seed, int blockCount, HashSet<int> blocks)
        {
            Random r = new Random(seed);

            int size = CalculateBlockSetCount(r, blockCount);
            if (size > blockCount)
                throw new InvalidOperationException("Size must be less than blockcount");

            blocks.Clear();
            while (blocks.Count < size)
                blocks.Add(r.Next(0, blockCount));
        }
    }
}
