using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using VisualHFT.Model;
using VisualHFT.Helpers;

namespace VisualHFT.AnalyticReport
{
    /// <summary>
    /// Interaction logic for ucEquityChart.xaml
    /// </summary>
    public partial class ucEquityChart : UserControl
    {
        public List<PositionEx> Signals { get; set; }
        public ucEquityChart()
        {
            InitializeComponent();
        }
        public void LoadData(List<PositionEx> signals, ChartPalette chartPalette)
        {
            this.Signals = signals;
            chartEquity.Series.Clear();

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                if (this.Signals == null || this.Signals.Count == 0)
                    throw new Exception("No signals found.");
            }
            else
                return;

            List<cEquity> aCandlesGrouped = HelperAnalytics.GetEquityCurveByHour(signals);
            List<cBalance> aBalance = (from x in HelperAnalytics.GetBalanceCurve(signals)
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


            #region adding custom Drawdowns bar chart
            List<cDrawDown> _drawdowns = HelperAnalytics.GetDrawdowns(this.Signals);

            List<Telerik.Charting.CategoricalDataPoint> xList = (from x in _drawdowns select new Telerik.Charting.CategoricalDataPoint { Category = x.Date, Value = (double)x.DrawDownAmmount }).ToList();
            RadObservableCollection<Telerik.Charting.CategoricalDataPoint> aData = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(xList);
            BarSeries series = new BarSeries();
            series.CategoryBinding = CreateBinding("Category");
            series.ValueBinding = CreateBinding("Value");
            series.ItemsSource = aData;
            series.LegendSettings = new SeriesLegendSettings() { Title = "DrawDown" };
            series.TrackBallInfoTemplate = getTrackBallInfoTemplate("DrawDown", null);
            series.VerticalAxis = new LinearAxis() { FontSize = 9, ShowLabels = false, IsInverse = true, Visibility= System.Windows.Visibility.Hidden, Maximum = (double)_drawdowns.DefaultIfEmpty(new cDrawDown()).Max(x => x.DrawDownAmmount)*3  };
            series.HorizontalAxis = new CategoricalAxis() { ShowLabels = false, Visibility = System.Windows.Visibility.Hidden };
            series.ShowLabels = false;            
            chartEquity.Series.Add(series);
            #endregion

            CreateLineSeries(chartEquity, aCandlesGrouped, "Equity", "Equity ($)", null, 8, false); //EQUITY
            //CreateLineSeries(chartEquity, aBalance, "Balance", "Balance ($)", null, 8, true); //BALANCE
            CreateBarSeries<decimal>(chartEquity, aCandlesGrouped, "VolumeQty", "VolumeQty", null, true); //VOLUME

            #region AddCustomAnnotation
            List<KeyValuePair<int, List<cEquity>>> aStagnations = HelperAnalytics.GetStagnationsInHours(this.Signals); 
            if (aStagnations.Count > 0)
            {
                List<cEquity> stagnationMax = aStagnations.OrderByDescending(x => x.Key).FirstOrDefault().Value;
                CartesianPlotBandAnnotation bandAnnotation = new CartesianPlotBandAnnotation();
                bandAnnotation.From = stagnationMax.Min(x => x.Date);
                bandAnnotation.To = stagnationMax.Max(x => x.Date);
                bandAnnotation.Stroke = Brushes.Transparent;
                bandAnnotation.Opacity = 0.6;
                //bandAnnotation.Fill = (Brush)(new BrushConverter().ConvertFrom("#15000000"));            
                bandAnnotation.Axis = chartEquity.HorizontalAxis;
                bandAnnotation.Label = "Max Stagnation period (" + stagnationMax.Max(x => x.Date).Subtract(stagnationMax.Min(x => x.Date)).Days.ToString("n0") + " d)";
                bandAnnotation.FontSize = 10;
                chartEquity.Annotations.Add(bandAnnotation);
            }
            #endregion            
            
            chartEquity.Palette = chartPalette;
        }
        private DataTable GenerateHourlyPerformanceGrid(List<PositionEx> aSignals)
        {

            ///     Close = PL amount of trade,
            ///     Tag = Balance amount End (PL cummulative),
            ///     Volume = Volume of transactions
            List<cEquity> aEquity = HelperAnalytics.GetEquityCurveByHour(aSignals);

            #region Create Table
            DataTable dt = new DataTable();
            dt.Columns.Add("0");
            dt.Columns.Add("1");
            dt.Columns.Add("2");
            dt.Columns.Add("3");
            dt.Columns.Add("4");
            dt.Columns.Add("5");
            dt.Columns.Add("6");
            dt.Columns.Add("7");
            dt.Columns.Add("8");
            dt.Columns.Add("9");
            dt.Columns.Add("10");
            dt.Columns.Add("11");
            dt.Columns.Add("12");
            dt.Columns.Add("Day");
            #endregion
            int currentYear = 0;
            double yearlyCummulative = 0;
            DataRow oRow = null;
            foreach (cEquity c in aEquity)
            {
                if (currentYear != c.Date.Year)
                {
                    if (oRow != null)//END of ROW
                    {
                        oRow["YearPerc"] = yearlyCummulative.ToString("p2");
                        dt.Rows.Add(oRow);
                    }
                    yearlyCummulative = 0;
                    currentYear = c.Date.Year;
                    oRow = dt.NewRow();
                    oRow["Year"] = c.Date.Year.ToString();
                }
                if (oRow != null)
                {
                    /*oRow[c.Date.ToString("MMM")] = c.PLPerc.ToString("p2");
                    yearlyCummulative += (double)c.PLPerc;*/
                }
            }
            if (oRow != null)
            {
                oRow["YearPerc"] = yearlyCummulative.ToString("p2");
                dt.Rows.Add(oRow);
            }
            return dt;
        }


