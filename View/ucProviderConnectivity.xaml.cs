using System.Windows.Controls;

namespace VisualHFT.View
{
    /// <summary>
    /// Interaction logic for ucProviderConnectivity.xaml
    /// </summary>
    public partial class ucProviderConnectivity : UserControl
    {


        public ucProviderConnectivity()
        {
            InitializeComponent();
            this.DataContext = new VisualHFT.ViewModel.vmProvider(Helpers.HelperCommon.GLOBAL_DIALOGS);
        }
    }
}
