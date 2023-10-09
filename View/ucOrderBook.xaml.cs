using OxyPlot;
using OxyPlot.Axes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using VisualHFT.Helpers;

namespace VisualHFT.View
{
    /// <summary>
    /// Interaction logic for ucOrderBook.xaml
    /// </summary>
    public partial class ucOrderBook : UserControl
    {

        public ucOrderBook()
        {
            InitializeComponent();
        }


        private void butPopPriceChart_Click(object sender, RoutedEventArgs e)
        {
            //var newViewModel = new ViewModel.vmOrderBook((ViewModel.vmOrderBook)this.DataContext);
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            HelperCommon.CreateCommonPopUpWindow(chtPrice, (Button) sender, newViewModel);
        }

        private void butPopSpreadChart_Click(object sender, RoutedEventArgs e)
        {
            //var newViewModel = new ViewModel.vmOrderBook((ViewModel.vmOrderBook)this.DataContext);
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            HelperCommon.CreateCommonPopUpWindow(chtSpread, (Button)sender, newViewModel);
        }

        private void butPopSymbol_Click(object sender, RoutedEventArgs e)
        {
            //var newViewModel = new ViewModel.vmOrderBook((ViewModel.vmOrderBook)this.DataContext);
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            HelperCommon.CreateCommonPopUpWindow(grdSymbol, (Button)sender,  newViewModel, "Symbol", 450, 600);
            newViewModel.SetSortDescriptions();
        }

        private void butDepthView1_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.SwitchView = 0;
            newViewModel.SetSortDescriptions();
        }

        private void butDepthView2_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.SwitchView = 1;
            newViewModel.SetSortDescriptions();
        }

        private void butDepthView3_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.SwitchView = 2;
            newViewModel.SetSortDescriptions();
        }

        private void butPopImbalances_Click(object sender, RoutedEventArgs e)
        {
            var currentViewModel = (ViewModel.vmOrderBook)this.DataContext;
            //create model
            var newViewModel = new ViewModel.vmOrderBookFlowAnalysis(Helpers.HelperCommon.GLOBAL_DIALOGS);
            newViewModel.SelectedSymbol = currentViewModel.SelectedSymbol;
            newViewModel.SelectedLayer = currentViewModel.SelectedLayer;
            newViewModel.SelectedProvider = currentViewModel.SelectedProvider;

            // Create a new LineSeries
            var lineSeries = new OxyPlot.Wpf.LineSeries
            {
                DataFieldX = "Date",
                DataFieldY = "Volume",
                StrokeThickness = 2,
                LineStyle = LineStyle.Solid,
                LineJoin = LineJoin.Round,
                LabelMargin = 5,
                LabelFormatString = "",
                Color = Colors.Blue, 
                IsEnabled = false,                
            };
            // Create a new Binding
            var binding = new Binding("RealTimeData");
            // Set the Binding to the ItemsSource property of the LineSeries
            //BindingOperations.SetBinding(lineSeries, LineSeries.ItemsSourceProperty, binding);



            var newForm = new VisualHFT.View.GenericHistoricalLineChart(newViewModel, lineSeries);
            newForm.Title = "LOB Imbalance (vs mid-price): " + newViewModel.SelectedSymbol;
            newForm.chtChart.Title = "";
            newForm.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var axeY = newForm.chtChart.Axes.Where(x => x.Position == AxisPosition.Left).First();
            axeY.Maximum = 1;
            axeY.Minimum = -1;
            newForm.Show();

        }
    }
}
