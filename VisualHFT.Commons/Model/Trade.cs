namespace VisualHFT.Model
{
    public class Trade
    {
        private int _providerId;
        private string _providerName;
        private string _symbol;
        private decimal _price;
        private decimal _size;
        private DateTime _timestamp;
        private bool? _isBuy;
        private string _flags;
        private double _midMarketPrice;

        public int ProviderId { get => _providerId; set => _providerId = value; }
        public string ProviderName { get => _providerName; set => _providerName = value; }
        public string Symbol { get => _symbol; set => _symbol = value; }
        public decimal Price { get => _price; set => _price = value; }
        public decimal Size { get => _size; set => _size = value; }
        public DateTime Timestamp { get => _timestamp; set => _timestamp = value; }
        public bool? IsBuy { get => _isBuy; set => _isBuy = value; }
        public string Flags { get => _flags; set => _flags = value; }
        public double MarketMidPrice { get => _midMarketPrice; set => _midMarketPrice = value; }

        internal void CopyTo(Trade target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            target.Symbol = Symbol;
            target.Price = Price;
            target.Size = Size;
            target.Timestamp = Timestamp;
            target.IsBuy = IsBuy;
            target.Flags = Flags;
            target.ProviderId = ProviderId;
            target.ProviderName = ProviderName;
            target.MarketMidPrice = MarketMidPrice;

        }
    }
}