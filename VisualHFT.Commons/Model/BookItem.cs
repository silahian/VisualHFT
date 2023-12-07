using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VisualHFT.Commons.Model;

namespace VisualHFT.Model
{
    public partial class BookItem : IEquatable<BookItem>, IEqualityComparer<BookItem>, IResettable
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

        public BookItem()
        {
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

        public void Reset()
        {
            Symbol = "";
            ProviderID = 0;
            EntryID = "";
            LayerName = "";
            LocalTimeStamp = DateTime.MinValue;
            ServerTimeStamp = DateTime.MinValue;
            Price = 0;
            Size = 0;
            IsBid = false;
            DecimalPlaces = 0;
            ActiveSize = 0;
        }

        public int DecimalPlaces { get; set; }


        public string Symbol
        {
            get => _Symbol;
            set => _Symbol = value;
        }

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        public string EntryID
        {
            get => _EntryID;
            set => _EntryID = value;
        }

        public string LayerName
        {
            get => _LayerName;
            set => _LayerName = value;
        }

        public DateTime LocalTimeStamp
        {
            get => _LocalTimeStamp;
            set => _LocalTimeStamp = value;
        }

        public double? Price
        {
            get => _Price;
            set => _Price = value;
        }

        public DateTime ServerTimeStamp
        {
            get => _ServerTimeStamp;
            set => _ServerTimeStamp = value;
        }

        public double? Size
        {
            get => _Size;
            set => _Size = value;
        }

        public bool IsBid
        {
            get => _IsBid;
            set => _IsBid = value;
        }
        public string FormattedPrice => this.Price.HasValue ? this.Price.Value.ToString("N" + DecimalPlaces) : "";
        public string FormattedSize => this.Size.HasValue ? Helpers.HelperCommon.GetKiloFormatter(this.Size.Value) : "";
        public string FormattedActiveSize => this.ActiveSize.HasValue ? Helpers.HelperCommon.GetKiloFormatter(this.ActiveSize.Value) : "";

        public double? ActiveSize
        {
            get => _ActiveSize;
            set => _ActiveSize = value;
        }
    }
}
