using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Linq;

namespace VisualHFT.ViewModel.Model
{
    public class BookItemPriceSplit : BindableBase, ICloneable
    {
        private double _price;
        private string _lastDecimal = "";
        private string _nextTwoDecimals = "";
        private string _rest = "";
        private string _size = "";
        private object _locker = new object();
        public void SetNumber(double price, double size, int symbolDecimalPlaces)
        {
            lock (_locker)
            {
                _price = price;
                if (price != 0)
                {
                    try
                    {
                        string sPrice = string.Format("{0:N" + symbolDecimalPlaces + "}", price);
                        if (symbolDecimalPlaces > 0)
                        {
                            /*LastDecimal = sPrice.Last().ToString();
                            NextTwoDecimals = sPrice.Substring(sPrice.Length - 3, 2);
                            Rest = sPrice.Substring(0, sPrice.Length - 3);*/

                            _rest = sPrice.Split('.')[0];
                            _nextTwoDecimals = (sPrice.Split('.')[1] + "00").Substring(0, symbolDecimalPlaces);
                            _lastDecimal = "";
                        }
                        else
                        {
                            _rest = sPrice.Split(',')[0];
                            _nextTwoDecimals = sPrice.Split(',')[1];
                        }
                        _size = Helpers.HelperCommon.GetKiloFormatter(size);

                    }
                    catch
                    {
                        _lastDecimal = "-";
                        _nextTwoDecimals = "-";
                        _rest = "-";
                        _size = "-";
                    }
                }


                if (price == 0)
                {
                    _lastDecimal = "";
                    _nextTwoDecimals = "";
                    _rest = "";
                    _size = "";
                }
            }
        }
        public void Clear()
        {
            lock (_locker)
            {
                _price = 0;
                _lastDecimal = "";
                _nextTwoDecimals = "";
                _rest = "";
                _size = "";
            }
            RaiseUIThread();
        }
        public void RaiseUIThread()
        {
            lock (_locker)
            {
                RaisePropertyChanged(nameof(LastDecimal));
                RaisePropertyChanged(nameof(NextTwoDecimals));
                RaisePropertyChanged(nameof(Rest));
                RaisePropertyChanged(nameof(Size));
            }
        }

        public object Clone() => MemberwiseClone();

        public string LastDecimal
        {
            get => _lastDecimal;
        }
        public string NextTwoDecimals
        {
            get => _nextTwoDecimals;
        }
        public string Rest
        {
            get => _rest;
        }
        public string Size
        {
            get => _size;
        }
        public double Price
        {
            get => _price;
        }
    }
}
