using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace VisualHFT.AnalyticReport
{
    public delegate void ValuesChangedHandler(Object sender, EventArgs e, double initialAmmount, double tradeAmmount);
    /// <summary>
    /// Interaction logic for ucOverview.xaml
    /// </summary>
    public partial class ucOverview : UserControl
    {

        public event ValuesChangedHandler ValuesChanged = new ValuesChangedHandler(OnValuesChanged);
        public static void OnValuesChanged(Object sender, EventArgs e, double initialAmmount, double tradeAmmount) { }


        public List<PositionEx> Signals { get; set; }
        public ucOverview()
        {
            InitializeComponent();
        }
        public void LoadData(List<PositionEx> signals)
        {
            //if (ValuesChanged != null)
            //    ValuesChanged(this, new EventArgs(), 0, 0);

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
                decimal totalReturn = this.Signals.Sum(s => s.PipsPnLInCurrency.Value);
                double winCount = this.Signals.Where(s => s.PipsPnLInCurrency.Value >= 0).Count();
                double lossCount = this.Signals.Where(s => s.PipsPnLInCurrency.Value < 0).Count();
                decimal grossProfit = this.Signals.Where(s => s.PipsPnLInCurrency.HasValue && s.PipsPnLInCurrency.Value >=0).Sum(s => s.PipsPnLInCurrency.Value);
                decimal grossLoss = this.Signals.Where(s => s.PipsPnLInCurrency.HasValue && s.PipsPnLInCurrency.Value < 0).Sum(s => s.PipsPnLInCurrency.Value);

				lblVolumeTraded.Content = HelperFormat.FormatNumber((double)this.Signals.Sum(x => x.GetCloseQuantity + x.GetOpenQuantity));

                lblNumTrades.ToolTip = @"Total amount of trades made within testing.";
                lblNumTrades.Content = totalCount.ToString("n0");
                
                lblSharpeRatio.ToolTip = @"The Sharpe ratio characterizes how well the return of an asset compensates the investor for the risk taken. When comparing two assets, the one with a higher Sharpe ratio provides better return for the same risk (or, equivalently, the same return for lower risk).
The higher Sharpe ratio, the better.";
                lblSharpeRatio.Content = HelperAnalytics.GetIntradaySharpeRatio(this.Signals).ToString("n2");

                lblProfitFactor.ToolTip = @"Profit factor, shows the ratio between gross profit and gross loss:
ProfitFactor = GrossProfit / GrossLoss";
				if (grossLoss > 0.0m)
					lblProfitFactor.Content = Math.Abs(grossProfit / grossLoss).ToString("n2");
				else
					lblProfitFactor.Content = "";
				lblExpectancy.ToolTip = @"Expectancy tells you on average how much you expect to make per dollar at risk.";
                lblExpectancy.Content = (HelperAnalytics.GetExpectancy(this.Signals)).ToString("n2");

                lblWinningPerc.ToolTip = @"Percentage of winning trades.";
                lblWinningPerc.Content = (winCount / totalCount).ToString("p2");

                //******************************************************************************************************
                lblMaxDrawDownPercDaily.ToolTip = @"Max Drawdown % is the highest difference between one of the local maximums and the subsequent minimum of the percentage of the equity chart.
Drawdown tells you how big of equity has lost historically in the worst losing streak. ";
                lblMaxDrawDownPercDaily.Content = HelperAnalytics.GetMaximumDrawdownPerc(this.Signals, false).ToString("p2");

                lblMaxDrawDownPercIntraday.ToolTip = @"";
                lblMaxDrawDownPercIntraday.Content = HelperAnalytics.GetMaximumDrawdownPerc(this.Signals).ToString("p2");


				//HOURLY AVG PROFIT


				var avgDailyPnL = HelperAnalytics.GetAverageProfitByDay(this.Signals);
				lblDailyAvgProfit.Content = avgDailyPnL.Equity.ToString("p2");
                lblDailyAvgTrades.Content = "(" + avgDailyPnL.VolumeQty.ToString("n0") + ")";

				var avgHourlyPnL = HelperAnalytics.GetAverageProfitByHour(this.Signals);
				lblHourlyAvgProfit.Content = avgHourlyPnL.Equity.ToString("p2");
                lblHourlyAvgTrades.Content = "(" + avgHourlyPnL.VolumeQty.ToString("n0") + ")";


                lbltTestValue.ToolTip = @"The t-Test is a statistical test used to gage how likely your trading system’s results occurred by chance alone. You would like to see a value greater than 1.6 which indicates the trading results are more likely to not be based on chance. Any other value below indicates the trading results might be based upon chance. The t-Test value should be calculated with no less than 30 trades. Below is the t-Test calculation.";
                //t = square root ( number of trades ) * (average profit per trade trade / standard deviation of trades)
                List<double> tradesPnL = this.Signals.Select(x => (double)x.PipsPnLInCurrency).ToList();
                lbltTestValue.Content = (Math.Sqrt(totalCount) * tradesPnL.Average() / tradesPnL.StdDev()).ToString("n2");
            }
        }
    }
}
