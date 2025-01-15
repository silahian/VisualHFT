namespace VisualHFT
{
    public static class HelperTimeProvider
    {
        private static DateTime? _fixedTime = null;


        public static event EventHandler OnSetFixedTime;

        public static void SetFixedTime(DateTime dateTime)
        {
            _fixedTime = dateTime;
            OnSetFixedTime?.Invoke(null, new EventArgs());
        }

        public static void ResetToSystemTime()
        {
            _fixedTime = null;
            OnSetFixedTime?.Invoke(null, new EventArgs());
        }

        public static DateTime Now
        {
            get { return _fixedTime ?? DateTime.Now; }
        }
        public static void IncrementByMilliseconds(long milliseconds)
        {
            if (_fixedTime.HasValue)
            {
                _fixedTime = _fixedTime.Value.AddMilliseconds(milliseconds);
                // Optionally, trigger an event if necessary
                //OnSetFixedTime?.Invoke(null, new EventArgs());
            }

        }
        public static void IncrementByTicks(long ticks)
        {
            if (_fixedTime.HasValue)
            {
                _fixedTime = _fixedTime.Value.AddTicks(ticks);
                // Optionally, trigger an event if necessary
                //OnSetFixedTime?.Invoke(null, new EventArgs());
            }
        }
    }
}