using System;
using System.Runtime.InteropServices;


namespace VisualHFT.Commons.Helpers
{
    public static class HelperHighResolutionTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);
        private static long frequency = 0;
        private static long ticksPerMicrosecond = 0;
        public static void DelayMicroseconds(long microseconds)
        {
            if (microseconds <= 0) return;
            if (frequency == 0)
            {
                QueryPerformanceFrequency(out frequency);
                ticksPerMicrosecond = frequency / 1_000_000;
            }
            long ticks = microseconds * ticksPerMicrosecond;

            QueryPerformanceCounter(out long startTicks);
            long endTicks = startTicks + ticks;

            while (true)
            {
                QueryPerformanceCounter(out long currentTicks);
                if (currentTicks >= endTicks)
                    break;
            }
        }
    }
}
