using VisualHFT.Helpers;
using VisualHFT.View;
using VisualHFT.View.StatisticsView;
using VisualHFT.ViewModel;
using VisualHFT.ViewModel.StatisticsViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
                HelperWebsocket oWS = new HelperWebsocket();
                oWS.Connect();
            }).Start();

            this.DataContext = new VisualHFT.ViewModel.vmDashboard(Helpers.HelperCommon.GLOBAL_DIALOGS);
            


            ucPosition1.SetBinding(ucPositions.ucPositionsSymbolProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedSymbol"),
                Mode = BindingMode.TwoWay
            });
            ucPosition1.SetBinding(ucPositions.ucPositionsSelectedStrategyProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedStrategy"),
                Mode = BindingMode.TwoWay
            });




            /*ucFirmMM.SetBinding(ucStrategyParameterFirmMM.ucStrategyParameterFirmMMSymbolProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedSymbol"),
                Mode = BindingMode.TwoWay
            });
            ucFirmMM.SetBinding(ucStrategyParameterFirmMM.ucStrategyParameterFirmMMLayerProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedLayer"),
                Mode = BindingMode.TwoWay
            });
            ucFirmMM.SetBinding(ucStrategyParameterFirmMM.ucStrategyParameterFirmMMSelectedStrategyProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedStrategy"),
                Mode = BindingMode.TwoWay
            });*/


            /*
            ucFirmBB.SetBinding(ucStrategyParameterFirmBB.ucStrategyParameterFirmBBSymbolProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedSymbol"),
                Mode = BindingMode.TwoWay
            });
            ucFirmBB.SetBinding(ucStrategyParameterFirmBB.ucStrategyParameterFirmBBLayerProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedLayer"),
                Mode = BindingMode.TwoWay
            });
            ucFirmBB.SetBinding(ucStrategyParameterFirmBB.ucStrategyParameterFirmBBSelectedStrategyProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedStrategy"),
                Mode = BindingMode.TwoWay
            });




			ucHFTAcceptor.SetBinding(ucStrategyParameterHFTAcceptor.ucStrategyParameterHFTAcceptorSymbolProperty, new Binding
			{
				Source = DataContext,
				Path = new PropertyPath("SelectedSymbol"),
				Mode = BindingMode.TwoWay
			});
			ucHFTAcceptor.SetBinding(ucStrategyParameterHFTAcceptor.ucStrategyParameterHFTAcceptorLayerProperty, new Binding
			{
				Source = DataContext,
				Path = new PropertyPath("SelectedLayer"),
				Mode = BindingMode.TwoWay
			});
			ucHFTAcceptor.SetBinding(ucStrategyParameterHFTAcceptor.ucStrategyParameterHFTAcceptorSelectedStrategyProperty, new Binding
			{
				Source = DataContext,
				Path = new PropertyPath("SelectedStrategy"),
				Mode = BindingMode.TwoWay
			});
            */

			ucOrderBook1.SetBinding(ucOrderBook.ucOrderBookSymbolProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedSymbol"),
                Mode = BindingMode.TwoWay
            });
            ucOrderBook1.SetBinding(ucOrderBook.ucOrderBookLayerProperty, new Binding
            {
                Source = DataContext,
                Path = new PropertyPath("SelectedLayer"),
                Mode = BindingMode.TwoWay
            });

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

		private void ButtonPriceCharting_Click(object sender, RoutedEventArgs e)
		{
			PriceCharting oForm = new PriceCharting();
			oForm.Show();
		}
	}
}
    