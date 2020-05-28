using System.Collections.Generic;
using System.Linq;

namespace SoundManipulation
{
    public static class LinqExtension
    {

        public static IEnumerable<double> MergeSimilar(this IEnumerable<double> source, double similarityThreshold)
        {
            List<double> encountered = new List<double>();
            foreach (double value in source)
            {
                if (!encountered.Any(x => (1 - similarityThreshold) * x < value
                    && value > (1 + similarityThreshold) * x))
                {
                    encountered.Add(value);
                }
            }
            return encountered;
        }

        public static IEnumerable<int> MergeSimilar(this IEnumerable<int> source, double similarityThreshold)
        {
            List<int> encountered = new List<int>();
            foreach (int value in source)
            {
                if (!encountered.Any(x => (1 - similarityThreshold) * x < value
                    && value < (1 + similarityThreshold) * x))
                {
                    encountered.Add(value);
                }
            }
            return encountered;
        }
    }
}
