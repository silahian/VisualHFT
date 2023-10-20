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
        public event EventHandler<VisualHFT.ViewModel.Model.Trade> OnDataReceived;

        public HelperTrade()
        { }

        protected virtual void RaiseOnDataReceived(List<Trade> trades)
        {
            EventHandler<VisualHFT.ViewModel.Model.Trade> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                foreach (var p in trades.Select(x => new VisualHFT.ViewModel.Model.Trade(x)))
                    _handler(this, p);
            }
        }
        public void UpdateData(IEnumerable<Trade> trades)
        {
            RaiseOnDataReceived(trades.ToList());
        }
    }
}
