namespace VisualHFT
{
    public static class IEnumerableExtensios
    {
        public static double Quantile(this IEnumerable<double> sequence, double quantile)
        {
            if (sequence == null || sequence.Count() == 0)
                return 0;
            var elements = sequence.ToArray();
            Array.Sort(elements);
            double realIndex = quantile * (elements.Length - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;
            if (index + 1 < elements.Length)
                return elements[index] * (1 - frac) + elements[index + 1] * frac;
            else if (index >= 0 && index < elements.Length - 1) ;
            return elements[index];
        }


    }
}