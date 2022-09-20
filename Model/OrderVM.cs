using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
    public class OrderVM
    {
        public OrderVM()
        {
            this.IsEmpty = true;
        }
        public string ProviderName { get; set; }
        public long OrderID{ get; set; }
        string StrategyCode{ get; set; }
        public string Symbol{ get; set; }
        public int ProviderId{ get; set; }
        public string ClOrdId{ get; set; }
        public eORDERSIDE Side { get; set; }
        public eORDERTYPE OrderType { get; set; }
        public eORDERTIMEINFORCE TimeInForce { get; set; }
        public eORDERSTATUS Status { get; set; }
        public double Quantity{ get; set; }
        public double MinQuantity{ get; set; }
        public double FilledQuantity{ get; set; }
        public double PricePlaced{ get; set; }
        public string Currency{ get; set; }
        public string FutSettDate{ get; set; }
        public bool IsMM{ get; set; }
        public bool IsEmpty{ get; set; }
        public string LayerName{ get; set; }
        public int AttemptsToClose{ get; set; }
        public int SymbolMultiplier{ get; set; }
        public int SymbolDecimals{ get; set; }
        public string FreeText{ get; set; }
        public string OriginPartyID{ get; set; }
        public IEnumerable<OpenExecution> Executions{ get; set; }
        public int QuoteID { get; set; }
        public DateTime QuoteServerTimeStamp{ get; set; }
        public DateTime QuoteLocalTimeStamp{ get; set; }
        public DateTime CreationTimeStamp{ get; set; } 
        public DateTime ExecutedTimeStamp{ get; set; }
        public DateTime FireSignalTimestamp{ get; set; }
        public double StopLoss{ get; set; }
        public double TakeProfit{ get; set; }
        public bool PipsTrail{ get; set; }
        public double UnrealizedPnL{ get; set; }
        public double MaxDrowdown{ get; set; }
        public double BestBid{ get; set; }
        public double BestAsk{ get; set; }

        public double GetAvgPrice { get; set; }
        public double GetQuantity { get; set; }
    }
}
