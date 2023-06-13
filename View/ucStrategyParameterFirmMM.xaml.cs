using System.Windows.Controls;

namespace VisualHFT.View
{
    /// <summary>
    /// Interaction logic for ucStrategyParameterFirmMM.xaml
    /// </summary>
    public partial class ucStrategyParameterFirmMM : UserControl
    {
        //protected VisualHFT.ViewModel.vmStrategyParameterFirmMM _dc;


        public ucStrategyParameterFirmMM()
        {
            InitializeComponent();
            //_dc = new VisualHFT.ViewModel.vmStrategyParameterFirmMM(Helpers.HelperCommon.GLOBAL_DIALOGS, ucStrategyOverview1);
            //this.DataContext = _dc;
            
            //((VisualHFT.ViewModel.vmStrategyParameterFirmMM)this.DataContext).IsActive = Visibility.Hidden;
        }

 

        /*
        public string SelectedStrategy
        {
            get { return (string)GetValue(SelectedStrategyProperty); }
            set { SetValue(SelectedStrategyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedStrategy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedStrategyProperty =
            DependencyProperty.Register("SelectedStrategy", typeof(string), typeof(ucStrategyParameterFirmMM), new UIPropertyMetadata(""));



        public string SelectedSymbol
        {
            get { return (string)GetValue(SelectedSymbolProperty); }
            set { SetValue(SelectedSymbolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSymbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSymbolProperty =
            DependencyProperty.Register("SelectedSymbol", typeof(string), typeof(ucStrategyParameterFirmMM), new UIPropertyMetadata(""));




        public string SelectedLayer
        {
            get { return (string)GetValue(SelectedLayerProperty); }
            set { SetValue(SelectedLayerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLayer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register("SelectedLayer", typeof(string), typeof(ucStrategyParameterFirmMM), new UIPropertyMetadata(""));

        */

    }
}
