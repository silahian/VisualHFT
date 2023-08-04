using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Linq;

namespace VisualHFT.Model
{
    public class BookItemPriceSplit : BindableBase, ICloneable
    {
        private double _price;
        private string _lastDecimal = "";
        private string _nextTwoDecimals = "";
        private string _rest = "";
        private string _size = "";

        public void SetNumber(double price, double size, int symbolDecimalPlaces)
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

                        Rest = sPrice.Split('.')[0];
                        NextTwoDecimals = sPrice.Split('.')[1];
                        LastDecimal = "";
                    }
                    else
                    {
                        Rest = sPrice.Split(',')[0];
                        NextTwoDecimals = sPrice.Split(',')[1];
                    }
                    Size = Helpers.HelperCommon.GetKiloFormatter(size);
                }
                catch
                {
                    LastDecimal = "-";
                    NextTwoDecimals = "-";
                    Rest = "-";
                    Size = "-";
                }
            }


            if (price == 0)
            {
                LastDecimal = "";
                NextTwoDecimals = "";
                Rest = "";
                Size = "";
            }
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
            set => SetProperty(ref _lastDecimal, value);
        }

        public string NextTwoDecimals
        {
            get => _nextTwoDecimals;
            set => SetProperty(ref _nextTwoDecimals, value);
        }
        public string Rest
        {
            get => _rest;
            set => SetProperty(ref _rest, value);
        }
        public string Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }
        public double Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }
    }
}
