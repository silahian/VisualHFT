using System.Windows;
using System.Windows.Controls;
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
        }
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
    }
}
