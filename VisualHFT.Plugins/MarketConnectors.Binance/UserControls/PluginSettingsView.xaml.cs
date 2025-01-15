using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MarketConnectors.Binance.UserControls
{
    /// <summary>
    /// Interaction logic for PluginSettingsView.xaml
    /// </summary>
    public partial class PluginSettingsView : UserControl
    {
        public PluginSettingsView()
        {
            InitializeComponent();
        }
        private void ShowTooltip(object sender, MouseEventArgs e)
        {
            TooltipPopup.IsOpen = true;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}