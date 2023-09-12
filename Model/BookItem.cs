using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace VisualHFT.Model
{
    public class BookItem : BindableBase, IEquatable<BookItem>, IEqualityComparer<BookItem>
    {

        private string _Symbol;
        private int _ProviderID;
        private string _EntryID;
        private string _LayerName;
        private DateTime _LocalTimeStamp;
        private double? _Price;
        private DateTime _ServerTimeStamp;
        private double? _Size;
        private bool _IsBid;
        private double? _ActiveSize;

        public BookItem(bool isBindable = false)
        {
            IsBindable = isBindable;
        }

        public void Update(BookItem b)
        {
            Symbol = b.Symbol;
            ProviderID = b.ProviderID;
            EntryID = b.EntryID;
            LayerName = b.LayerName;
            LocalTimeStamp = b.LocalTimeStamp;
            ServerTimeStamp = b.ServerTimeStamp;
            Price = b.Price;
            Size = b.Size;
            IsBid = b.IsBid;
            DecimalPlaces = b.DecimalPlaces;
            ActiveSize = b.ActiveSize;
        }

        public bool Equals(BookItem other)
        {
            if (other == null)
                return false;
            if (IsBid != other.IsBid)
                return false;
            if (EntryID != other.EntryID)
                return false;
            if (Price != other.Price)
                return false;
            if (Size != other.Size)
                return false;
            return true;

        }

        public bool Equals(BookItem x, BookItem y)
        {
            return x.Price == y.Price;
        }

        public int GetHashCode(BookItem obj)
        {
            return obj.Price.GetHashCode();
        }

        public bool IsBindable { get; set; } = true;
        public int DecimalPlaces { get; set; }


        public string Symbol
        {
            get => _Symbol;
            set 
            {
                if (IsBindable)
                    SetProperty(ref _Symbol, value);
                else
                    _Symbol = value;
            }
        }

        public int ProviderID
        {
            get => _ProviderID;
            set
            {
                if (IsBindable)
                    SetProperty(ref _ProviderID, value);
                else
                    _ProviderID = value;
            }
        }

        public string EntryID
        {
            get => _EntryID;
            set 
            {
                if (IsBindable)
                    SetProperty(ref _EntryID, value); 
                else
                    _EntryID = value;
            }
        }

        public string LayerName
        {
            get => _LayerName;
            set
            {
                if (IsBindable)
                    SetProperty(ref _LayerName, value);
                else
                    _LayerName = value;
            }
        }

        public DateTime LocalTimeStamp
        {
            get => _LocalTimeStamp;
            set
            {
                if (IsBindable)
                    SetProperty(ref _LocalTimeStamp, value);
                else
                    _LocalTimeStamp = value;
            }
        }

        public double? Price
        {
            get => _Price;
            set
            {
                if (IsBindable)
                    SetProperty(ref _Price, value, onChanged: () => RaisePropertyChanged(nameof(FormattedPrice)));
                else
                    _Price = value;
            }
        }

        public DateTime ServerTimeStamp
        {
            get => _ServerTimeStamp;
            set
            {
                if (IsBindable)
                    SetProperty(ref _ServerTimeStamp, value);
                else
                    _ServerTimeStamp = value;
            }
        }

        public double? Size
        {
            get => _Size;
            set
            {
                if (IsBindable)
                    SetProperty(ref _Size, value, onChanged: () => RaisePropertyChanged(nameof(FormattedSize)));
                else
                    _Size = value;
            }
        }

        public bool IsBid
        {
            get => _IsBid;
            set
            {
                if (IsBindable)
                    SetProperty(ref _IsBid, value);
                else
                    _IsBid = value;
            }
        }
        public string FormattedPrice => this.Price.HasValue ? this.Price.Value.ToString("N" + DecimalPlaces) : "";
        public string FormattedSize => this.Size.HasValue ? Helpers.HelperCommon.GetKiloFormatter(this.Size.Value) : "";

        public double? ActiveSize
        {
            get => _ActiveSize;
            set
            {
                if (IsBindable)
                    SetProperty(ref _ActiveSize, value);
                else
                    _ActiveSize = value;
            }
        }
    }
}
