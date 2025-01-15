using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Prism.Mvvm;

namespace VisualHFT.ViewModel.StatisticsViewModel
{

    public class vmStrategyOverview : BindableBase
    {

        private ObservableCollection<VisualHFT.Model.Position> _positions;
        public vmStrategyOverview()
        {
            _positions = new ObservableCollection<VisualHFT.Model.Position>();
            BlankFields();
        }

        private Dictionary<string, Func<string, string, bool>> _dialogs;
        public vmStrategyOverview(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            _positions = new ObservableCollection<VisualHFT.Model.Position>();
            _dialogs = dialogs;
            BlankFields();
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
            get => _winningRateChartPoints;
            set => SetProperty(ref _winningRateChartPoints, value);

        }
        public List<ChartDateCategoryDataPoint> EquityChartPoints
        {
            get => _equityChartPoints;
            set => SetProperty(ref _equityChartPoints, value);
        }

        public string WinnersPnL
        {
            get => _winnersPnL;
            set => SetProperty(ref _winnersPnL, value);
        }

        public string WinnersAttempts
        {
            get => _winnersAttempts; 
            set => SetProperty(ref _winnersAttempts, value);
        }

        public string WinnersSpan
        {
            get => _winnersSpan;
            set => SetProperty(ref _winnersSpan, value);
        }

        public string LosersPnL
        {
            get => _losersPnL;
            set => SetProperty(ref _losersPnL, value);
        }

        public string LosersAttempts
        {
            get => _losersAttempts;
            set => SetProperty(ref _losersAttempts, value);
        }

        public string LosersSpan
        {
            get => _losersSpan; 
            set => SetProperty(ref _losersSpan, value);
        }

        public string AllPnL
        {
            get => _allPnL;
            set => SetProperty(ref _allPnL, value);
        }

        public string AllAttempts
        {
            get => _allAttempts;
            set => SetProperty(ref _allAttempts, value);
        }

        public string AllSpan
        {
            get => _allSpan;
            set => SetProperty(ref _allSpan, value);
        }

        public double PnLAmount
        {
            get => _pnLAmount;
            set => SetProperty(ref _pnLAmount, value);
        }

        public double WinningRate
        {
            get => _winningRate;
            set => SetProperty(ref _winningRate, value);
        }
        public int WinningCount
        {
            get => _winningCount;
            set => SetProperty(ref _winningCount, value);
        }

        public int LoserCount
        {
            get => _loserCount;
            set => SetProperty(ref _loserCount, value);
        }
        public ObservableCollection<VisualHFT.Model.Position> Positions => _positions;
        public void AddNewPositions(IEnumerable<VisualHFT.Model.Position> pos)
        {
            foreach (var position in pos)
                _positions.Add(position);
            CalculatePositionStats();
        }
        public void AddNewPosition(VisualHFT.Model.Position p)
        {
            _positions.Add(p);
            CalculatePositionStats();
        }
        public void ClearPositions()
        {
            _positions.Clear();
            CalculatePositionStats();
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
            WinningRateChartPoints = new ObservableCollection<PlotInfo>
            {
                new PlotInfo() { Value = 0 },
                new PlotInfo() { Value = 0 }
            };
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
                var losers = _positions.Where(x => x.GetPipsPnL < 0).DefaultIfEmpty(new VisualHFT.Model.Position()).ToList();
                var winners = _positions.Where(x => x.GetPipsPnL >= 0).DefaultIfEmpty(new VisualHFT.Model.Position()).ToList();
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


                //ALL

                this.AllPnL = "Avg PnL: " + _positions.Where(x => x.PipsPnLInCurrency.HasValue).DefaultIfEmpty(new VisualHFT.Model.Position() { PipsPnLInCurrency = 0m }).Average(x => x.PipsPnLInCurrency.Value).ToString("C0");
                this.AllAttempts = "Avg Attempts: " + _positions.Average(x => (double)x.AttemptsToClose).ToString("N1");
                this.AllSpan = "Avg Span: " + Helpers.HelperCommon.GetKiloFormatterTime(_positions.Average(x => (x.CloseTimeStamp - x.CreationTimeStamp).TotalMilliseconds));
                

                if (losers != null && losers.Count() > 0)
                {
                    this.LoserCount = losers.Count();
                    this.LosersPnL = "Avg PnL: " + losers.Where(x => x.PipsPnLInCurrency.HasValue).DefaultIfEmpty(new VisualHFT.Model.Position() { PipsPnLInCurrency = 0m }).Average(x => x.PipsPnLInCurrency.Value).ToString("C2");
                    this.LosersAttempts = "Avg Attempts: " + losers.Average(x => (double)x.AttemptsToClose).ToString("N1");
                    this.LosersSpan = "Avg Span: " + Helpers.HelperCommon.GetKiloFormatterTime(losers.Average(x => (x.CloseTimeStamp - x.CreationTimeStamp).TotalMilliseconds));
                }
                if (winners != null && winners.Count() > 0)
                {
                    this.WinningCount = winners.Count();
                    this.WinnersPnL = "Avg PnL: " + winners.Where(x => x.PipsPnLInCurrency.HasValue).DefaultIfEmpty(new VisualHFT.Model.Position() { PipsPnLInCurrency = 0m }).Average(x => x.PipsPnLInCurrency.Value).ToString("C2");
                    this.WinnersAttempts = "Avg Attempts: " + winners.Average(x => (double)x.AttemptsToClose).ToString("N1");
                    this.WinnersSpan = "Avg Span: " + Helpers.HelperCommon.GetKiloFormatterTime(winners.Average(x => (x.CloseTimeStamp - x.CreationTimeStamp).TotalMilliseconds));
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
            catch (Exception ex)
            {  
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
