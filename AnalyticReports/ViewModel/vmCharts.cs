using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class vmCharts : BindableBase
    {
        public List<cHourSerie> HourSerieWins { get; private set; }
        public List<cHourSerie> HourSerieLosses { get; private set; }

        public void LoadData(List<PositionEx> signals)
        {

            //#region Hours of the day
            //var HourSerieTotal = HelperAnalytics.GetEquityCurveByHour(signals, initialDeposit);
            HourSerieWins = (from x in signals.Where(X => X.PipsPnLInCurrency.Value >= 0)
                                 group x by new { date = new DateTime(x.CreationTimeStamp.Year, x.CreationTimeStamp.Month, x.CreationTimeStamp.Day, x.CreationTimeStamp.Hour, 0, 0) } into g
                                 select new cHourSerie { Date = g.Key.date.ToDateTime(), PLAmount = g.Sum(s => s.PipsPnLInCurrency.ToDouble()), VolumeQty = g.Count() }).ToList();
            HourSerieLosses = (from x in signals.Where(X => X.PipsPnLInCurrency.Value < 0)
                                   group x by new { date = new DateTime(x.CreationTimeStamp.Year, x.CreationTimeStamp.Month, x.CreationTimeStamp.Day, x.CreationTimeStamp.Hour, 0, 0) } into g
                                   select new cHourSerie { Date = g.Key.date.ToDateTime(), PLAmount = g.Sum(s => Math.Abs(s.PipsPnLInCurrency.ToDouble())), VolumeQty = g.Count() }).ToList();



            //#region Hour QTY
            //RadCartesianChart oChartWeekDay = new RadCartesianChart();
            //oChartWeekDay.Name = "chartWeekDay";
            //oChartWeekDay.Palette = myPalette;
            //oChartWeekDay.HorizontalAxis = new DateTimeContinuousAxis() { LabelFormat = "MMM/dd HH 'hs'", LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate, LabelRotationAngle = 60, MajorStepUnit = Telerik.Charting.TimeInterval.Hour };
            //oChartWeekDay.VerticalAxis = new LinearAxis() { LabelFormat = "N0" };
            //oChartWeekDay.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //oChartWeekDay.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            //oChartWeekDay.Width = Double.NaN;
            //oChartWeekDay.Height = Double.NaN;

            //BarSeries series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack100 };
            //series1.Name = "Losses";
            //series1.ShowLabels = false;
            //series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            //series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            //series1.CategoryBinding = CreateBinding("Category");
            //series1.ValueBinding = CreateBinding("Value");
            //series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            //series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieLosses.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = x.VolumeQty.ToDouble() }).ToList());

            //BarSeries series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            //series2.Name = "Wins";
            //series2.ShowLabels = false;
            //series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            //series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            //series2.CategoryBinding = CreateBinding("Category");
            //series2.ValueBinding = CreateBinding("Value");
            //series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            //series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieWins.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = x.VolumeQty.ToDouble() }).ToList());

            //oChartWeekDay.Series.Add(series1);
            //oChartWeekDay.Series.Add(series2);
            //#endregion
            //#region Hours QTY PL
            //RadCartesianChart oChartWeekDayPL = new RadCartesianChart();
            //oChartWeekDayPL.Name = "chartWeekDayPL";
            //oChartWeekDayPL.Palette = myPalette;
            //oChartWeekDayPL.HorizontalAxis = new DateTimeContinuousAxis() { LabelFormat = "MMM/dd HH 'hs'", LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate, LabelRotationAngle = 60, MajorStepUnit = Telerik.Charting.TimeInterval.Hour };
            //oChartWeekDayPL.VerticalAxis = new LinearAxis() { LabelFormat = "C0" };
            //oChartWeekDayPL.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //oChartWeekDayPL.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            //oChartWeekDayPL.Width = Double.NaN;
            //oChartWeekDayPL.Height = Double.NaN;

            //series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack100 };
            //series1.Name = "Losses";
            //series1.ShowLabels = false;
            //series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:C0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            //series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            //series1.CategoryBinding = CreateBinding("Category");
            //series1.ValueBinding = CreateBinding("Value");
            //series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            //series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieLosses.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = Math.Abs(x.PLAmount.ToDouble()) }).ToList());

            //series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            //series2.Name = "Wins";
            //series2.ShowLabels = false;
            //series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:C0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            //series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            //series2.CategoryBinding = CreateBinding("Category");
            //series2.ValueBinding = CreateBinding("Value");
            //series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            //series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieWins.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = x.PLAmount.ToDouble() }).ToList());

            //oChartWeekDayPL.Series.Add(series1);
            //oChartWeekDayPL.Series.Add(series2);
            //#endregion

            //AddControlsToGrid(oChartWeekDay, "By Hour Win/Loss | Qty of Trades", oChartWeekDayPL, "By Hour Win/Loss PL");
            //#endregion

            RaisePropertyChanged(nameof(HourSerieWins));
            RaisePropertyChanged(nameof(HourSerieLosses));
        }
    }
}
