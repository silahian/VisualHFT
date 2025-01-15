using System.Timers;
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
            var data = new VisualHFT.ViewModel.vmProvider(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.DataContext = data;

            string providers = data.Providers.ToString();

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += (s, e) =>
            {
                providers = string.Join(",", data.Providers);
                string str = providers;

            };

            aTimer.Interval = 2000; // ~ 5 seconds
            aTimer.Enabled = true;
             
        }
    }
}
