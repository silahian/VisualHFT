using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using VisualHFT.Model;
using VisualHFT.Helpers;

namespace VisualHFT.AnalyticReport
{
    /// <summary>
    /// Interaction logic for ucCharts.xaml
    /// </summary>
    public partial class ucChartsStatistics : UserControl
    {
        public ucChartsStatistics()
        {
            InitializeComponent();
        }

        public void LoadData(List<PositionEx> signals, ChartPalette chartPalette)
        {
            ChartPalette myPalette = chartPalette;
            mainStackPanel.Children.Clear();

            #region Trade Duration

            var heldBars = (from x in signals.Where(s => s.CloseTimeStamp != null)
                                     group x by x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds.ToInt() into m
                                     select new 
                                     {
                                         SecondsHeld = m.Key,
                                         TotalPnL = m.Sum(x => x.PipsPnLInCurrency),
                                         TotalWinPnL = m.Where(y => y.PipsPnLInCurrency >= 0).Sum(y => y.PipsPnLInCurrency.Value),    //winers
                                         TotalLossPnl = m.Where(y => y.PipsPnLInCurrency < 0).Sum(y => Math.Abs(y.PipsPnLInCurrency.Value)),      //lossers
                                         WinPnLCount = m.Count(y => y.PipsPnLInCurrency >= 0),    //winers
                                         LossPnlCount = m.Count(y => y.PipsPnLInCurrency < 0)      //lossers
                                     }).OrderBy(x => x.SecondsHeld).Take(20).ToList();
            #region Volume by Holding Period
            RadCartesianChart oChartDuration = new RadCartesianChart();
            oChartDuration.Name = "oChartDuration";
            oChartDuration.Palette = myPalette;
            oChartDuration.HorizontalAxis = new CategoricalAxis() { LabelFormat = "{0:n0}s", LabelInterval = 5, Title = "Seconds" };
            oChartDuration.VerticalAxis = new LinearAxis() { LabelFormat = "n0", Title = "Qty Trades" };
            oChartDuration.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartDuration.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartDuration.Width = Double.NaN;
            oChartDuration.Height = Double.NaN;

            BarSeries series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series1.Name = "Losses";
            series1.ShowLabels = true;
            series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            series1.CategoryBinding = CreateBinding("Category");
            series1.ValueBinding = CreateBinding("Value");
            series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(heldBars.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.SecondsHeld.ToString(), Value = x.LossPnlCount.ToDouble() }).ToList());

