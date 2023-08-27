using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace VisualHFT.Model
{
    public class VPIN: BindableBase
    {
        private DateTime _timestampIni;
        private DateTime _timestampEnd;
        private decimal _value;
        private decimal _marketMidPrice;

        public DateTime TimestampIni
        {
            get => _timestampIni;
            set => SetProperty(ref _timestampIni, value);
        }
        public DateTime TimestampEnd
        {
            get => _timestampEnd;
            set => SetProperty(ref _timestampEnd, value);
        }
        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public decimal MarketMidPrice
        { 
            get => _marketMidPrice;
            set => SetProperty(ref _marketMidPrice, value);
        }

    }
}
