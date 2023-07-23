using Newtonsoft.Json;
using Prism.Mvvm;
using System;

namespace VisualHFT.Model
{
    public class BookItem : BindableBase, IEquatable<BookItem>
    {
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


        public int DecimalPlaces { get; set; }


        public string Symbol
        {
            get => _Symbol;
            set => SetProperty(ref _Symbol, value);
        }

        public int ProviderID
        {
            get => _ProviderID;
            set => SetProperty(ref _ProviderID, value);
        }

        public string EntryID
        {
            get => _EntryID;
            set => SetProperty(ref _EntryID, value);
        }

        public string LayerName
        {
            get => _LayerName;
            set => SetProperty(ref _LayerName, value);
        }

        public DateTime LocalTimeStamp
        {
            get => _LocalTimeStamp;
            set => SetProperty(ref _LocalTimeStamp, value);
        }

        public double? Price
        {
            get => _Price;
            set => SetProperty(ref _Price, value, onChanged: () => RaisePropertyChanged(nameof(FormattedPrice)));
        }

        public DateTime ServerTimeStamp
        {
            get => _ServerTimeStamp;
            set => SetProperty(ref _ServerTimeStamp, value);
        }

        public double? Size
        {
            get => _Size;
            set => SetProperty(ref _Size, value, onChanged: () => RaisePropertyChanged(nameof(FormattedPrice)));
        }

        public bool IsBid
        {
            get => _IsBid;
            set => SetProperty(ref _IsBid, value);
        }
        public string FormattedPrice => this.Price.HasValue ? this.Price.Value.ToString("N" + DecimalPlaces) : "";
        public string FormattedSize => this.Size.HasValue ? Helpers.HelperCommon.GetKiloFormatter(this.Size.Value) : "";

        public double? ActiveSize
        {
            get => _ActiveSize;
            set => SetProperty(ref _ActiveSize, value);
        }
    }
}
