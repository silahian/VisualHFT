using System;
using System.Linq;
using System.Windows;
using System.Globalization;
using System.Windows.Markup;
using VisualHFT.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

            InitializeComponent();
            this.DataContext = new VisualHFT.ViewModel.vmDashboard(Helpers.HelperCommon.GLOBAL_DIALOGS);

        }

        private void ButtonAnalyticsReport_Click(object sender, RoutedEventArgs e)
        {
            /*
            AnalyticReport.AnalyticReport oReport = new AnalyticReport.AnalyticReport();
            try
            {
                oReport.Signals = VisualHFT.Commons.Helpers.HelperCommon.EXECUTEDORDERS.Positions.Where(x => x.PipsPnLInCurrency.HasValue && cboSelectedSymbol.SelectedValue.ToString() == x.Symbol).OrderBy(x => x.CreationTimeStamp).ToList();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "ERRROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            oReport.Show();
            */
        }

        private void ButtonAppSettings_Click(object sender, RoutedEventArgs e)
        {
            var vm = new vmUserSettings();
            vm.LoadJson(SettingsManager.Instance.GetAllSettings());

            var form = new View.UserSettings();
            form.DataContext = vm;
            form.ShowDialog();
        }

        private void ButtonMultiVenuePrices_Click(object sender, RoutedEventArgs e)
        {
            var form = new View.MultiVenuePrices();
            form.DataContext = new vmMultiVenuePrices();
            form.Show();
        }

        private void ButtonPluginManagement_Click(object sender, RoutedEventArgs e)
        {
            var form = new View.PluginManagerWindow();
            form.DataContext = new vmPluginManager();
            form.Show();
        }
    }
}
