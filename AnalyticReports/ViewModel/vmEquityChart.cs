using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class vmEquityChart : BindableBase
    {
        public List<PositionEx> Signals { get; set; }
        private List<cEquity> aCandlesGrouped;
        private List<cBalance> aBalance;
        private List<cDrawDown> _drawdowns;
        public vmEquityChart()
        {
            
        }
        public void LoadData(List<PositionEx> signals)
        {
            this.Signals = signals;

                if (this.Signals == null || this.Signals.Count == 0)
                    throw new Exception("No signals found.");

            aCandlesGrouped = HelperAnalytics.GetEquityCurveByHour(signals);
            aBalance = (from x in HelperAnalytics.GetBalanceCurve(signals)
                                       group x by new { date = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day).AddHours(x.Date.Hour) } into g
                                       select new cBalance()
                                       {
                                           Date = g.Key.date,
                                           Balance = g.Last().Balance,
                                       }).ToList();


            /*DataTable dt = GenerateHourlyPerformanceGrid(signals);
            if (dt != null)
            {                
                dataGrid1.ItemsSource = dt.DefaultView;
                //GRID HEIGHT

                double rowHeightUnit = 18;
                double originalChartHeight = 690;

                StackPanel oParentStack = (dataGrid1.Parent as StackPanel);
                Grid oParentGrid = oParentStack.Parent as Grid;
                UserControl oUC = oParentGrid.Parent as UserControl;
                double dBottomHeight = Math.Max(150, rowHeightUnit * (dt.Rows.Count + 4));// plus title + grid header;
                
                oUC.Height = originalChartHeight + dBottomHeight; 
                this.Height = oUC.Height;
            }*/
            /*if (aCandlesGrouped.Count > 200)
                (chartEquity.HorizontalAxis as DateTimeCategoricalAxis).LabelInterval = 2;
            if (aCandlesGrouped.Count > 500)
                (chartEquity.HorizontalAxis as DateTimeCategoricalAxis).LabelInterval = 3;
            if (aCandlesGrouped.Count > 1000)
                (chartEquity.HorizontalAxis as DateTimeCategoricalAxis).LabelInterval = 5;
            if (aCandlesGrouped.Count > 2000)
                (chartEquity.HorizontalAxis as DateTimeCategoricalAxis).LabelInterval = 10;*/


            //#region adding custom Drawdowns bar chart
            _drawdowns = HelperAnalytics.GetDrawdowns(this.Signals);

            //List<Telerik.Charting.CategoricalDataPoint> xList = (from x in _drawdowns select new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = (double)x.DrawDownAmmount }).ToList();
            //RadObservableCollection<Telerik.Charting.CategoricalDataPoint> aData = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(xList);
            //BarSeries series = new BarSeries();
            //series.CategoryBinding = CreateBinding("Category");
            //series.ValueBinding = CreateBinding("Value");
            //series.ItemsSource = aData;
            //series.LegendSettings = new SeriesLegendSettings() { Title = "DrawDown" };
            //series.TrackBallInfoTemplate = getTrackBallInfoTemplate("DrawDown", null);
            //series.VerticalAxis = new LinearAxis() { FontSize = 9, ShowLabels = false, IsInverse = true, Visibility = System.Windows.Visibility.Hidden, Maximum = (double)_drawdowns.DefaultIfEmpty(new cDrawDown()).Max(x => x.DrawDownAmmount) * 3 };
            //series.HorizontalAxis = new CategoricalAxis() { ShowLabels = false, Visibility = System.Windows.Visibility.Hidden };
            //series.ShowLabels = false;
            //chartEquity.Series.Add(series);
            //#endregion

            //CreateLineSeries(chartEquity, aCandlesGrouped, "Equity", "Equity ($)", null, 8, false); //EQUITY
            ////CreateLineSeries(chartEquity, aBalance, "Balance", "Balance ($)", null, 8, true); //BALANCE
            //CreateBarSeries<decimal>(chartEquity, aCandlesGrouped, "VolumeQty", "VolumeQty", null, true); //VOLUME

            //#region AddCustomAnnotation
            //List<KeyValuePair<int, List<cEquity>>> aStagnations = HelperAnalytics.GetStagnationsInHours(this.Signals);
            //if (aStagnations.Count > 0)
            //{
            //    List<cEquity> stagnationMax = aStagnations.OrderByDescending(x => x.Key).FirstOrDefault().Value;
            //    CartesianPlotBandAnnotation bandAnnotation = new CartesianPlotBandAnnotation();
            //    bandAnnotation.From = stagnationMax.Min(x => x.Date);
            //    bandAnnotation.To = stagnationMax.Max(x => x.Date);
            //    bandAnnotation.Stroke = Brushes.Transparent;
            //    bandAnnotation.Opacity = 0.6;
            //    //bandAnnotation.Fill = (Brush)(new BrushConverter().ConvertFrom("#15000000"));            
            //    bandAnnotation.Axis = chartEquity.HorizontalAxis;
            //    bandAnnotation.Label = "Max Stagnation period (" + stagnationMax.Max(x => x.Date).Subtract(stagnationMax.Min(x => x.Date)).Days.ToString("n0") + " d)";
            //    bandAnnotation.FontSize = 10;
            //    chartEquity.Annotations.Add(bandAnnotation);
            //}
            //#endregion

            //chartEquity.Palette = chartPalette;
            RaisePropertyChanged(nameof(CandlesGrouped));
            RaisePropertyChanged(nameof(Balance));
            RaisePropertyChanged(nameof(DrawDowns));
        }
        public List<cEquity> CandlesGrouped
        {
            get { return aCandlesGrouped; }
        }
        public List<cBalance> Balance
        {
            get { return aBalance; }
        }
        public List<cDrawDown> DrawDowns
        {
            get { return _drawdowns; }
        }
    }
}
