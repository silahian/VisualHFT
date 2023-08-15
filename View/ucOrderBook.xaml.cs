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
        //Dependency property
        public static readonly DependencyProperty ucOrderBookSymbolProperty = DependencyProperty.Register(
            "SelectedSymbol",
            typeof(string), typeof(ucOrderBook),
            new UIPropertyMetadata("", new PropertyChangedCallback(symbolChangedCallBack))
            );
        public static readonly DependencyProperty ucOrderBookLayerProperty = DependencyProperty.Register(
            "SelectedLayer",
            typeof(string), typeof(ucOrderBook),
            new UIPropertyMetadata("", new PropertyChangedCallback(layerChangedCallBack))
            );


        public ucOrderBook()
        {
            InitializeComponent();
            //StyleOxyPlot();
        }


        /*private void StyleOxyPlot()
        {

            // Get the brush resources from your Material Design theme
            var backgroundBrush = (SolidColorBrush)FindResource("MaterialDesignPaper");
            var textBrush = (SolidColorBrush)FindResource("MaterialDesignBody");
            var axisBrush = (SolidColorBrush)FindResource("MaterialDesignBody");

            // Get the color values from the brushes
            var backgroundColor = backgroundBrush.Color;
            var textColor = textBrush.Color;
            var axisColor = axisBrush.Color;

            // Create a new PlotModel
            var plotModel = plotCummBid.Model;
            plotModel.Background = OxyColor.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
            plotModel.TextColor = OxyColor.FromArgb(textColor.A, textColor.R, textColor.G, textColor.B);

            foreach (var axe in plotModel.Axes)
            {
                axe.AxislineColor = OxyColor.FromArgb(axisColor.A, axisColor.R, axisColor.G, axisColor.B);
                axe.TicklineColor = OxyColor.FromArgb(axisColor.A, axisColor.R, axisColor.G, axisColor.B);
            }
        }*/
        public string SelectedSymbol
        {
            get { return (string)GetValue(ucOrderBookSymbolProperty); }
            set { SetValue(ucOrderBookSymbolProperty, value); ((VisualHFT.ViewModel.vmOrderBook)this.DataContext).SelectedSymbol = value; }
        }
        public string SelectedLayer
        {
            get { return (string)GetValue(ucOrderBookLayerProperty); }
            set { SetValue(ucOrderBookLayerProperty, value); ((VisualHFT.ViewModel.vmOrderBook)this.DataContext).SelectedLayer = value; }
        }
        static void symbolChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ucOrderBook ucSelf = (ucOrderBook)property;
            ucSelf.SelectedSymbol = (string)args.NewValue;
        }
        static void layerChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ucOrderBook ucSelf = (ucOrderBook)property;
            ucSelf.SelectedLayer = (string)args.NewValue;
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
