using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VisualHFT.ViewModel.Model
{
    public class BookItemPriceSplit : BindableBase, ICloneable
    {
        private double _price;
        private string _lastDecimal = "";
        private string _nextTwoDecimals = "";
        private string _rest = "";
        private string _size = "";
        private string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private string thousandsSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
        private StringBuilder sPriceBuilder = new StringBuilder();

        public void SetNumber_TOREMOVE(double price, double size, int symbolDecimalPlaces)
        {
            _price = price;
            if (price != 0)
            {
                try
                {
                    string sPrice = string.Format("{0:N" + symbolDecimalPlaces + "}", price);
                    if (symbolDecimalPlaces > 0)
                    {
                        _rest = sPrice.Split(decimalSeparator)[0];
                        _nextTwoDecimals = (sPrice.Split(decimalSeparator)[1] + "00").Substring(0, symbolDecimalPlaces);
                        _lastDecimal = "";
                    }
                    else
                    {
                        _rest = sPrice.Split(thousandsSeparator)[0];
                        _nextTwoDecimals = sPrice.Split(thousandsSeparator)[1];
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
        public void SetNumber(double price, double size, int symbolDecimalPlaces)
        {
            if (price == 0)
            {
                _lastDecimal = _nextTwoDecimals = _rest = _size = "";
                return;
            }

            _price = price;
            try
            {
                // Use StringBuilder to minimize allocations
                sPriceBuilder.Clear();
                sPriceBuilder.AppendFormat("{0:N" + symbolDecimalPlaces + "}", price);

                string sPrice = sPriceBuilder.ToString();
                string[] parts;

                if (symbolDecimalPlaces > 0)
                {
                    parts = sPrice.Split(decimalSeparator);
                    _rest = parts[0];
                    _nextTwoDecimals = parts[1].PadRight(2, '0').Substring(0, symbolDecimalPlaces);
                }
                else
                {
                    parts = sPrice.Split(thousandsSeparator);
                    _rest = parts[0];
                    _nextTwoDecimals = parts.Length > 1 ? parts[1] : "";
                }

                _size = Helpers.HelperCommon.GetKiloFormatter(size);
            }
            catch
            {
                _lastDecimal = _nextTwoDecimals = _rest = _size = "-";
            }
        }


        public void Clear()
        {
            _price = 0;
            _lastDecimal = "";
            _nextTwoDecimals = "";
            _rest = "";
            _size = "";
        }
        public void RaiseUIThread()
        {
            RaisePropertyChanged(nameof(LastDecimal));
            RaisePropertyChanged(nameof(NextTwoDecimals));
            RaisePropertyChanged(nameof(Rest));
            RaisePropertyChanged(nameof(Size));
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
