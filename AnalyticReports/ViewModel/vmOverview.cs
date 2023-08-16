using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class vmOverview : BindableBase
    {
        public List<PositionEx> Signals { get; set; }

        public string VolumeTraded { get; private set; }
        public string NumTrades { get; private set; }
        public string SharpeRatio { get; private set; }
        public string ProfitFactor { get; private set; }
        public string Expectancy { get; private set; }
        public string WinningPerc { get; private set; }
        public string MaxDrawDownPercDaily { get; private set; }
        public string MaxDrawDownPercIntraday { get; private set; }
        public string DailyAvgProfit { get; private set; }
        public string DailyAvgTrades { get; private set; }
        public string HourlyAvgProfit { get; private set; }
        public string HourlyAvgTrades { get; private set; }
        public string tTestValue { get; private set; }

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
                decimal grossProfit = this.Signals.Where(s => s.PipsPnLInCurrency.HasValue && s.PipsPnLInCurrency.Value >= 0).Sum(s => s.PipsPnLInCurrency.Value);
                decimal grossLoss = this.Signals.Where(s => s.PipsPnLInCurrency.HasValue && s.PipsPnLInCurrency.Value < 0).Sum(s => s.PipsPnLInCurrency.Value);

                VolumeTraded = HelperFormat.FormatNumber((double)this.Signals.Sum(x => x.GetCloseQuantity + x.GetOpenQuantity));

                NumTrades = totalCount.ToString("n0");

                SharpeRatio = HelperAnalytics.GetIntradaySharpeRatio(this.Signals).ToString("n2");

                if (grossLoss > 0.0m)
                    ProfitFactor = Math.Abs(grossProfit / grossLoss).ToString("n2");
                else
                    ProfitFactor = "";

                Expectancy = (HelperAnalytics.GetExpectancy(this.Signals)).ToString("n2");

                WinningPerc = (winCount / totalCount).ToString("p2");

                //******************************************************************************************************
                MaxDrawDownPercDaily = HelperAnalytics.GetMaximumDrawdownPerc(this.Signals, false).ToString("p2");
                MaxDrawDownPercIntraday = HelperAnalytics.GetMaximumDrawdownPerc(this.Signals).ToString("p2");


                //HOURLY AVG PROFIT
                var avgDailyPnL = HelperAnalytics.GetAverageProfitByDay(this.Signals);
                DailyAvgProfit = avgDailyPnL.Equity.ToString("p2");
                DailyAvgTrades = "(" + avgDailyPnL.VolumeQty.ToString("n0") + ")";

                var avgHourlyPnL = HelperAnalytics.GetAverageProfitByHour(this.Signals);
                HourlyAvgProfit = avgHourlyPnL.Equity.ToString("p2");
                HourlyAvgTrades = "(" + avgHourlyPnL.VolumeQty.ToString("n0") + ")";

                //t = square root ( number of trades ) * (average profit per trade trade / standard deviation of trades)
                List<double> tradesPnL = this.Signals.Select(x => (double)x.PipsPnLInCurrency).ToList();
                tTestValue = (Math.Sqrt(totalCount) * tradesPnL.Average() / tradesPnL.StdDev()).ToString("n2");

                RaisePropertyChanged(String.Empty);
            }

        }
    }
}
