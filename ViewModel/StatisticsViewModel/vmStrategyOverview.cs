using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ChartView;

namespace VisualHFT.ViewModel.StatisticsViewModel
{
    public class ChartDateCategoryDataPoint
    {
        private DateTime _date;
        private double _value;

        public DateTime Date
        {
            get
            {
                return _date;
            }

            set
            {
                _date = value;
            }
        }

        public double Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }

    public class vmStrategyOverview : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<PositionEx> _positions;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        public vmStrategyOverview(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            BlankFields();
        }
        private void _positions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CalculatePositionStats();
        }

        #region Fields
        //WINNERS
        string _winnersPnL;
        string _winnersAttempts;
        string _winnersSpan;
        //LOSERS
        string _losersPnL;
        string _losersAttempts;
        string _losersSpan;
        //ALL
        string _allPnL;
        string _allAttempts;
        string _allSpan;
        //PNL
        double _pnLAmount;
        double _winningRate;
        int _winningCount;
        int _loserCount;

        ObservableCollection<PlotInfo> _winningRateChartPoints;
        List<ChartDateCategoryDataPoint> _equityChartPoints;
        public ObservableCollection<PlotInfo> WinningRateChartPoints
        {
            get { return _winningRateChartPoints; }
            set
            {
				if (_winningRateChartPoints != value)
				{
					_winningRateChartPoints = value;
					RaisePropertyChanged();
				}
            }
        }
        public List<ChartDateCategoryDataPoint> EquityChartPoints
        {
            get
            {
                return _equityChartPoints;
            }

            set
            {
				if (_equityChartPoints != value)
				{
					_equityChartPoints = value;
				}				
            }
        }

        public string WinnersPnL
        {
            get
            {
                return _winnersPnL;
            }

            set
            {
                if (_winnersPnL != value)
                {
                    _winnersPnL = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WinnersAttempts
        {
            get
            {
                return _winnersAttempts;
            }

            set
            {
                if (_winnersAttempts != value)
                {
                    _winnersAttempts = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WinnersSpan
        {
            get
            {
                return _winnersSpan;
            }

            set
            {
                if (_winnersSpan != value)
                {
                    _winnersSpan = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string LosersPnL
        {
            get
            {
                return _losersPnL;
            }

            set
            {
                if (_losersPnL != value)
                {
                    _losersPnL = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string LosersAttempts
        {
            get
            {
                return _losersAttempts;
            }

            set
            {
                if (LosersAttempts != value)
                {
                    _losersAttempts = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string LosersSpan
        {
            get
            {
                return _losersSpan;
            }

            set
            {
                if (_losersSpan != value)
                {
                    _losersSpan = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string AllPnL
        {
            get
            {
                return _allPnL;
            }

            set
            {
                if (_allPnL != value)
                {
                    _allPnL = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string AllAttempts
        {
            get
            {
                return _allAttempts;
            }

            set
            {
                if (_allAttempts != value)
                {
                    _allAttempts = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string AllSpan
        {
            get
            {
                return _allSpan;
            }

            set
            {
                if (_allSpan != value)
                {
                    _allSpan = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double PnLAmount
        {
            get
            {
                return _pnLAmount;
            }

            set
            {
                if (_pnLAmount != value)
                {
                    _pnLAmount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double WinningRate
        {
            get
            {
                return _winningRate;
            }

            set
            {
                if (_winningRate != value)
                {
                    _winningRate = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int WinningCount
        {
            get
            {
                return _winningCount;
            }

            set
            {
                if (_winningCount != value)
                {
                    _winningCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int LoserCount
        {
            get
            {
                return _loserCount;
            }

            set
            {
                if (_loserCount != value)
                {
                    _loserCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<PositionEx> Positions
        {
            get
            {
                return _positions;
            }

            set
            {
                _positions = value;
                if (_positions != null)
                    _positions.CollectionChanged += _positions_CollectionChanged;
                CalculatePositionStats();
            }
        }


        #endregion
        void BlankFields()
        {
            WinnersPnL = "";
            WinnersAttempts = "";
            WinnersSpan = "";
            LosersPnL = "";
            LosersAttempts = "";
            LosersSpan = "";
            AllPnL = "";
            AllAttempts = "";
            AllSpan = "";
            PnLAmount = 0;
            WinningRate = 0;
            WinningCount = 0;
            LoserCount = 0;
            WinningRateChartPoints = new ObservableCollection<PlotInfo>();
            WinningRateChartPoints.Add(new PlotInfo() { Value = 0 });
            WinningRateChartPoints.Add(new PlotInfo() { Value = 0 });
            EquityChartPoints = new List<ChartDateCategoryDataPoint>();
        }
        void CalculatePositionStats()
        {
            if (_positions == null || !_positions.Any())
            {
                BlankFields();
                return;
            }

            try
            {
                //ALL
                AllPnL = "Avg PnL: " + _positions.Where(x => x.PipsPnLInCurrency.HasValue).DefaultIfEmpty(new Position() { PipsPnLInCurrency = 0m }).Average(x => x.PipsPnLInCurrency.Value).ToString("C0");
                AllAttempts = "Avg Attempts: " + _positions.Average(x => (double)x.AttemptsToClose).ToString("N1");
                AllSpan = "Avg Span: " + Helpers.HelperCommon.GetKiloFormatterTime(_positions.Average(x => (x.CloseTimeStamp - x.CreationTimeStamp).TotalMilliseconds));

                var losers = _positions.Where(x => x.GetPipsPnL < 0).DefaultIfEmpty(new PositionEx());
                var winners = _positions.Where(x => x.GetPipsPnL >= 0).DefaultIfEmpty(new PositionEx());
                foreach(var item in losers)
                {
                    if (!item.PipsPnLInCurrency.HasValue)
                        item.PipsPnLInCurrency = item.GetPipsPnL;
                }
                foreach (var item in winners)
                {
                    if (!item.PipsPnLInCurrency.HasValue)
                        item.PipsPnLInCurrency = item.GetPipsPnL;
                }




                if (losers != null && losers.Count() > 0)
                {
                    LoserCount = losers.Count();
                    LosersPnL = "Avg PnL: " + losers.Average(x => x.PipsPnLInCurrency.Value).ToString("C0");
                    LosersAttempts = "Avg Attempts: " + losers.Average(x => (double)x.AttemptsToClose).ToString("N1");
                    LosersSpan = "Avg Span: " + Helpers.HelperCommon.GetKiloFormatterTime(losers.Average(x => (x.CloseTimeStamp - x.CreationTimeStamp).TotalMilliseconds));
                }
                if (winners != null && winners.Count() > 0)
                {
                    WinningCount = winners.Count();
                    WinnersPnL = "Avg PnL: " + winners.Average(x => x.PipsPnLInCurrency.Value).ToString("C0");
                    WinnersAttempts = "Avg Attempts: " + winners.Average(x => (double)x.AttemptsToClose).ToString("N1");
                    WinnersSpan = "Avg Span: " + Helpers.HelperCommon.GetKiloFormatterTime(winners.Average(x => (x.CloseTimeStamp - x.CreationTimeStamp).TotalMilliseconds));
                }
                WinningRateChartPoints[0].Value = _winningCount;
                WinningRateChartPoints[1].Value = _loserCount;
                //RaisePropertyChanged("WinningRateChartPoints");

                if (losers != null && losers.Count() > 0 && winners != null && winners.Count() > 0)
                    WinningRate = ((double)winners.Count() / (double)_positions.Count);
                else if (winners != null && winners.Count() > 0 && (losers == null || losers.Count() == 0))
                    WinningRate = 1;
                else if (losers != null && losers.Count() > 0 && (winners == null || winners.Count() == 0))
                    WinningRate = 0;


                //GET Equity
                var max = _positions.Max(x => x.CloseTimeStamp);
                var min = _positions.Min(x => x.CreationTimeStamp);
                List<cEquity> equity = null;
                if (max.Subtract(min).TotalHours <= 1)
                    equity = Helpers.HelperPositionAnalysis.GetEquityCurve(_positions.ToList());
                else
                    equity = Helpers.HelperPositionAnalysis.GetEquityCurveByHour(_positions.ToList());
                if (equity != null && equity.Count > 0)
                {
                    EquityChartPoints = equity.OrderBy(x => x.Date).Select(x => new ChartDateCategoryDataPoint() { Date = x.Date, Value = x.Equity.ToDouble()}).ToList();
                    double iniEquity = equity.First().Equity.ToDouble();
                    double endEquity = equity.Last().Equity.ToDouble();

                    PnLAmount = endEquity;
                    RaisePropertyChanged("EquityChartPoints");
                }
            }
            catch (Exception ex){  }
        }

    }
}
