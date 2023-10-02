using PropertyChanged;

namespace VisualHFT.ViewModel.Model
{
    [AddINotifyPropertyChangedInterface]
    public class Trade : VisualHFT.Model.Trade
    {
        public Trade(VisualHFT.Model.Trade t)
        {

            this.ProviderId = t.ProviderId;
            this.ProviderName = t.ProviderName;
            this.IsBuy = t.IsBuy;
            this.Symbol = t.Symbol;
            this.Size = t.Size;
            this.Price = t.Price;
            this.Flags = t.Flags;
            this.Timestamp = t.Timestamp;
        }
    }
}
