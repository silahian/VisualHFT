using VisualHFT.Enums;

namespace System
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan ToTimeSpan(this AggregationLevel aggregationLevel)
        {
            switch (aggregationLevel)
            {
                case AggregationLevel.None: return TimeSpan.Zero;
                case AggregationLevel.Ms1: return TimeSpan.FromMilliseconds(1);
                case AggregationLevel.Ms10: return TimeSpan.FromMilliseconds(10);
                case AggregationLevel.Ms100: return TimeSpan.FromMilliseconds(100);
                case AggregationLevel.Ms500: return TimeSpan.FromMilliseconds(500);
                case AggregationLevel.S1: return TimeSpan.FromSeconds(1);
                case AggregationLevel.S3: return TimeSpan.FromSeconds(3);
                case AggregationLevel.S5: return TimeSpan.FromSeconds(5);
                case AggregationLevel.D1: return TimeSpan.FromDays(1);
                //case AggregationLevel.Automatic: return TimeSpan.Zero; // Default behavior for Automatic. It will be recalculated.
                default: throw new ArgumentException("Unsupported aggregation level", nameof(aggregationLevel));
            }
        }

    }
}