using System;
using System.ComponentModel;

namespace VisualHFT.Model
{
    public class BookItem : INotifyPropertyChanged, IComparable, IEquatable<BookItem>
    {
        public BookItem()
        {

        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int CompareTo(object obj)
        {
            return Price.CompareTo(((BookItem)obj).Price);
        }
        public bool Equals(BookItem other)
        {
            if (other == null)
                return false;
            if (this.IsBid != other.IsBid)
                return false;
            if (this.EntryID != other.EntryID)
                return false;
            if (this.Price != other.Price)
                return false;
            if (this.Size != other.Size)
                return false;
            return true;

        }

        private string _Symbol;
        private int _ProviderID;
        private string _EntryID;
        private string _LayerName;
        private DateTime _LocalTimeStamp;
        private double _Price;
        private DateTime _ServerTimeStamp;
        private double _Size;
        private bool _IsBid;
        private int _DecimalPlaces;
        private double? _ActiveSize;

        public void Update(BookItem b)
        {
            this.Symbol = b.Symbol;
            this.ProviderID = b.ProviderID; 
            this.EntryID = b.EntryID;
            this.LayerName = b.LayerName;
            this.LocalTimeStamp = b.LocalTimeStamp;
            this.ServerTimeStamp = b.ServerTimeStamp;
            this.Price = b.Price;
            this.Size = b.Size;
            this.IsBid = b.IsBid;
            this.DecimalPlaces = b.DecimalPlaces;
            this.ActiveSize = b.ActiveSize;
        }
        public int DecimalPlaces { get => _DecimalPlaces; set => _DecimalPlaces = value; }
        public string Symbol
        {
            get
            {
                return _Symbol;
            }

            set
            {
                if (_Symbol != value)
                {
                    _Symbol = value;
                    RaisePropertyChanged("Symbol");
                }
            }
        }
        public int ProviderID
        {
            get
            {
                return _ProviderID;
            }

            set
            {
                if (_ProviderID != value)
                {
                    _ProviderID = value;
                    RaisePropertyChanged("ProviderID");
                }
            }
        }
        public string EntryID
        {
            get
            {
                return _EntryID;
            }

            set
            {
                if (_EntryID != value)
                {
                    _EntryID = value;
                    RaisePropertyChanged("EntryID");
                }
            }
        }
        public string LayerName
        {
            get
            {
                return _LayerName;
            }

            set
            {
                if (_LayerName != value)
                {
                    _LayerName = value;
                    RaisePropertyChanged("LayerName");
                }
            }
        }
        public DateTime LocalTimeStamp
        {
            get
            {
                return _LocalTimeStamp;
            }

            set
            {
                if (_LocalTimeStamp != value)
                {
                    _LocalTimeStamp = value;
                    RaisePropertyChanged("LocalTimeStamp");
                }
            }
        }
        public double Price
        {
            get
            {
                return _Price;
            }

            set
            {
                if (_Price != value)
                {
                    _Price = value;
                    RaisePropertyChanged("Price");
                    RaisePropertyChanged("FormattedPrice");
                }
            }
        }

        public DateTime ServerTimeStamp
        {
            get
            {
                return _ServerTimeStamp;
            }

            set
            {
                if (_ServerTimeStamp != value)
                {
                    _ServerTimeStamp = value;
                    RaisePropertyChanged("ServerTimeStamp");
                }
            }
        }
        public double Size
        {
            get
            {
                return _Size;
            }

            set
            {
                if (_Size != value)
                {
                    _Size = value;
                    RaisePropertyChanged("Size");
                    RaisePropertyChanged("FormattedSize");
                }
            }
        }
        public bool IsBid
        {
            get
            {
                return _IsBid;
            }

            set
            {
                if (_IsBid != value)
                {
                    _IsBid = value;
                    RaisePropertyChanged("IsBid");
                }
            }
        }

        public string FormattedPrice
        {
            get
            {
                return this.Price.ToString("N" + _DecimalPlaces);
            }
        }
        public string FormattedSize
        {
            get
            {
                return Helpers.HelperCommon.GetKiloFormatter(this.Size);
                //return this.Size.ToString("N0");
            }
        }

        public double? ActiveSize
        {
            get
            {
                return _ActiveSize;
            }

            set
            {
                if (_ActiveSize != value)
                {
                    _ActiveSize = value;
                    RaisePropertyChanged("ActiveSize");
                }
            }
        }
    }
}
