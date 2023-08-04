using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT
{
    public static class IEnumerableExtensios
    {
        public static double Quantile(this IEnumerable<double> sequence, double quantile)
        {
            var elements = sequence.ToArray();
            Array.Sort(elements);
            double realIndex = quantile * (elements.Length - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;
            if (index + 1 < elements.Length)
                return elements[index] * (1 - frac) + elements[index + 1] * frac;
            else
                return elements[index];
        }


    }
}
