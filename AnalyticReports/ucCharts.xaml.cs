using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
    public partial class ucCharts : UserControl
    {
        public ucCharts()
        {
            InitializeComponent();            
        }

        public void LoadData(List<PositionEx> signals, ChartPalette chartPalette)
        {
            ChartPalette myPalette = chartPalette;
            mainStackPanel.Children.Clear();

            #region Hours of the day
            //var HourSerieTotal = HelperAnalytics.GetEquityCurveByHour(signals, initialDeposit);
            var HourSerieWins = (from x in signals.Where(X => X.PipsPnLInCurrency.Value >= 0)
                                group x by new { date = new DateTime(x.CreationTimeStamp.Year, x.CreationTimeStamp.Month, x.CreationTimeStamp.Day, x.CreationTimeStamp.Hour, 0, 0) } into g
                                 select new { Date = g.Key.date.ToDateTime(), PLAmount = g.Sum(s => s.PipsPnLInCurrency.ToDouble()), VolumeQty = g.Count() }).ToList();
            var HourSerieLosses = (from x in signals.Where(X => X.PipsPnLInCurrency.Value < 0)
                                   group x by new { date = new DateTime(x.CreationTimeStamp.Year, x.CreationTimeStamp.Month, x.CreationTimeStamp.Day, x.CreationTimeStamp.Hour, 0, 0) } into g
                                   select new { Date = g.Key.date.ToDateTime(), PLAmount = g.Sum(s => Math.Abs(s.PipsPnLInCurrency.ToDouble())), VolumeQty = g.Count() }).ToList();



            #region Hour QTY
            RadCartesianChart oChartWeekDay = new RadCartesianChart();
            oChartWeekDay.Name = "chartWeekDay";
            oChartWeekDay.Palette = myPalette;
            oChartWeekDay.HorizontalAxis = new DateTimeContinuousAxis() { LabelFormat = "MMM/dd HH 'hs'", LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate, LabelRotationAngle = 60, MajorStepUnit = Telerik.Charting.TimeInterval.Hour };
			oChartWeekDay.VerticalAxis = new LinearAxis() { LabelFormat = "N0"};
            oChartWeekDay.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartWeekDay.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartWeekDay.Width = Double.NaN;
            oChartWeekDay.Height = Double.NaN;            
            
            BarSeries series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack100 };
            series1.Name = "Losses";                
            series1.ShowLabels = false;
            series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            series1.CategoryBinding = CreateBinding("Category");
            series1.ValueBinding = CreateBinding("Value");
            series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieLosses.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = x.VolumeQty.ToDouble() }).ToList());

            BarSeries series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series2.Name = "Wins";
            series2.ShowLabels = false;
            series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:N0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            series2.CategoryBinding = CreateBinding("Category");
            series2.ValueBinding = CreateBinding("Value");
            series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieWins.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = x.VolumeQty.ToDouble() }).ToList());

            oChartWeekDay.Series.Add(series1);
            oChartWeekDay.Series.Add(series2);
            #endregion
            #region Hours QTY PL
            RadCartesianChart oChartWeekDayPL = new RadCartesianChart();
            oChartWeekDayPL.Name = "chartWeekDayPL";
            oChartWeekDayPL.Palette = myPalette;
			oChartWeekDayPL.HorizontalAxis = new DateTimeContinuousAxis() { LabelFormat = "MMM/dd HH 'hs'", LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate, LabelRotationAngle = 60, MajorStepUnit = Telerik.Charting.TimeInterval.Hour };
			oChartWeekDayPL.VerticalAxis = new LinearAxis() { LabelFormat = "C0"};
            oChartWeekDayPL.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            oChartWeekDayPL.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            oChartWeekDayPL.Width = Double.NaN;
            oChartWeekDayPL.Height = Double.NaN;

            series1 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack100 };
            series1.Name = "Losses";
            series1.ShowLabels = false;
            series1.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:C0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series1.LegendSettings = new SeriesLegendSettings() { Title = series1.Name };
            series1.CategoryBinding = CreateBinding("Category");
            series1.ValueBinding = CreateBinding("Value");
            series1.TrackBallInfoTemplate = getTrackBallInfoTemplate(series1.Name, null);
            series1.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieLosses.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = Math.Abs(x.PLAmount.ToDouble()) }).ToList());

            series2 = new BarSeries() { CombineMode = Telerik.Charting.ChartSeriesCombineMode.Stack };
            series2.Name = "Wins";
            series2.ShowLabels = false;
            series2.LabelDefinitions.Add(new ChartSeriesLabelDefinition() { Format = "{0:C0}", DefaultVisualStyle = Resources["ChartDataLabelStyle"] as Style });
            series2.LegendSettings = new SeriesLegendSettings() { Title = series2.Name };
            series2.CategoryBinding = CreateBinding("Category");
            series2.ValueBinding = CreateBinding("Value");
            series2.TrackBallInfoTemplate = getTrackBallInfoTemplate(series2.Name, null);
            series2.ItemsSource = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(HourSerieWins.Select(x => new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = x.PLAmount.ToDouble() }).ToList());
            
            oChartWeekDayPL.Series.Add(series1);
            oChartWeekDayPL.Series.Add(series2);
            #endregion

            AddControlsToGrid(oChartWeekDay, "By Hour Win/Loss | Qty of Trades", oChartWeekDayPL, "By Hour Win/Loss PL");
            #endregion
        }

        private void AddControlsToGrid(RadCartesianChart chart1, string title1, RadCartesianChart chart2, string title2)
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
                    oGrid.Children.Add(t);
                    Grid.SetColumn(t, 0); Grid.SetRow(t, 0);
                    Grid.SetColumnSpan(t, 2);
                }
                #endregion
                #region Grid1
                chart1.Margin = new Thickness(10, 0, 0, 10);
                chart1.FontSize = 12;
                chart1.Behaviors.Add(new ChartTrackBallBehavior() {ShowTrackInfo=true });
                oGrid.Children.Add(chart1);
                Grid.SetColumn(chart1, 0); Grid.SetRow(chart1, 1);
                #endregion
                #region Legend
                RadLegend oLegend = new RadLegend();
                Binding b = new Binding("LegendItems");                
                b.Source = chart1;
                oLegend.SetBinding(RadLegend.ItemsProperty, b);
                oLegend.Margin = new Thickness(0);
                oGrid.Children.Add(oLegend);
                Grid.SetColumn(oLegend, 1); Grid.SetRow(oLegend, 0);
                #endregion
            }
            if (chart2 != null)
            {
                #region Title
                if (title2 != "")
                {
                    System.Windows.Controls.Label t = new System.Windows.Controls.Label();
                    t.Name = "title2"; t.Content = title2; t.Style = Resources["ChartTitleStyle"] as Style;
                    oGrid.Children.Add(t);
                    Grid.SetColumn(t, 2); Grid.SetRow(t, 0);
                    Grid.SetColumnSpan(t, 2);
                }
                #endregion
                #region Grid2
                chart2.Margin = new Thickness(10, 0, 0, 10);
                chart2.FontSize = 12;
                chart2.Behaviors.Add(new ChartTrackBallBehavior() { ShowTrackInfo = true });
                oGrid.Children.Add(chart2);
                Grid.SetColumn(chart2, 2); Grid.SetRow(chart2, 1);
                #endregion
                #region Legend
                RadLegend oLegend = new RadLegend();
                Binding b = new Binding("LegendItems");
                b.Source = chart2;
                oLegend.SetBinding(RadLegend.ItemsProperty, b);
                oLegend.Margin = new Thickness(0);
                oGrid.Children.Add(oLegend);
                Grid.SetColumn(oLegend, 3); Grid.SetRow(oLegend, 1);
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
        #endregion

    }
}
