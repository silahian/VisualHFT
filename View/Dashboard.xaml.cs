using VisualHFT.Helpers;
using VisualHFT.View;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using VisualHFT.DataRetriever;
using VisualHFT.DataRetriever.DataParsers;
using System.Diagnostics;

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

            //START SNIFFER THREAD 
            /*
            new Thread(() =>
                Helpers.HelperFIXSniffer.Start()
            ).Start();
            */
            //START WEBSOCKET LISTENER THREAD 
            new Thread(() => {
                Thread.CurrentThread.IsBackground = true;

                //DATA RETRIEVER = WEBSOCKETS
                var dataRetriever = new WebSocketDataRetriever(new JsonParser());
                var processor = new DataProcessor(dataRetriever);
                dataRetriever.Start();


                while (true) { } ;


            }).Start();

            this.DataContext = new VisualHFT.ViewModel.vmDashboard(Helpers.HelperCommon.GLOBAL_DIALOGS);
            
        }

        private void ButtonAnalyticsReport_Click(object sender, RoutedEventArgs e)
        {
            AnalyticReport.AnalyticReport oReport = new AnalyticReport.AnalyticReport();
            try
            {
                if (cboSelectedSymbol.SelectedValue == null || cboSelectedSymbol.SelectedValue.ToString() == "")
                {
                    oReport.Signals = Helpers.HelperCommon.CLOSEDPOSITIONS.Positions.Where(x => x.PipsPnLInCurrency.HasValue).OrderBy(x => x.CreationTimeStamp).ToList();
                }
                else
                {
                    oReport.Signals = Helpers.HelperCommon.CLOSEDPOSITIONS.Positions.Where(x => x.PipsPnLInCurrency.HasValue && cboSelectedSymbol.SelectedValue.ToString() == x.Symbol).OrderBy(x => x.CreationTimeStamp).ToList();
                }
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "ERRROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            oReport.Show();

        }
	}
}
    