            BarSeries series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series2.Name = "Wins";
            series2.ShowLabels = true;
            series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            series2.CategoryBinding = CreateBinding("Category");
            series2.ValueBinding = CreateBinding("Value");
            series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(heldBars.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.SecondsHeld.ToString() , Value = x.WinPnLCount.ToDouble() }).ToList());

            oChartDuration.Series.Add(series1);
            oChartDuration.Series.Add(series2);
            #endregion

            #region PnL by Holding Period
            RadCartesianChart oChartDuration1 = new RadCartesianChart();
            oChartDuration1.Name = "oChartDuration1";
            oChartDuration1.Palette = myPalette;
            oChartDuration1.HorizontalAxis = new CategoricalAxis() { LabelFormat = "{0:n0}s", LabelInterval = 5, Title = "Seconds" };
            oChartDuration1.VerticalAxis = new LinearAxis() { LabelFormat = "n0", Title = "PnL $" };
            oChartDuration1.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartDuration1.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartDuration1.Width = Double.NaN;
            oChartDuration1.Height = Double.NaN;

            series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series1.Name = "Losses";
            series1.ShowLabels = true;
            series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            series1.CategoryBinding = CreateBinding("Category");
            series1.ValueBinding = CreateBinding("Value");
            series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(heldBars.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.SecondsHeld.ToString(), Value = x.TotalLossPnl.ToDouble() }).ToList());

            series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series2.Name = "Wins";
            series2.ShowLabels = true;
            series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            series2.CategoryBinding = CreateBinding("Category");
            series2.ValueBinding = CreateBinding("Value");
            series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(heldBars.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.SecondsHeld.ToString(), Value = x.TotalWinPnL.ToDouble() }).ToList());

            oChartDuration1.Series.Add(series1);
            oChartDuration1.Series.Add(series2);
            #endregion

            AddControlsToGrid(oChartDuration, "Trade Duration (Qty trades by holding period)", oChartDuration1, "Trade Duration (PnL by holding period)", true);
            #endregion

            #region Top 20 Symbols & PL Ranges

            #region Top 20 Symbols
            var Top20 = (from x in signals
                                  group x by x.Symbol into m
                                  select new
                                  {
                                      Symbol = m.Key,
                                      WinsPnL = m.Where(y => y.PipsPnLInCurrency >= 0).Sum(y => y.PipsPnLInCurrency.Value),
                                      LossesPnL = m.Where(y => y.PipsPnLInCurrency < 0).Sum(y => Math.Abs(y.PipsPnLInCurrency.Value)),
                                      TotalPnl = m.Sum(y => y.PipsPnLInCurrency.Value),
                                      WinsCount = m.Count(y => y.PipsPnLInCurrency >= 0),
                                      LossesCount = m.Count(y => y.PipsPnLInCurrency < 0),
                                      TotalCount = m.Count()
                                  }).ToList().OrderByDescending(x => x.TotalCount).Take(20).ToList();

            RadCartesianChart oChartTop10 = new RadCartesianChart();
            oChartTop10.Name = "oChartTop10";
            oChartTop10.Palette = myPalette;
            oChartTop10.HorizontalAxis = new CategoricalAxis() { Title = "Symbol" };
            oChartTop10.VerticalAxis = new LinearAxis() { LabelFormat = "n0", Title = "Profit $ (usd)" };
            oChartTop10.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartTop10.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartTop10.Width = Double.NaN;
            oChartTop10.Height = Double.NaN;

            series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series1.Name = "QtyLosses";
            series1.ShowLabels = true;
            series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:C0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            series1.CategoryBinding = CreateBinding("Category");
            series1.ValueBinding = CreateBinding("Value");
            series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(Top20.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Symbol, Value = x.LossesPnL.ToDouble() }).ToList());

            series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series2.Name = "QtyWins";
            series2.ShowLabels = true;
            series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:C0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            series2.CategoryBinding = CreateBinding("Category");
            series2.ValueBinding = CreateBinding("Value");
            series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(Top20.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Symbol, Value = x.WinsPnL.ToDouble() }).ToList());

            oChartTop10.Series.Add(series1);
            oChartTop10.Series.Add(series2);
            #endregion
            #region PL Ranges
            #region __prepare data
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
            var aListPLRanges = Enumerable.Empty<object>()
             .Select(r => new { Range = "", Qty = 0 }) // prototype of anonymous type
             .ToList();

            foreach (double[] r in aRanges)
            {
                string _label = "";
                if (r[0] == double.MinValue)
                    _label = "<"+ r[1].ToString();
                else if (r[1] == double.MaxValue)
                    _label = ">"+ r[0].ToString();
                else
                    _label = r[0].ToString() + "-"+ r[1].ToString();

                var tempVal = (from x in signals
                               where (double)x.PipsPnLInCurrency >= r[0] && (double)x.PipsPnLInCurrency < r[1]
                               group x by _label into m
                               select new
                               {
                                   Range = m.Key,
                                   Qty = m.Count()
                               }).ToList();
                aListPLRanges.AddRange(tempVal);
            }
            #endregion
            RadCartesianChart oChartPLRanges = new RadCartesianChart();
            oChartPLRanges.Name = "oChartPLRanges";
            oChartPLRanges.Palette = myPalette;
            oChartPLRanges.HorizontalAxis = new CategoricalAxis() { LabelRotationAngle=270, LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate, Title = "PL Range"};
            oChartPLRanges.VerticalAxis = new LinearAxis() { Title = "Qty Trades"};
            oChartPLRanges.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartPLRanges.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartPLRanges.Width = Double.NaN;
            oChartPLRanges.Height = Double.NaN;

            series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Cluster };
            series1.Name = "QtyByRange";
            series1.ShowLabels = true;
            series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series1.LegendSettings = new SeriesLegendSettings() { Title = "Qty By Range" };
            series1.CategoryBinding = CreateBinding("Category");
            series1.ValueBinding = CreateBinding("Value");
            series1.TrackBallInfoTemplate = getTrackBallInfoTemplate("Qty By Range", null);
            series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(aListPLRanges.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Range, Value = x.Qty.ToDouble() }).ToList());

            oChartPLRanges.Series.Add(series1);
            #endregion

            AddControlsToGrid(oChartTop10, "Top 20 Symbols (by PL)", oChartPLRanges, "PL Ranges", false);
            #endregion
                            
            #region Scatter PL Range vs Duration
            RadCartesianChart oChartPLRangeDuration = new RadCartesianChart();
            oChartPLRangeDuration.Name = "oChartPLRangeDuration";
            oChartPLRangeDuration.Palette = myPalette;
            oChartPLRangeDuration.HorizontalAxis = new LogarithmicAxis() { LabelFormat = "0.0 s", Title = "Seconds" };
            oChartPLRangeDuration.VerticalAxis = new LinearAxis() { LabelFormat = "c1", Title = "PL Range ($)" };
            oChartPLRangeDuration.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartPLRangeDuration.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartPLRangeDuration.Width = Double.NaN;
            oChartPLRangeDuration.Height = Double.NaN;
            oChartPLRangeDuration.Grid = new CartesianChartGrid() { IsTabStop = false, MajorLinesVisibility = GridLineVisibility.XY, StripLinesVisibility = GridLineVisibility.Y };
            oChartPLRangeDuration.Behaviors.Add(new ChartPanAndZoomBehavior() { ZoomMode = ChartPanZoomMode.Both, PanMode = ChartPanZoomMode.Both, DragMode = ChartDragMode.Pan, MouseWheelMode = ChartMouseWheelMode.ZoomHorizontally });
            oChartPLRangeDuration.Behaviors.Add(new ChartTrackBallBehavior() { SnapMode = TrackBallSnapMode.None, ShowTrackInfo = true, ShowIntersectionPoints = true });
            ScatterPointSeries seriesData = new ScatterPointSeries()
            {
                LegendSettings = new SeriesLegendSettings {},
                ShowLabels = false,
                PointSize = new Size(5, 5),                
                XValueBinding = CreateBinding("XValue"),
                YValueBinding = CreateBinding("YValue"),
                ItemsSource = new RadObservableCollection<ScatterChartData>(signals.OrderBy(x => x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds).Select(x => new ScatterChartData(x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds, (double)x.PipsPnLInCurrency, "" )).ToList()),
                TrackBallInfoTemplate = getTrackBallInfoTemplateScatter("Seconds", "n1", "PL", "c1", null),
                PointTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate x:Key='maDataPoint' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ><Ellipse Width='5' Height='5' Fill='{Binding DataItem.Brush}'/></DataTemplate>")
            };
            var regressionLineData = HelperMath.LinearRegression((from x in signals select new System.Drawing.PointF((float)x.CloseTimeStamp.Subtract(x.CreationTimeStamp).TotalSeconds, (float)x.PipsPnLInCurrency)).ToList());
            oChartPLRangeDuration.Annotations.Add(new CartesianCustomLineAnnotation() { HorizontalFrom = regressionLineData.First().X, VerticalFrom = regressionLineData.First().Y, HorizontalTo = regressionLineData.Last().X, VerticalTo = regressionLineData.Last().Y, Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 3 });
            oChartPLRangeDuration.Series.Add(seriesData);
                       
            AddControlsToGrid(oChartPLRangeDuration, "PL Range($) vs Duration", null, "", false);
            #endregion

            #region Scatter MAE and MFE
            var aMAE = (from x in HelperAnalytics.GetMaximumAdversExcursion(signals) select new ScatterChartData(x.Key.ToDouble(), (double)x.Value.GetPipsPnL, "")).ToList();
            RadCartesianChart oChartMAE = new RadCartesianChart();
            oChartMAE.Name = "oChartMAE";
            oChartMAE.Palette = myPalette;
            oChartMAE.HorizontalAxis = new LinearAxis() { LabelFormat = "c1", Title = "MAE ($ usd)"};
            oChartMAE.VerticalAxis = new LinearAxis() { LabelFormat = "c1", Title = "PL ($ usd)"};
            oChartMAE.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartMAE.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartMAE.Width = Double.NaN;
            oChartMAE.Height = Double.NaN;
            oChartMAE.Grid = new CartesianChartGrid() {IsTabStop=false, MajorLinesVisibility = GridLineVisibility.XY, StripLinesVisibility = GridLineVisibility.Y};
            oChartMAE.Behaviors.Add(new ChartPanAndZoomBehavior() { ZoomMode = ChartPanZoomMode.Both, PanMode = ChartPanZoomMode.Both, DragMode = ChartDragMode.Pan, MouseWheelMode = ChartMouseWheelMode.ZoomHorizontally });
            oChartMAE.Behaviors.Add(new ChartTrackBallBehavior() { SnapMode = TrackBallSnapMode.None, ShowTrackInfo = true, ShowIntersectionPoints = true });            
            seriesData = new ScatterPointSeries()
            {                
                LegendSettings = new SeriesLegendSettings {},
                ShowLabels = false,
                XValueBinding = CreateBinding("XValue"),
                YValueBinding = CreateBinding("YValue"),
                PointSize = new Size(5, 5),
                ItemsSource = new RadObservableCollection<ScatterChartData>(aMAE),
                TrackBallInfoTemplate = getTrackBallInfoTemplateScatter("MAE", "n2", "PL", "n2", null),
                PointTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate x:Key='maDataPoint' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ><Ellipse Width='5' Height='5' Fill='{Binding DataItem.Brush}'/></DataTemplate>")
            };
            regressionLineData = HelperMath.LinearRegression((from x in aMAE select new System.Drawing.PointF((float)x.XValue, (float)x.YValue)).ToList());
            if (regressionLineData != null)
            {
                oChartMAE.Annotations.Add(new CartesianCustomLineAnnotation() { HorizontalFrom = regressionLineData.First().X, VerticalFrom = regressionLineData.First().Y, HorizontalTo = regressionLineData.Last().X, VerticalTo = regressionLineData.Last().Y, Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 3 });
                oChartMAE.Series.Add(seriesData);
            }

            var aMFE = from x in HelperAnalytics.GetMaximumFavorableExcursion(signals) select new ScatterChartData(x.Key.ToDouble(), (double)x.Value.GetPipsPnL, "");
            RadCartesianChart oChartMFE = new RadCartesianChart();
            oChartMFE.Name = "oChartMFE";
            oChartMFE.Palette = myPalette;
            oChartMFE.HorizontalAxis = new LinearAxis() { LabelFormat = "c1", Title = "MFE ($ usd)"};
            oChartMFE.VerticalAxis = new LinearAxis() { LabelFormat = "c1", Title = "PL ($ usd)" };
            oChartMFE.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartMFE.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartMFE.Width = Double.NaN;
            oChartMFE.Height = Double.NaN;
            oChartMFE.Grid = new CartesianChartGrid() {IsTabStop=false, MajorLinesVisibility = GridLineVisibility.XY, StripLinesVisibility = GridLineVisibility.Y};
            oChartMFE.Behaviors.Add(new ChartPanAndZoomBehavior() { ZoomMode = ChartPanZoomMode.Both, PanMode = ChartPanZoomMode.Both, DragMode = ChartDragMode.Pan, MouseWheelMode = ChartMouseWheelMode.ZoomHorizontally });
            oChartMFE.Behaviors.Add(new ChartTrackBallBehavior() { SnapMode = TrackBallSnapMode.None, ShowTrackInfo = true, ShowIntersectionPoints = true });
            seriesData = new ScatterPointSeries()
            {
                LegendSettings = new SeriesLegendSettings {},
                ShowLabels = false,
                XValueBinding = CreateBinding("XValue"),
                YValueBinding = CreateBinding("YValue"),
                PointSize = new Size(5, 5),
                ItemsSource = new RadObservableCollection<ScatterChartData>(aMFE),
                TrackBallInfoTemplate = getTrackBallInfoTemplateScatter("MFE", "n2", "PL", "n2", null),
                PointTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate x:Key='maDataPoint' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ><Ellipse Width='5' Height='5' Fill='{Binding DataItem.Brush}'/></DataTemplate>")
            };
            regressionLineData = HelperMath.LinearRegression((from x in aMFE select new System.Drawing.PointF((float)x.XValue, (float)x.YValue)).ToList());
            if (regressionLineData != null)
            {
                oChartMFE.Annotations.Add(new CartesianCustomLineAnnotation() { HorizontalFrom = regressionLineData.First().X, VerticalFrom = regressionLineData.First().Y, HorizontalTo = regressionLineData.Last().X, VerticalTo = regressionLineData.Last().Y, Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 3 });
                oChartMFE.Series.Add(seriesData);
            }

            AddControlsToGrid(oChartMAE, "Maximum Adverse Excursion (MAE)", oChartMFE, "Maximum Favorable Excursion (MFE)", false);
            #endregion
        }

        private void AddControlsToGrid(RadCartesianChart chart1, string title1, RadCartesianChart chart2, string title2, bool showLegendItems)
        {
            Grid oGrid = new Grid();
            ColumnDefinition oCol1 = new ColumnDefinition(); oCol1.Width = new GridLength(50, GridUnitType.Star); 
            ColumnDefinition oCol2 = new ColumnDefinition(); oCol2.Width = new GridLength(80, GridUnitType.Pixel);
            ColumnDefinition oCol3 = new ColumnDefinition(); oCol3.Width = new GridLength(50, GridUnitType.Star);
            ColumnDefinition oCol4 = new ColumnDefinition(); oCol4.Width = new GridLength(80, GridUnitType.Pixel);
            oGrid.ColumnDefinitions.Add(oCol1); oGrid.ColumnDefinitions.Add(oCol2); oGrid.ColumnDefinitions.Add(oCol3); oGrid.ColumnDefinitions.Add(oCol4);
            RowDefinition oNewRowTitle = new RowDefinition(); oNewRowTitle.Height = new GridLength(50, GridUnitType.Pixel);
            RowDefinition oNewRow = new RowDefinition(); oNewRow.Height = new GridLength(500, GridUnitType.Pixel);
            oGrid.RowDefinitions.Add(oNewRow);

            //ADD IT INTO A NEW ROW
            if (chart1 != null)
            {
                #region Title
                if (title1 != "")
                {                    
                    System.Windows.Controls.Label t = new System.Windows.Controls.Label();
                    t.Name = "title1"; t.Content = title1; t.Style = Resources["ChartTitleStyle"] as Style;
                    Canvas.SetZIndex(t, 2000);
                    oGrid.Children.Add(t);
                    Grid.SetColumn(t, 0); Grid.SetRow(t, 0);
                    if (chart2 != null)
                        Grid.SetColumnSpan(t, 2);
                    else
                        Grid.SetColumnSpan(t, 4);
                }
                #endregion
                #region Grid1
                chart1.Margin = new Thickness(10, 0, 0, 10);
                chart1.FontSize = 12;
                oGrid.Children.Add(chart1);
                Grid.SetColumn(chart1, 0); Grid.SetRow(chart1, 1);
                if (chart2 == null)
                    Grid.SetColumnSpan(chart1, 3);
                #endregion
                #region Legend
                if (showLegendItems)
                {
                    RadLegend oLegend = new RadLegend();
                    Binding b = new Binding("LegendItems");
                    b.Source = chart1;
                    oLegend.SetBinding(RadLegend.ItemsProperty, b);
                    oLegend.Margin = new Thickness(0);
                    oGrid.Children.Add(oLegend);
                    if (chart2 != null)
                    {
                        Grid.SetColumn(oLegend, 1); Grid.SetRow(oLegend, 1);
                    }
                    else
                    {
                        Grid.SetColumn(oLegend, 3); Grid.SetRow(oLegend, 1);
                    }
                }
                #endregion
            }
            if (chart2 != null)
            {
                #region Title
                if (title2 != "")
                {
                    System.Windows.Controls.Label t = new System.Windows.Controls.Label();
                    t.Name = "title2"; t.Content = title2; t.Style = Resources["ChartTitleStyle"] as Style;
                    Canvas.SetZIndex(t, 2000);
                    oGrid.Children.Add(t);
                    Grid.SetColumn(t, 2); Grid.SetRow(t, 0);
                    Grid.SetColumnSpan(t, 2);
                }
                #endregion
                #region Grid2
                chart2.Margin = new Thickness(10, 0, 0, 10);
                chart2.FontSize = 12;
                oGrid.Children.Add(chart2);
                Grid.SetColumn(chart2, 2); Grid.SetRow(chart2, 1);
                #endregion
                #region Legend
                if (showLegendItems)
                {
                    RadLegend oLegend = new RadLegend();
                    Binding b = new Binding("LegendItems");
                    b.Source = chart2;
                    oLegend.SetBinding(RadLegend.ItemsProperty, b);
                    oLegend.Margin = new Thickness(0);
                    oGrid.Children.Add(oLegend);
                    Grid.SetColumn(oLegend, 3); Grid.SetRow(oLegend, 1);
                }
                #endregion
            }
            mainStackPanel.Children.Add(oGrid);
        }

        #region Chart methods
        private static PropertyNameDataPointBinding CreateBinding(string propertyName)
        {
            PropertyNameDataPointBinding binding = new PropertyNameDataPointBinding();
            binding.PropertyName = propertyName;
            return binding;
        }
        public static Func<TItem, TField> SelectExpression<TItem, TField>(string fieldName)
        {
            System.Linq.Expressions.MemberExpression field = null;
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TItem), "item");
            string[] aFields = fieldName.Split('.');
            foreach (string f in aFields)
            {
                if (field == null)
                    field = System.Linq.Expressions.Expression.Property(param, f);
                else
                    field = System.Linq.Expressions.Expression.Property(field, f);
            }
            return System.Linq.Expressions.Expression.Lambda<Func<TItem, TField>>(field, new System.Linq.Expressions.ParameterExpression[] { param }).Compile();
        }
        private static bool _alreadyShowCategory;
        private static DataTemplate getTrackBallInfoTemplate(string name, Brush color)
        {
            if (color == null)
                color = Brushes.Black;
            StringBuilder templateString = new StringBuilder();
            templateString.Append("<DataTemplate x:Key='maTrackballInfoTemplate' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >");

            templateString.Append("<StackPanel>");
            if (!_alreadyShowCategory)
            {
                templateString.Append("<Border Background='#FFEBEBEB' HorizontalAlignment='Center'>");
                templateString.Append("    <TextBlock Text='{Binding Path=DataPoint.Category, StringFormat=d}' Foreground='Black' FontSize='16' FontWeight='Bold' Margin='5,5,5,5' />");
                templateString.Append("</Border>");
                _alreadyShowCategory = true;
            }

            templateString.Append("<StackPanel Orientation='Horizontal' Background='White' HorizontalAlignment='Right'>");
            templateString.Append("<TextBlock Text=' " + name + ": ' Foreground='" + color.ToString() + "' FontSize='12'/>");
            templateString.Append("<TextBlock Text='{Binding Path=DataPoint.Value, StringFormat=n2}'  Foreground='" + color.ToString() + "' FontSize='12' />");
            templateString.Append("</StackPanel>");
            templateString.Append("</StackPanel>");
            templateString.Append("</DataTemplate>");
            var template = (DataTemplate)System.Windows.Markup.XamlReader.Parse(templateString.ToString());
            return template as DataTemplate;

        }
        private static DataTemplate getTrackBallInfoTemplateScatter(string nameX, string formatX, string nameY, string formatY, Brush color)
        {
            if (color == null)
                color = Brushes.Black;
            StringBuilder templateString = new StringBuilder();
            templateString.Append("<DataTemplate x:Key='maTrackballInfoTemplateScatter' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >");
            templateString.Append("<StackPanel Orientation='Vertical' >");

            templateString.Append("<StackPanel Orientation='Horizontal' Background='White' HorizontalAlignment='Right'>");
            templateString.Append("<TextBlock Text='" + nameX  + ": '  Foreground='" + color.ToString() + "' FontSize='12' />");
            templateString.Append("<TextBlock Text='{Binding Path=DataPoint.XValue, StringFormat=" + formatX +"}'  Foreground='" + color.ToString() + "' FontSize='12' />");
            templateString.Append("</StackPanel>");

            templateString.Append("<StackPanel Orientation='Horizontal' Background='White' HorizontalAlignment='Right'>");
            templateString.Append("<TextBlock Text='" + nameY + ": '  Foreground='" + color.ToString() + "' FontSize='12' />");
            templateString.Append("<TextBlock Text='{Binding Path=DataPoint.YValue, StringFormat=" + formatY + "}'  Foreground='" + color.ToString() + "' FontSize='12' />");
            templateString.Append("</StackPanel>");

            templateString.Append("</StackPanel>");
            templateString.Append("</DataTemplate>");

            
            var template = (DataTemplate)System.Windows.Markup.XamlReader.Parse(templateString.ToString());
            return template as DataTemplate;

        }

        #endregion

    }
    public class ScatterChartData
    {
        public ScatterChartData(double x, double y, string toolTip)
        {
            this.XValue = x;
            this.YValue = y;
            this.ToolTip = toolTip;
        }

        public double XValue { get; set; }
        public double YValue { get; set; }

        public string Brush
        {
            get
            {
                if (this.YValue < 0)
                {
                    return "Red";
                }
                else
                {
                    return "Green";
                }
            }
        }
        public string ToolTip { get; set; }
    }

}