        #region Chart methods
        private static void CreateBarSeries(RadCartesianChart chart, List<cEquity> aCandles, string ObjectPropertyName, string name, Brush color)
        {
            CreateBarSeries(chart, aCandles, ObjectPropertyName, name, color);
        }
        private static void CreateBarSeries<T>(RadCartesianChart chart, List<cEquity> aCandles, string ObjectPropertyName, string name, Brush color, bool createOnNewAxis)
        {
            try
            {
                var expr = SelectExpression<cEquity, T>(ObjectPropertyName);
                List<T> propValues = aCandles.Select(expr).ToList();
                List<Telerik.Charting.CategoricalDataPoint> xList = aCandles.Zip(propValues, (valx, valy) =>
                                                                    new Telerik.Charting.CategoricalDataPoint { Category = valx.Date, Value = valy.ToDouble() }).ToList();
                RadObservableCollection<Telerik.Charting.CategoricalDataPoint> aData = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(xList);
                BarSeries series = new BarSeries();
                series.CategoryBinding = CreateBinding("Category");
                series.ValueBinding = CreateBinding("Value");
                series.ItemsSource = aData;
                series.LegendSettings = new SeriesLegendSettings() { Title = name };
                series.TrackBallInfoTemplate = getTrackBallInfoTemplate(name, color);
                LinearAxis axis = null;
                if (createOnNewAxis || chart.VerticalAxis == null)
                    axis = new LinearAxis();
                else 
                {
                    axis = chart.VerticalAxis as LinearAxis;
                }
                axis.FontSize = 9;
                axis.Maximum = aData.Max(x => x.Value.ToDouble()) * 10;
                axis.ShowLabels = false;

                series.VerticalAxis = axis;
                series.VerticalAlignment = VerticalAlignment.Bottom;
                chart.Series.Add(series);
                chart.HorizontalAxis.LabelRotationAngle = 90;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private static void CreateLineSeries(RadCartesianChart chart, List<cEquity> aCandles, string ObjectPropertyName, string name, Brush color)
        {
            CreateLineSeries(chart, aCandles, ObjectPropertyName, name, color, 1, false);
        }
        private static void CreateLineSeries(RadCartesianChart chart, List<cEquity> aCandles, string ObjectPropertyName, string name, Brush color, int Thickness, bool createOnNewAxis)
        {
            try
            {
                var expr = SelectExpression<cEquity, decimal>(ObjectPropertyName);
                List<decimal> propValues = aCandles.Select(expr).ToList();
                List<Telerik.Charting.CategoricalDataPoint> xList = aCandles.Zip(propValues, (valx, valy) =>
                                                                    new Telerik.Charting.CategoricalDataPoint { Category = valx.Date, Value = valy.ToDouble() }).ToList();
                RadObservableCollection<Telerik.Charting.CategoricalDataPoint> aData = new RadObservableCollection<Telerik.Charting.CategoricalDataPoint>(xList);
                LineSeries series = new LineSeries();
                series.CategoryBinding = CreateBinding("Category");
                series.ValueBinding = CreateBinding("Value");
                series.ItemsSource = aData;
                series.LegendSettings = new SeriesLegendSettings() { Title = name };
                if (color != null)
                    series.Stroke = color;
                series.StrokeThickness = Thickness;
                if (name != "")
                    series.TrackBallInfoTemplate = getTrackBallInfoTemplate(name, color);

                LinearAxis axis = null;
                if (createOnNewAxis || chart.VerticalAxis == null)
                    axis = new LinearAxis();
                else
                {
                    axis = chart.VerticalAxis as LinearAxis;
                }
                axis.FontSize = 9;
                axis.LabelFormat = "N0";
                axis.Title = name;
                series.VerticalAxis = axis;
                chart.Series.Add(series);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        
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
