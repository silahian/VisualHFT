using VisualHFT.Model;
using VisualHFT.View.StatisticsView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ucStrategyParameterFirmMM.xaml
    /// </summary>
    public partial class ucStrategyParameterBBook : UserControl
    {
        //Dependency property
        public static readonly DependencyProperty ucStrategyParameterBBookSymbolProperty = DependencyProperty.Register(
            "SelectedSymbol", 
            typeof(string), typeof(ucStrategyParameterBBook),
            new UIPropertyMetadata("", new PropertyChangedCallback(symbolChangedCallBack))
            );
        public static readonly DependencyProperty ucStrategyParameterBBookLayerProperty = DependencyProperty.Register(
            "SelectedLayer",
            typeof(string), typeof(ucStrategyParameterBBook),
            new UIPropertyMetadata("", new PropertyChangedCallback(layerChangedCallBack))
            );
        public static readonly DependencyProperty ucStrategyParameterBBookSelectedStrategyProperty = DependencyProperty.Register(
            "SelectedStrategy",
            typeof(string), typeof(ucStrategyParameterBBook),
            new UIPropertyMetadata("", new PropertyChangedCallback(strategyChangedCallBack))
            );

        public ucStrategyParameterBBook()
        {
            InitializeComponent();
            this.DataContext = new VisualHFT.ViewModel.vmStrategyParameterBBook(Helpers.HelperCommon.GLOBAL_DIALOGS, ucStrategyOverview1);
            ((VisualHFT.ViewModel.vmStrategyParameterBBook)this.DataContext).IsActive = Visibility.Hidden;
        }


		private string _selectedSymbol;

		public string SelectedSymbol
        {
            get { return _selectedSymbol; }
            set { _selectedSymbol = value; ((VisualHFT.ViewModel.vmStrategyParameterBBook)this.DataContext).SelectedSymbol = value; }
        }

		private string _selectedLayer;
		public string SelectedLayer
        {
            get { return _selectedLayer; }
            set {
				_selectedLayer = value;
				((VisualHFT.ViewModel.vmStrategyParameterBBook)this.DataContext).SelectedLayer = value;
			}
        }

		
		private string _selectedStrategy;
		public string SelectedStrategy
		{
			get { return _selectedStrategy; }
			set
			{
				_selectedStrategy = value;
				((VisualHFT.ViewModel.vmStrategyParameterBBook)this.DataContext).SelectedStrategy = value;
			}
		}


		static void symbolChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
			ucStrategyParameterBBook ucSelf = (ucStrategyParameterBBook)property;
            ucSelf.SelectedSymbol = (string)args.NewValue;
        }
        static void layerChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
			ucStrategyParameterBBook ucSelf = (ucStrategyParameterBBook)property;
            ucSelf.SelectedLayer = (string)args.NewValue;
        }
        static void strategyChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ucStrategyParameterBBook ucSelf = (ucStrategyParameterBBook)property;
            ucSelf.SelectedStrategy = (string)args.NewValue;
        }
		
    }
}
