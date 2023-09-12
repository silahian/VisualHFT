using VisualHFT.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Prism.Mvvm;
using VisualHFT.ViewModel.Studies;
using System.Windows.Documents;
using System.Windows.Ink;
using VisualHFT.Model;
using LumenWorks.Framework.IO.Csv;
using System.Net.Sockets;
using System.Windows.Media;
using VisualHFT.ViewModels;

namespace VisualHFT.ViewModel
{
    public class vmDashboard : BindableBase
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private string _selectedSymbol;
        private string _selectedLayer;
        private string _selectedStrategy;

        protected vmStrategyParameterFirmMM _vmStrategyParamsFirmMM;
        protected vmPosition _vmPosition;
        protected vmOrderBook _vmOrderBook;
        public ObservableCollection<Studies.MetricTileViewModel> Tiles { get; set; }


        public vmDashboard(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            CmdAbort = new RelayCommand(DoAbort);

            HelperCommon.ACTIVESTRATEGIES.CollectionChanged += ACTIVESTRATEGIES_CollectionChanged;
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();


            this.StrategyParamsFirmMM = new vmStrategyParameterFirmMM(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.Positions = new vmPosition(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.OrderBook = new vmOrderBook(Helpers.HelperCommon.GLOBAL_DIALOGS);

            LoadTiles();
        }

        private void LoadTiles()
        {
            Tiles = new ObservableCollection<MetricTileViewModel>();
            Tiles.Add(new MetricTileViewModel(eTILES_TYPE.STUDY_LOB_IMBALANCE, "dashboard_LOB_Imbalance")
            {
                Value = "",
                Title = "LOB Imbalance",
                Tooltip = "The <b>Limit Order Book Imbalance</b> represents the disparity between buy and sell orders at a specific price level.<br/><br/>" +
                "It highlights the difference in demand and supply in the order book, providing insights into potential price movements.<br/>" +
                "A significant imbalance can indicate a strong buying or selling interest at that price."                
            });
            Tiles.Add(new MetricTileViewModel(eTILES_TYPE.STUDY_VPIN, "dashboard_VPIN")
            {
                Value = "",
                Title = "VPIN",
                Tooltip = "The <b>VPIN</b> (Volume - Synchronized Probability of Informed Trading) value is a measure of the imbalance between buy and sell volumes in a given bucket.<br/><br/>" +
                "It's calculated as the absolute difference between buy and sell volumes divided by the total volume (buy + sell) for that bucket.<br/>"
            });
            Tiles.Add(new MetricTileViewModel(eTILES_TYPE.STUDY_TTO, "dashboard_TTO")
            {
                Value = "",
                Title = "TTO",
                Tooltip = "The <b>TTO</b> (Volume - Trade To Order Ratio) value is a key metric that measures the efficiency of trading by comparing the number of executed trades to the number of orders placed.<br/><br/>" +
                "<b>TTO</b> is calculation as follows: <i>TTO Ratio=Number of Executed Trades / Number of Orders Placed</i><br/>" +
                ""
            });
            Tiles.Add(new MetricTileViewModel(eTILES_TYPE.STUDY_OTT, "dashboard_OTT")
            {
                Value = "",
                Title = "OTT",
                Tooltip = "The <b>OTT</b> (Volume - Order To Trade Ratio) is a key metric used to evaluate trading behavior. <br/> It measures the number of orders placed relative to the number of trades executed. This ratio is often <b>monitored by regulatory bodies</b> to identify potentially manipulative or disruptive trading activities.<br/><br/>" +
                "<b>OTT</b> is calculation as follows: <i>OTT Ratio  = Number of Orders Placed / Number of Executed Trades</i><br/>" +
                ""
            });
            Tiles.Add(new MetricTileViewModel(eTILES_TYPE.STUDY_MARKETRESILIENCE, "dashboard_MKTRESIL")
            {
                Value = "",
                Title = "MR",
                Tooltip = "<b>Market Resilience</b> (MR) is a real-time metric that quantifies how quickly a market rebounds after experiencing a large trade. <br/> It's an invaluable tool for traders to gauge market stability and sentiment.<br/><br/>" +
                "The <b>MR</b> score is a composite index derived from two key market behaviors:<br/>" +
                "1. <b>Spread Recovery:</b> Measures how quickly the gap between buying and selling prices returns to its normal state after a large trade.<br/>" +
                "2. <b>Depth Recovery:</b>  Assesses how fast the consumed levels of the Limit Order Book (LOB) are replenished post-trade.<br/>" +
                "<br/>" +
                "The <b>MR</b> score is the average of these two normalized metrics, ranging from 0 (no recovery) to 1 (full recovery)."
            });
            Tiles.Add(new MetricTileViewModel(eTILES_TYPE.STUDY_MARKETRESILIENCEBIAS, "dashboard_MKTRESILBIAS")
            {
                Value = "",
                Title = "MBR",
                Tooltip = "<b>Market Resilience Bias</b> (MRB) is a real-time metric that quantifies the directional tendency of the market following a large trade. <br/> It provides insights into the prevailing sentiment among market participants, enhancing traders' understanding of market dynamics.<br/><br/>" +
                "The <b>MRB</b> score is derived from the behavior of the Limit Order Book (LOB) post-trade:<br/>" +
                "1. <b>Volume Addition Rate:</b> Analyzes the rate at which volume is added to the bid and ask sides of the LOB after a trade.<br/>" +
                "2. <b>Directional Inclination:</b> Determines whether the market is leaning towards a bullish or bearish stance based on the volume addition rate.<br/>" +
                "<br/>" +
                "The <b>MRB</b> score indicates the market's bias, with a value of 1 representing a bullish sentiment (sentiment up) and 0 representing a bearish sentiment (sentiment down)."

             });
        }


        public vmStrategyParameterFirmMM StrategyParamsFirmMM
        {
            get => _vmStrategyParamsFirmMM;
            set => SetProperty(ref _vmStrategyParamsFirmMM, value);
        }
        public vmPosition Positions
        {
            get => _vmPosition;
            set => SetProperty(ref _vmPosition, value);
        }
        public vmOrderBook OrderBook
        {
            get => _vmOrderBook;
            set => SetProperty(ref _vmOrderBook, value);
        }

        public RelayCommand CmdAbort { get; set; }

        private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshSymbolList();
        }
        private void ACTIVESTRATEGIES_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(nameof(StrategyList));

        public ObservableCollection<string> StrategyList => HelperCommon.ACTIVESTRATEGIES;

        private void dispatcherTimer_Tick(object sender, EventArgs e) => RefreshSymbolList();

        private void RefreshSymbolList()
        {
            try
            {
                RaisePropertyChanged(nameof(SymbolList));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DoAbort(object item)
        {
            if (_dialogs.ContainsKey("confirm"))
            {
                if (!_dialogs["confirm"]("Are you sure you want to abort the system?", ""))
                    return;
            }
            System.ComponentModel.BackgroundWorker bwDoAbort = new System.ComponentModel.BackgroundWorker();
            bwDoAbort.DoWork += (ss, args) =>
            {
                try
                {
                    args.Result = RESTFulHelper.SetVariable<string>("ABORTSYSTEM");
                }
                catch { /*System.Threading.Thread.Sleep(5000);*/ }
            };
            bwDoAbort.RunWorkerCompleted += (ss, args) =>
            {
                if (args.Result == null)
                    _dialogs["popup"]("Message timeout.", "System Abort");
                else if (args.Result.ToBoolean())
                    _dialogs["popup"]("Message received OK.", "System Abort");
                else
                    _dialogs["popup"]("Message failed.", "System Abort");
            };
            if (!bwDoAbort.IsBusy)
                bwDoAbort.RunWorkerAsync();

        }


        public string SelectedStrategy
        {
            get => _selectedStrategy;

            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "";
                if (value.IndexOf(":") > -1)
                {
                    _selectedStrategy = value.Split(':')[0].Trim();
                    _selectedLayer = value.Split(':')[1].Trim();
                }
                else
                {
                    _selectedStrategy = value;
                    _selectedLayer = "";
                }
                if (value != "")
                {
                    _selectedSymbol = "-- All symbols --";
                    _vmStrategyParamsFirmMM.SelectedStrategy = value;
                    _vmPosition.SelectedStrategy = value;

                    RaisePropertyChanged(nameof(SelectedStrategy));
                    RaisePropertyChanged(nameof(SelectedSymbol));
                    RaisePropertyChanged(nameof(SelectedLayer));
                };
            }
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    _vmStrategyParamsFirmMM.SelectedSymbol = value;
                    _vmPosition.SelectedSymbol = value;
                    _vmOrderBook.SelectedSymbol = value;

                    RaisePropertyChanged(nameof(SelectedSymbol));
                }
            }
        }
        public string SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                if (_selectedLayer != value)
                {
                    _selectedLayer = value;
                    _vmOrderBook.SelectedLayer = value;

                    RaisePropertyChanged(nameof(SelectedLayer));
                }
            }
        }

        public ObservableCollection<string> SymbolList => HelperCommon.ALLSYMBOLS;

    }
}
