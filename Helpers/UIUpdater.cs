using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class UIUpdater: IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private DispatcherTimer _debounceTimer;
        private Action _updateAction;

        public UIUpdater(Action updateAction, double debounceTimeInMilliseconds=30)
        {
            _updateAction = updateAction;

            // Ensure the timer is created on the UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _debounceTimer = new DispatcherTimer();
                _debounceTimer.Interval = TimeSpan.FromMilliseconds(debounceTimeInMilliseconds);
                _debounceTimer.Tick += _debounceTimer_Tick;
                _debounceTimer.Start();
            });
        }
        ~UIUpdater()
        {
            Dispose(false);
        }
        private void _debounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();  // Stop the timer
            _updateAction();        // Execute the UI update action
            _debounceTimer.Start();  // Stop the timer
        }

        public void Stop()
        {
            _debounceTimer.Stop();   // Reset the timer if it's running
        }
        public void Start()
        {
            _debounceTimer.Start();  // Start (or restart) the timer
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
