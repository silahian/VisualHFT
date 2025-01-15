namespace VisualHFT.Commons.Pools
{
    public class StringPool
    {
        private HashSet<string> _strings = new HashSet<string>();

        public string GetOrAdd(string value)
        {
            if (_strings.TryGetValue(value, out var existing))
                return existing;
            _strings.Add(value);
            return value;
        }

        public static StringPool Shared { get; } = new StringPool();
    }

}