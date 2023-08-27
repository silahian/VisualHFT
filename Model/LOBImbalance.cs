using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace VisualHFT.Model
{
    public class LOBImbalance : BindableBase
    {
        private DateTime _timestamp;
        private decimal _value;
        private decimal _marketMidPrice;

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }
        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public decimal MarketMidPrice { 
            get => _marketMidPrice; 
            set => SetProperty(ref _marketMidPrice, value); 
        }
    }
}
