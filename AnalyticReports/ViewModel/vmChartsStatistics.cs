using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VisualHFT.AnalyticReport;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class vmChartsStatistics : BindableBase
    {
        public List<cheldBars> heldBars { get; private set; }
        public List<cTop20> Top20 { get; private set; }
        public List<cListPLRanges> ListPLRanges { get; private set; }
        public List<cPLRangeDuration> PLRangeDuration { get; private set; }
        public List<PointF> regressionLineData { get; private set; }
        public List<PointF> regressionLineDataMAE { get; private set; }
        public List<ScatterChartData> MFE { get; private set; }
        public List<PointF> regressionLineDataMFE { get; private set; }
        public List<ScatterChartData> MAE { get; private set; }

        public void LoadData(List<PositionEx> signals)
        {
            heldBars = (from x in signals.Where(s => s.CloseTimeStamp != null)
                            group x by x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds.ToInt() into m
                            select new cheldBars(
                                m.Key,
                                m.Sum(x => x.PipsPnLInCurrency),
                                m.Where(y => y.PipsPnLInCurrency >= 0).Sum(y => y.PipsPnLInCurrency.Value),    //winers
                                m.Where(y => y.PipsPnLInCurrency < 0).Sum(y => Math.Abs(y.PipsPnLInCurrency.Value)),      //lossers
                                m.Count(y => y.PipsPnLInCurrency >= 0),    //winers
                                m.Count(y => y.PipsPnLInCurrency < 0)      //lossers
                                                )).OrderBy(x => x.SecondsHeld).Take(20).ToList();

            Top20 = (from x in signals
                         group x by x.Symbol into m
                         select new cTop20(
                            m.Key,
                            m.Where(y => y.PipsPnLInCurrency >= 0).Sum(y => y.PipsPnLInCurrency.Value),
                            m.Where(y => y.PipsPnLInCurrency < 0).Sum(y => Math.Abs(y.PipsPnLInCurrency.Value)),
                            m.Sum(y => y.PipsPnLInCurrency.Value),
                            m.Count(y => y.PipsPnLInCurrency >= 0),
                            m.Count(y => y.PipsPnLInCurrency < 0),
                            m.Count()
                                        )).ToList().OrderByDescending(x => x.TotalCount).Take(20).ToList();

            List<double[]> aRanges = new List<double[]>();
            aRanges.Add(new double[] { double.MinValue, 0 });
            aRanges.Add(new double[] { 0, 50 });
            aRanges.Add(new double[] { 50, 100 });
            aRanges.Add(new double[] { 100, 500 });
            aRanges.Add(new double[] { 500, 800 });
            aRanges.Add(new double[] { 800, 1000 });
            aRanges.Add(new double[] { 1000, 1500 });
            aRanges.Add(new double[] { 1500, double.MaxValue });

            //List<> aListPLRanges = new List<PnLRange>();
            ListPLRanges = Enumerable.Empty<object>()
             .Select(r => new cListPLRanges("", 0)) // prototype of anonymous type
             .ToList();

            foreach (double[] r in aRanges)
            {
                string _label = "";
                if (r[0] == double.MinValue)
                    _label = "<" + r[1].ToString();
                else if (r[1] == double.MaxValue)
                    _label = ">" + r[0].ToString();
                else
                    _label = r[0].ToString() + "-" + r[1].ToString();

                var tempVal = (from x in signals
                               where (double)x.PipsPnLInCurrency >= r[0] && (double)x.PipsPnLInCurrency < r[1]
                               group x by _label into m
                               select new cListPLRanges(
                                m.Key,
                                m.Count()
                               )).ToList();
                ListPLRanges.AddRange(tempVal);
            }

            PLRangeDuration = signals.OrderBy(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds).Select(x => new cPLRangeDuration(x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds, (double)x.PipsPnLInCurrency)).ToList();

            regressionLineData = HelperMath.LinearRegression((from x in signals select new System.Drawing.PointF((float)x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds, (float)x.PipsPnLInCurrency)).ToList());

            MAE = (from x in HelperAnalytics.GetMaximumAdversExcursion(signals) select new ScatterChartData(x.Key.ToDouble(), (double)x.Value.GetPipsPnL, "")).ToList();

            regressionLineDataMAE = HelperMath.LinearRegression((from x in MAE select new System.Drawing.PointF((float)x.XValue, (float)x.YValue)).ToList());

            MFE = (from x in HelperAnalytics.GetMaximumFavorableExcursion(signals) select new ScatterChartData(x.Key.ToDouble(), (double)x.Value.GetPipsPnL, "")).ToList();

            regressionLineDataMFE = HelperMath.LinearRegression((from x in MFE select new System.Drawing.PointF((float)x.XValue, (float)x.YValue)).ToList());

            RaisePropertyChanged(String.Empty);
        }
    }
}
