using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VisualHFT.View
{
	public partial class GenericHistoricalLineChart : Window
	{
        OxyPlot.Wpf.LineSeries _lineSeries = new OxyPlot.Wpf.LineSeries();
        public GenericHistoricalLineChart(object dataContext, OxyPlot.Wpf.LineSeries lineSeries)
		{
            this.DataContext = dataContext;
            _lineSeries = lineSeries;

            InitializeComponent();
            InitData();
        }
        public void InitData()
        {
            //instead of adding the lineseries dynamically, we use the one already in xaml
            // The reason is we couldn't make it work => so we update the values with the existing instead ( so we can use it for other general purposes later on)


            //chtChart.Series.Clear();            
            //chtChart.Series.Add(_lineSeries);
            var existingSerie = chtChart.Series[0] as OxyPlot.Wpf.LineSeries;
            if (existingSerie != null)
            {
                existingSerie.DataFieldX = _lineSeries.DataFieldX;
                existingSerie.DataFieldY = _lineSeries.DataFieldY;
                existingSerie.StrokeThickness = _lineSeries.StrokeThickness;
                existingSerie.LineStyle = _lineSeries.LineStyle;
                existingSerie.LabelMargin = _lineSeries.LabelMargin;
                existingSerie.LabelFormatString = _lineSeries.LabelFormatString;
                existingSerie.Color = _lineSeries.Color;
                existingSerie.IsEnabled = _lineSeries.IsEnabled;
            }
            chtChart.InvalidatePlot(true);
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {

            //There is no other option but to make sure we dispose the viewmodel object.
            // Doing so, we make sure to unsubscribe to all the events. 
            // So if in these type of popup windows, if the viewmodel doesn't implement the IDisposable interface
            //  it will throw an exception.

            var disposableVM = this.DataContext as IDisposable;
            disposableVM.Dispose();

        }
    }
}
