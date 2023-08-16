using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class vmStats : BindableBase
    {
        public List<PositionEx> Signals { get; set; }
        public string WinLosses { get; private set; }
        public string PayoutRatio { get; private set; }
        public string AvgBarsTrade { get; private set; }
        public string AHPR { get; private set; }
        public string ZScore { get; private set; }
        public string ZProbability { get; private set; }
        public string Expectancy { get; private set; }
        public string Deviation { get; private set; }
        public string Volatility { get; private set; }
        public string StagnationDays { get; private set; }
        public string StagnationPerc { get; private set; }
        public string NumWins { get; private set; }
        public string NumLosses { get; private set; }
        public string GrossProfit { get; private set; }
        public string GrossLoss { get; private set; }
        public string AverageWin { get; private set; }
        public string AverageLoss { get; private set; }
        public string LargestWin { get; private set; }
        public string LargestLoss { get; private set; }
        public string MaxConsecWins { get; private set; }
        public string MaxConsecLosses { get; private set; }
        public string AvgConsecWins { get; private set; }
        public string AvgConsecLoss { get; private set; }
        public string AvgNumBarsInWins { get; private set; }
        public string AvgNumBarsInLosses { get; private set; }

        public void LoadData(List<PositionEx> signals)
        {
            this.Signals = signals;
            if (this.Signals == null || this.Signals.Count == 0)
                throw new Exception("No signals found.");

            Position oSignal = this.Signals[0];
            if (oSignal != null)
            {
                double totalCount = this.Signals.Count;
                decimal totalReturn = this.Signals.Sum(s => s.GetPipsPnL);
                double winCount = this.Signals.Where(s => s.GetPipsPnL >= 0).Count();
                double lossCount = this.Signals.Where(s => s.GetPipsPnL < 0).Count();
                double grossProfit = this.Signals.Where(s => s.GetPipsPnL >= 0 && s.PipsPnLInCurrency.HasValue).Sum(s => s.PipsPnLInCurrency.ToDouble());
                double grossLoss = this.Signals.Where(s => s.GetPipsPnL < 0 && s.PipsPnLInCurrency.HasValue).Sum(s => s.PipsPnLInCurrency.ToDouble());
                double avgWinPerc = 0.00;
                double avgLossPerc = 0.00;
                if (this.Signals.Where(s => s.GetPipsPnL >= 0).Count() > 0)
                    avgWinPerc = this.Signals.Where(s => s.GetPipsPnL >= 0 && s.PipsPnLInCurrency.HasValue).Average(x => x.PipsPnLInCurrency.ToDouble());
                if (this.Signals.Where(s => s.GetPipsPnL < 0).Count() > 0)
                    avgLossPerc = this.Signals.Where(s => s.GetPipsPnL < 0 && s.PipsPnLInCurrency.HasValue).Average(x => x.PipsPnLInCurrency.ToDouble());
                #region STRATEGY
                //Win/loss ratio
                WinLosses = (winCount / lossCount).ToString("n2");
                //Payout Ratio (Avg win/loss)
                if (avgLossPerc != 0)
                    PayoutRatio = (avgWinPerc / Math.Abs(avgLossPerc)).ToString("n2");
                //Average bars in trade (in seconds)
                AvgBarsTrade = signals.Where(x => x.CloseTimeStamp > x.CreationTimeStamp).Average(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds).ToString("n2");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************

                //AHPR
                AHPR = HelperAnalytics.GetAHPR(this.Signals).ToString("p2");
                //Z-Score
                ZScore = HelperAnalytics.GetZScore(this.Signals).ToString("n2");
                //Z-Probability
                ZProbability = HelperAnalytics.GetZProbability(this.Signals).ToString("p2");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************


                //Expectancy
                Expectancy = HelperAnalytics.GetExpectancy(this.Signals).ToString("n2");

                //Deviation
                Deviation = this.Signals.Where(x => x.GetPipsPnL >= 0).Select(x => ((winCount / totalCount) * x.PipsPnLInCurrency.ToDouble())).Concat(
                                        this.Signals.Where(x => x.GetPipsPnL < 0).Select(x => ((lossCount / totalCount) * x.PipsPnLInCurrency.ToDouble()))).StdDev().ToString("n2");
                //Volatility
                var equity = HelperAnalytics.GetEquityCurveByHour(this.Signals);
                Volatility = equity.Select(x => x.Equity.ToDouble()).StdDev().ToString("n2");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************

                //Stagnation in Days
                List<KeyValuePair<int, List<cEquity>>> aStagnations = HelperAnalytics.GetStagnationsInHours(this.Signals);
                int totalHours = Signals.Last().CreationTimeStamp.Subtract(Signals.First().CreationTimeStamp).TotalHours.ToInt();
                if (aStagnations.Count > 0)
                {
                    StagnationDays = aStagnations.Max(x => x.Key).ToString("n0") + " hs";
                    //Stagnation in %                
                    StagnationPerc = (aStagnations.Max(x => x.Key) / (double)totalHours).ToString("p2");
                }
                else
                {
                    StagnationDays = "0 hs";
                    //Stagnation in %                
                    StagnationPerc = "N/A";
                }
                #endregion
                #region TRADES
                NumWins = winCount.ToString("n0") + " (" + (winCount / totalCount).ToString("p2") + ")";
                NumLosses = lossCount.ToString("n0") + " (" + (lossCount / totalCount).ToString("p2") + ")";
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************
                GrossProfit = grossProfit.ToString("c2");
                GrossLoss = grossLoss.ToString("c2");
                if (signals.Where(x => x.GetPipsPnL >= 0).Count() > 0)
                    AverageWin = (signals.Where(x => x.GetPipsPnL >= 0).Average(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Where(x => x.GetPipsPnL >= 0).Average(x => x.GetPipsPnL.ToDouble()).ToString("n2") + " pips)";
                else
                    AverageWin = "";
                if (signals.Where(x => x.GetPipsPnL < 0).Count() > 0)
                    AverageLoss = (signals.Where(x => x.GetPipsPnL < 0).Average(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Where(x => x.GetPipsPnL < 0).Average(x => x.GetPipsPnL.ToDouble()).ToString("n2") + " pips)";
                else
                    AverageLoss = "";
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************
                LargestWin = (signals.Max(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Max(x => x.GetPipsPnL).ToString("n2") + " pips)";
                LargestLoss = (signals.Min(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Min(x => x.GetPipsPnL).ToString("n2") + " pips)";

                var consWins = HelperAnalytics.GetConsecutiveWins(signals);
                var consLoss = HelperAnalytics.GetConsecutiveLosses(signals);
                if (consWins.Count() > 0)
                    MaxConsecWins = consWins.Max().ToString("n0");
                if (consLoss.Count() > 0)
                    MaxConsecLosses = consLoss.Max().ToString("n0");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************

                if (consWins.Count > 0)
                    AvgConsecWins = consWins.Average().ToString("n2");
                if (consLoss.Count() > 0)
                    AvgConsecLoss = consLoss.Average().ToString("n2");
                if (signals.Where(x => x.GetPipsPnL >= 0).Count() > 0)
                    AvgNumBarsInWins = signals.Where(x => x.GetPipsPnL >= 0).Average(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalMinutes).ToString("n2");
                if (signals.Where(x => x.GetPipsPnL < 0).Count() > 0)
                    AvgNumBarsInLosses = signals.Where(x => x.GetPipsPnL < 0).Average(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalMinutes).ToString("n2");
                #endregion

                RaisePropertyChanged(String.Empty);
            }
        }
    }
}
