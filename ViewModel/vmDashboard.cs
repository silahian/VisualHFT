using VisualHFT.Helpers;
using VisualHFT.View;
using VisualHFT.View.StatisticsView;
using VisualHFT.ViewModel.StatisticsViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using Telerik.Windows.Controls.GridView;
using VisualHFT.Model;

namespace VisualHFT.ViewModel
{
    public class vmDashboard : INotifyPropertyChanged
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private string _selectedSymbol;
        private string _selectedLayer;
        private string _selectedStrategy;
        
        protected vmStrategyParameterFirmMM _vmStrategyParamsFirmMM;
        protected vmPosition _vmPosition;
        protected vmOrderBook _vmOrderBook;

        public event PropertyChangedEventHandler PropertyChanged;

        public vmDashboard(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            cmdAbort = new RelayCommand(DoAbort);

            HelperCommon.ACTIVESTRATEGIES.CollectionChanged += ACTIVESTRATEGIES_CollectionChanged;
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();

            
            this.StrategyParamsFirmMM = new vmStrategyParameterFirmMM(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.Positions = new vmPosition(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.OrderBook = new vmOrderBook(Helpers.HelperCommon.GLOBAL_DIALOGS);
        }
        public vmStrategyParameterFirmMM StrategyParamsFirmMM
        {
            get { return _vmStrategyParamsFirmMM;  }
            set { _vmStrategyParamsFirmMM = value; RaisePropertyChanged("StrategyParamsFirmMM"); }
        }
        public vmPosition Positions
        {
            get { return _vmPosition;  }
            set { _vmPosition = value; RaisePropertyChanged("Positions"); }
        }
        public vmOrderBook OrderBook
        {
            get { return _vmOrderBook; }
            set { _vmOrderBook = value; RaisePropertyChanged("OrderBook"); }
        }

        public RelayCommand cmdAbort { get; set; }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

		private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			RefreshSymbolList();
		}
		private void ACTIVESTRATEGIES_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged("StrategyList");
		}


        public ObservableCollection<string> StrategyList
        {
            get { return HelperCommon.ACTIVESTRATEGIES; }
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            RefreshSymbolList();
        }

        private void RefreshSymbolList()
        {
            try
            {
                RaisePropertyChanged("SymbolList");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
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
			get
			{
				return _selectedStrategy;
			}

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

                    RaisePropertyChanged("SelectedStrategy");                    
					RaisePropertyChanged("SelectedSymbol");
					RaisePropertyChanged("SelectedLayer");
				}
			}
		}
		public string SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
				if (_selectedSymbol != value)
				{
					_selectedSymbol = value;
                    _vmStrategyParamsFirmMM.SelectedSymbol = value;
                    _vmPosition.SelectedSymbol = value;
                    _vmOrderBook.SelectedSymbol = value;

                    RaisePropertyChanged("SelectedSymbol");
				}
            }
        }
        public string SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
				if (_selectedLayer != value)
				{
					_selectedLayer = value;
                    _vmOrderBook.SelectedLayer = value;

					RaisePropertyChanged("SelectedLayer");
				}
            }
        }

        public ObservableCollection<string> SymbolList
        {
            get
            {
                return HelperCommon.ALLSYMBOLS;
            }
        }


	}
}
