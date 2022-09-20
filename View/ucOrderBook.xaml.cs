using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            this.DataContext = new VisualHFT.ViewModel.vmOrderBook(Helpers.HelperCommon.GLOBAL_DIALOGS);
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



    }
}
