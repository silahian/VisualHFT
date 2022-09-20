using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
namespace VisualHFT.Model
{
    public partial class STRATEGY_PARAMETERS_BBOOK
    {
        public void CopyTo(StrategyParametersBBookVM item)
        {
            item.PositionSize = this.PositionSize;
			item.PipsArb = this.PipsArb;
			item.MillisecondsToWaitBeofreClosing = this.MillisecondsToWaitBeofreClosing;
			item.PNLoverallPositionToClose = this.PNLoverallPositionToClose;
			item.ClosingWaitingBBook = this.ClosingWaitingBBook;
			item.ClosingWaitingTime = this.ClosingWaitingTime;
			item.AfterCloseWaitForMillisec = this.AfterCloseWaitForMillisec;

            item.PipsHedgeStopLoss = this.PipsHedgeStopLoss;
            item.PipsHedgeTakeProf = this.PipsHedgeTakeProf;
            item.PipsHedgeTrailing = this.PipsHedgeTrailing;
        }
    }
    public class StrategyParametersBBookVM : STRATEGY_PARAMETERS_BBOOK, INotifyPropertyChanged, IStrategyParameters
	{

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public StrategyParametersBBookVM()
        { }
        public StrategyParametersBBookVM(STRATEGY_PARAMETERS_BBOOK item)
        {
            this.Symbol = item.Symbol;
            this.LayerName = item.LayerName;
            this.PositionSize = item.PositionSize;
			this.PipsArb = item.PipsArb;
			this.MillisecondsToWaitBeofreClosing = item.MillisecondsToWaitBeofreClosing;
			this.PNLoverallPositionToClose = item.PNLoverallPositionToClose;
			this.ClosingWaitingBBook = item.ClosingWaitingBBook;
			this.ClosingWaitingTime = item.ClosingWaitingTime;
			this.AfterCloseWaitForMillisec = item.AfterCloseWaitForMillisec;

			this.PipsHedgeStopLoss = item.PipsHedgeStopLoss;
            this.PipsHedgeTakeProf = item.PipsHedgeTakeProf;
            this.PipsHedgeTrailing = item.PipsHedgeTrailing;
        }

        public STRATEGY_PARAMETERS_BBOOK ThisToDBObject()
        {
            var item = new STRATEGY_PARAMETERS_BBOOK();
            item.Symbol = this.Symbol;
            item.LayerName = this.LayerName;
            item.PositionSize = this.PositionSize;
			item.PipsArb = this.PipsArb;
			item.MillisecondsToWaitBeofreClosing = this.MillisecondsToWaitBeofreClosing;
			item.PNLoverallPositionToClose = this.PNLoverallPositionToClose;
			item.ClosingWaitingBBook = this.ClosingWaitingBBook;
			item.ClosingWaitingTime = this.ClosingWaitingTime;
			item.AfterCloseWaitForMillisec = this.AfterCloseWaitForMillisec;

			item.PipsHedgeStopLoss = this.PipsHedgeStopLoss;
            item.PipsHedgeTakeProf = this.PipsHedgeTakeProf;
            item.PipsHedgeTrailing = this.PipsHedgeTrailing;
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
        public new decimal PipsArb
        {
            get { return base.PipsArb; }
            set
            {
                if (base.PipsArb != value)
                {
                    base.PipsArb = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public new decimal MillisecondsToWaitBeofreClosing
		{
            get { return base.MillisecondsToWaitBeofreClosing; }
            set
            {
                if (base.MillisecondsToWaitBeofreClosing != value)
                {
                    base.MillisecondsToWaitBeofreClosing = value;
                    NotifyPropertyChanged();
                }
            }
        }
		public new decimal PNLoverallPositionToClose
		{
			get { return base.PNLoverallPositionToClose; }
			set
			{
				if (base.PNLoverallPositionToClose != value)
				{
					base.PNLoverallPositionToClose = value;
					NotifyPropertyChanged();
				}
			}
		}
		public new bool ClosingWaitingBBook
		{
			get { return base.ClosingWaitingBBook; }
			set
			{
				if (base.ClosingWaitingBBook != value)
				{
					base.ClosingWaitingBBook = value;
					NotifyPropertyChanged();
				}
			}
		}
		public new bool ClosingWaitingTime
		{
			get { return base.ClosingWaitingTime; }
			set
			{
				if (base.ClosingWaitingTime != value)
				{
					base.ClosingWaitingTime = value;
					NotifyPropertyChanged();
				}
			}
		}
		public new decimal AfterCloseWaitForMillisec
		{
			get { return base.AfterCloseWaitForMillisec; }
			set
			{
				if (base.AfterCloseWaitForMillisec != value)
				{
					base.AfterCloseWaitForMillisec = value;
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


        private bool _IsStrategyOn;
        private int _DecimalPlaces;


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
    }
}
