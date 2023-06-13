using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReport
{
    /// <summary>
    /// Interaction logic for ucStats.xaml
    /// </summary>
    public partial class ucStats : UserControl
    {
        public List<PositionEx> Signals { get; set; }
        public ucStats()
        {
            InitializeComponent();
        }
        public void LoadData(List<PositionEx> signals)
        {
            this.Signals = signals;
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                if (this.Signals == null || this.Signals.Count == 0)
                    throw new Exception("No signals found.");
            }
            else
                return;

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
                lblWinLosses.Content = (winCount / lossCount).ToString("n2");
                //Payout Ratio (Avg win/loss)
                if (avgLossPerc != 0)
                    lblPayoutRatio.Content = (avgWinPerc / Math.Abs(avgLossPerc)).ToString("n2");
                //Average bars in trade (in seconds)
                lblAvgBarsTrade.Content = signals.Where(x => x.CloseTimeStamp > x.CreationTimeStamp).Average(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds).ToString("n2");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************

                //AHPR
                lblAHPR.ToolTip = @"Arithmetic Average Holding Period Return (AHPR) means how much (in %) the system makes on average on every trade. 
Computation of AHPR is dependent on correct setting of initial deposit for backtest.";
                lblAHPR.Content = HelperAnalytics.GetAHPR(this.Signals).ToString("p2");
                //Z-Score
                lblZScore.ToolTip = @"Z-score is the mathematical tool which can be used for calculating the capability of a trading system for generating wins and losses in streaks. 
Every startegy can generate streaks of wins and losses. 
Z-Score helps you to decide if a trading strategy is generating the streaks in a way that is non-random.

A negative Z-score means that winning trades tend to follow winning trades and that losing trades tend to follower losers. 
A positive Z-score means that winners tend to follow losers and vice versa. 
http://en.wikipedia.org/wiki/Z-Score_Financial_Analysis_Tool";
                lblZScore.Content = HelperAnalytics.GetZScore(this.Signals).ToString("n2");
                //Z-Probability
                lblZProbability.ToolTip = @"It is the probability related to the Z-Score, in this meaning the percentage probability that winning trade will be followed by winning trade and losing trade will be followed by losing trade.";
                lblZProbability.Content = HelperAnalytics.GetZProbability(this.Signals).ToString("p2");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************


                //Expectancy
                lblExpectancy.ToolTip = @"Expectancy is the amount you stand to gain, or lose, per dollar of risk. It is the amount that you can expect to make (or lose) on average on every trade.
Expectancy = (Probability of Win * Average Win $) – (Probability of Loss * Average Loss $)
http://www.tradermike.net/2004/05/trading_101_expectancy/";
                lblExpectancy.Content = HelperAnalytics.GetExpectancy(this.Signals).ToString("n2");

                //Deviation
                lblDeviation.ToolTip = @"Standard Deviation is a statistical measure of volatility. It says how much variance there is from the mean (Expectancy).";
                lblDeviation.Content = this.Signals.Where(x => x.GetPipsPnL >= 0).Select(x => ((winCount / totalCount) * x.PipsPnLInCurrency.ToDouble())).Concat(
                                        this.Signals.Where(x => x.GetPipsPnL < 0).Select(x => ((lossCount / totalCount) * x.PipsPnLInCurrency.ToDouble()))).StdDev().ToString("n2");
                //Volatility
                var equity = HelperAnalytics.GetEquityCurveByHour(this.Signals);
                lblVolatility.ToolTip = @"Volatility measures risk as the average range of price fluctuations for the equity over a fixed period of time.";
                lblVolatility.Content = equity.Select(x => x.Equity.ToDouble()).StdDev().ToString("n2");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************

                //Stagnation in Days
                List<KeyValuePair<int, List<cEquity>>> aStagnations = HelperAnalytics.GetStagnationsInHours(this.Signals);
                int totalHours = Signals.Last().CreationTimeStamp.Subtract(Signals.First().CreationTimeStamp).TotalHours.ToInt();
                lblStagnationDays.ToolTip = @"Stagnation is a time period during which the equity curve of your strategy didn't make a new high, which means that your strategy is not making profit.";
                if (aStagnations.Count > 0)
                {
                    lblStagnationDays.Content = aStagnations.Max(x => x.Key).ToString("n0") + " hs";
                    //Stagnation in %                
                    lblStagnationPerc.ToolTip = "";
                    lblStagnationPerc.Content = (aStagnations.Max(x => x.Key) / (double)totalHours).ToString("p2");
                }
                else
                {
                    lblStagnationDays.Content = "0 hs";
                    //Stagnation in %                
                    lblStagnationPerc.ToolTip = "";
                    lblStagnationPerc.Content = "N/A";
                }
                #endregion
                #region TRADES
                lblNumWins.Content = winCount.ToString("n0") + " (" + (winCount / totalCount).ToString("p2") + ")";
                lblNumLosses.Content = lossCount.ToString("n0") + " (" + (lossCount / totalCount).ToString("p2") + ")";
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************
                lblGrossProfit.Content = grossProfit.ToString("c2");
                lblGrossLoss.Content = grossLoss.ToString("c2");
                if (signals.Where(x => x.GetPipsPnL >= 0).Count() > 0)
                    lblAverageWin.Content = (signals.Where(x => x.GetPipsPnL >= 0).Average(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Where(x => x.GetPipsPnL >= 0).Average(x => x.GetPipsPnL.ToDouble()).ToString("n2") + " pips)";
                else
                    lblAverageWin.Content = "";
                if (signals.Where(x => x.GetPipsPnL < 0).Count() > 0)
                    lblAverageLoss.Content = (signals.Where(x => x.GetPipsPnL < 0).Average(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Where(x => x.GetPipsPnL < 0).Average(x => x.GetPipsPnL.ToDouble()).ToString("n2") + " pips)";
                else
                    lblAverageLoss.Content = "";
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************
                lblLargestWin.Content = (signals.Max(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Max(x => x.GetPipsPnL).ToString("n2") + " pips)";
                lblLargestLoss.Content = (signals.Min(x => x.PipsPnLInCurrency.ToDouble())).ToString("c2") + " (" + signals.Min(x => x.GetPipsPnL).ToString("n2") + " pips)";

                var consWins = HelperAnalytics.GetConsecutiveWins(signals);
                var consLoss = HelperAnalytics.GetConsecutiveLosses(signals);
                if (consWins.Count() > 0)
                    lblMaxConsecWins.Content = consWins.Max().ToString("n0");
                if (consLoss.Count() > 0)
                    lblMaxConsecLosses.Content = consLoss.Max().ToString("n0");
                //*********************************************************************************************************************************************************************************************************************
                //*********************************************************************************************************************************************************************************************************************

                if (consWins.Count > 0)
                    lblAvgConsecWins.Content = consWins.Average().ToString("n2");
                if (consLoss.Count() > 0)
                    lblAvgConsecLoss.Content = consLoss.Average().ToString("n2");
                if (signals.Where(x => x.GetPipsPnL >= 0).Count() > 0)
                    lblAvgNumBarsInWins.Content = signals.Where(x => x.GetPipsPnL >= 0).Average(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalMinutes).ToString("n2");
                if (signals.Where(x => x.GetPipsPnL < 0).Count() > 0)
                    lblAvgNumBarsInLosses.Content = signals.Where(x => x.GetPipsPnL < 0).Average(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalMinutes).ToString("n2");
                #endregion
            }
        }
    }
}
