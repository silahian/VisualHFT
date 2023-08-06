using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace VisualHFT.Model
{
    public class Trade : BindableBase
    {
        private int _providerId;
        private string _providerName;
        private string  _symbol;
        private decimal _price;
        private decimal _size;
        private DateTime _timestamp;
        private bool _isBuy;
        private string _flags;


        public int ProviderId { get => _providerId; set => SetProperty(ref _providerId, value); }
        public string ProviderName { get => _providerName; set => SetProperty(ref _providerName, value); }
        public string Symbol { get => _symbol; set => SetProperty(ref _symbol, value); }
        public decimal Price { get => _price; set => SetProperty(ref _price, value); }
        public decimal Size { get => _size; set => SetProperty(ref _size, value); }
        public DateTime Timestamp {
            get
            {
                RaisePropertyChanged(nameof(ForegroundColor));
                return _timestamp;
            }
            set
            {
                SetProperty(ref _timestamp, value);                
            }
        }
        public bool IsBuy { get => _isBuy; set => SetProperty(ref _isBuy, value);  }
        public string Flags { get => _flags; set => SetProperty(ref _flags, value); }
        public System.Windows.Media.Brush ForegroundColor
        {
            get => _isBuy ? System.Windows.Media.Brushes.LightGreen : System.Windows.Media.Brushes.LightPink;
        }
    }
}
