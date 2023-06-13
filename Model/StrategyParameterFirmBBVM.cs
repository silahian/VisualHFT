using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace VisualHFT.Model
{
    public partial class 
        STRATEGY_PARAMETERS_FIRMBB
    {
        public void CopyTo(StrategyParametersFirmBBVM item)
        {
            item.PositionSize = this.PositionSize;
            item.MaximumExposure = this.MaximumExposure;
            item.LookUpBookForSize = this.LookUpBookForSize;
            item.PipsMarkupAsk = this.PipsMarkupAsk;
            item.PipsMarkupBid = this.PipsMarkupBid;
            item.MinPipsDiffToUpdatePrice = this.MinPipsDiffToUpdatePrice;
            item.MinSpread = this.MinSpread;
            item.PipsSlippage = this.PipsSlippage;
            item.AggressingToHedge = this.AggressingToHedge;
            item.PipsSlippageToHedge = this.PipsSlippageToHedge;
            item.PipsHedgeStopLoss = this.PipsHedgeStopLoss;
            item.PipsHedgeTakeProf = this.PipsHedgeTakeProf;
            item.PipsHedgeTrailing = this.PipsHedgeTrailing;
            item.TickSample = this.TickSample;
            item.BollingerPeriod = this.BollingerPeriod;
            item.BollingerStdDev = this.BollingerStdDev;
        }
    }
    public class StrategyParametersFirmBBVM : STRATEGY_PARAMETERS_FIRMBB, INotifyPropertyChanged, IStrategyParameters
	{

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public StrategyParametersFirmBBVM()
        { }
        public StrategyParametersFirmBBVM(STRATEGY_PARAMETERS_FIRMBB item)
        {
            this.Symbol = item.Symbol;
            this.LayerName = item.LayerName;
            this.PositionSize = item.PositionSize;
            this.MaximumExposure = item.MaximumExposure;
            this.LookUpBookForSize = item.LookUpBookForSize;
            this.PipsMarkupAsk = item.PipsMarkupAsk;
            this.PipsMarkupBid = item.PipsMarkupBid;
            this.MinPipsDiffToUpdatePrice = item.MinPipsDiffToUpdatePrice;
            this.MinSpread = item.MinSpread;
            this.PipsSlippage = item.PipsSlippage;
            this.AggressingToHedge = item.AggressingToHedge;
            this.PipsSlippageToHedge = item.PipsSlippageToHedge;
            this.PipsHedgeStopLoss = item.PipsHedgeStopLoss;
            this.PipsHedgeTakeProf = item.PipsHedgeTakeProf;
            this.PipsHedgeTrailing = item.PipsHedgeTrailing;
            this.TickSample = item.TickSample;
            this.BollingerPeriod = item.BollingerPeriod;
            this.BollingerStdDev = item.BollingerStdDev;
        }

        public STRATEGY_PARAMETERS_FIRMBB ThisToDBObject()
        {
            var item = new STRATEGY_PARAMETERS_FIRMBB();
            item.Symbol = this.Symbol;
            item.LayerName = this.LayerName;
            item.PositionSize = this.PositionSize;
            item.MaximumExposure = this.MaximumExposure;
            item.LookUpBookForSize = this.LookUpBookForSize;
            item.PipsMarkupAsk = this.PipsMarkupAsk;
            item.PipsMarkupBid = this.PipsMarkupBid;
            item.MinPipsDiffToUpdatePrice = this.MinPipsDiffToUpdatePrice;
            item.MinSpread = this.MinSpread;
            item.PipsSlippage = this.PipsSlippage;
            item.AggressingToHedge = this.AggressingToHedge;
            item.PipsSlippageToHedge = this.PipsSlippageToHedge;
            item.PipsHedgeStopLoss = this.PipsHedgeStopLoss;
            item.PipsHedgeTakeProf = this.PipsHedgeTakeProf;
            item.PipsHedgeTrailing = this.PipsHedgeTrailing;            
            item.TickSample = this.TickSample;
            item.BollingerPeriod = this.BollingerPeriod;
            item.BollingerStdDev = this.BollingerStdDev;
            return item;
        }

        public new string Symbol
        {
            get { return base.Symbol; }
            set
            {
                if (base.Symbol != value)
                {
                    base.Symbol = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PositionSize
        {
            get { return base.PositionSize; }
            set
            {
                if (base.PositionSize != value)
                {
                    base.PositionSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal MaximumExposure
        {
            get { return base.MaximumExposure; }
            set
            {
                if (base.MaximumExposure != value)
                {
                    base.MaximumExposure = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal LookUpBookForSize
        {
            get { return base.LookUpBookForSize; }
            set
            {
                if (base.LookUpBookForSize != value)
                {
                    base.LookUpBookForSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsMarkupAsk
        {
            get { return base.PipsMarkupAsk; }
            set
            {
                if (base.PipsMarkupAsk != value)
                {
                    base.PipsMarkupAsk = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsMarkupBid
        {
            get { return base.PipsMarkupBid; }
            set
            {
                if (base.PipsMarkupBid != value)
                {
                    base.PipsMarkupBid = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal MinPipsDiffToUpdatePrice
        {
            get { return base.MinPipsDiffToUpdatePrice; }
            set
            {
                if (base.MinPipsDiffToUpdatePrice != value)
                {
                    base.MinPipsDiffToUpdatePrice = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal MinSpread
        {
            get { return base.MinSpread; }
            set
            {
                if (base.MinSpread != value)
                {
                    base.MinSpread = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsSlippage
        {
            get { return base.PipsSlippage; }
            set
            {
                if (base.PipsSlippage != value)
                {
                    base.PipsSlippage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new bool AggressingToHedge
        {
            get { return base.AggressingToHedge; }
            set
            {
                if (base.AggressingToHedge != value)
                {
                    base.AggressingToHedge = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsSlippageToHedge
        {
            get { return base.PipsSlippageToHedge; }
            set
            {
                if (base.PipsSlippageToHedge != value)
                {
                    base.PipsSlippageToHedge = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsHedgeStopLoss
        {
            get { return base.PipsHedgeStopLoss; }
            set
            {
                if (base.PipsHedgeStopLoss != value)
                {
                    base.PipsHedgeStopLoss = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsHedgeTakeProf
        {
            get { return base.PipsHedgeTakeProf; }
            set
            {
                if (base.PipsHedgeTakeProf != value)
                {
                    base.PipsHedgeTakeProf = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal PipsHedgeTrailing
        {
            get { return base.PipsHedgeTrailing; }
            set
            {
                if (base.PipsHedgeTrailing != value)
                {
                    base.PipsHedgeTrailing = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new int TickSample
        {
            get { return base.TickSample; }
            set
            {
                if (base.TickSample != value)
                {
                    base.TickSample = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new int BollingerPeriod
        {
            get { return base.BollingerPeriod; }
            set
            {
                if (base.BollingerPeriod != value)
                {
                    base.BollingerPeriod = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal BollingerStdDev
        {
            get { return base.BollingerStdDev; }
            set
            {
                if (base.BollingerStdDev != value)
                {
                    base.BollingerStdDev = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private bool _IsStrategyOn;
        private int _DecimalPlaces;
        private double _SentBidPrice;
        private double _SentAskPrice;
        private int _SentBidSize;
        private int _SentAskSize;
        private int _HedgeCancelRejectedQty;
        private int _HedgeFilledQty;


        //INFORMATION
        public bool IsStrategyOn
        {
            get { return _IsStrategyOn; }
            set
            {
                if (_IsStrategyOn != value)
                {
                    _IsStrategyOn = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int DecimalPlaces
        {
            get { return _DecimalPlaces; }
            set
            {
                if (_DecimalPlaces != value)
                {
                    _DecimalPlaces = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double SentBidPrice
        {
            get
            { return _SentBidPrice; }
            set
            {
                if (_SentBidPrice != value)
                {
                    _SentBidPrice = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double SentAskPrice
        {
            get
            { return _SentAskPrice; }
            set
            {
                if (_SentAskPrice != value)
                {
                    _SentAskPrice = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int SentBidSize
        {
            get
            { return _SentBidSize; }
            set
            {
                if (_SentBidSize != value)
                {
                    _SentBidSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int SentAskSize
        {
            get
            { return _SentAskSize; }
            set
            {
                if (_SentAskSize != value)
                {
                    _SentAskSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int HedgeCancelRejectedQty
        {
            get
            { return _HedgeCancelRejectedQty; }
            set
            {
                if (_HedgeCancelRejectedQty != value)
                {
                    _HedgeCancelRejectedQty = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int HedgeFilledQty
        {
            get
            { return _HedgeFilledQty; }
            set
            {
                if (_HedgeFilledQty != value)
                {
                    _HedgeFilledQty = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
