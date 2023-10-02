using VisualHFT.Helpers;
using VisualHFT.View;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using VisualHFT.DataRetriever;
using VisualHFT.DataRetriever.DataParsers;
using System.Diagnostics;
using VisualHFT.ViewModels;
using VisualHFT.Model;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Collections.Generic;
using VisualHFT.ViewModel;
using VisualHFT.UserSettings;
using System.Threading.Tasks;

namespace VisualHFT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
            this.DataContext = new VisualHFT.ViewModel.vmDashboard(Helpers.HelperCommon.GLOBAL_DIALOGS);

        }

        private void ButtonAnalyticsReport_Click(object sender, RoutedEventArgs e)
        {
            AnalyticReport.AnalyticReport oReport = new AnalyticReport.AnalyticReport();
            try
            {
                if (cboSelectedSymbol.SelectedValue == null || cboSelectedSymbol.SelectedValue.ToString() == "")
                {
                    oReport.Signals = Helpers.HelperCommon.EXECUTEDORDERS.Positions.Where(x => x.PipsPnLInCurrency.HasValue).OrderBy(x => x.CreationTimeStamp).ToList();
                }
                else
                {
                    oReport.Signals = Helpers.HelperCommon.EXECUTEDORDERS.Positions.Where(x => x.PipsPnLInCurrency.HasValue && cboSelectedSymbol.SelectedValue.ToString() == x.Symbol).OrderBy(x => x.CreationTimeStamp).ToList();
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "ERRROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            oReport.Show();

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
