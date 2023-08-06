using VisualHFT.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperTrade
    {
        public event EventHandler<Trade> OnDataReceived;

        public HelperTrade()
        {}

        protected virtual void RaiseOnDataReceived(List<Trade> trades)
        {
            EventHandler<Trade> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                foreach (var p in trades)
                    _handler(this, p);
            }
        }
        public void UpdateData(IEnumerable<Trade> trades)
        {
            RaiseOnDataReceived(trades.ToList());
        }

    }
}
