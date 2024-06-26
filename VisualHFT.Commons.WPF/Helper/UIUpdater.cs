using System;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class UIUpdater : IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private DispatcherTimer _debounceTimer;
        private Action _updateAction;
        private bool _isActionRunning = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UIUpdater(Action updateAction, double debounceTimeInMilliseconds = 30)
        {
            _updateAction = updateAction;

            _debounceTimer = new DispatcherTimer();
            _debounceTimer.Interval = TimeSpan.FromMilliseconds(debounceTimeInMilliseconds);
            _debounceTimer.Tick += _debounceTimer_Tick;
            _debounceTimer.Start();
        }

        ~UIUpdater()
        {
            Dispose(false);
        }

        private void _debounceTimer_Tick(object sender, EventArgs e)
        {
            if (_isActionRunning)
            {
                return; // Skip this tick if the action is still running
            }

            _isActionRunning = true;
            _debounceTimer.Stop(); // Stop the timer
            try
            {
                _updateAction(); // Execute the UI update action
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                _isActionRunning = false;
                _debounceTimer.Start(); // Restart the timer
            }
        }

        public void Stop()
        {
            _debounceTimer.Stop(); // Stop the timer
        }

        public void Start()
        {
            if (!_isActionRunning)
            {
                _debounceTimer.Start(); // Start the timer if action is not running
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _debounceTimer.Stop();
                    _debounceTimer.Tick -= _debounceTimer_Tick;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
