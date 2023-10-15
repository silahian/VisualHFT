using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.ViewModel
{
    public class vmOrderBookFlowAnalysis : BindableBase, IDisposable
    {
        private OrderBook _orderBook;
        protected object MTX_ORDERBOOK = new object();
        private Dictionary<string, Func<string, string, bool>> _dialogs;


        private ObservableCollection<Provider> _providers;
        private string _selectedSymbol;
        private Provider _selectedProvider = null;
        private string _layerName;

        private DispatcherTimer timerUI = new DispatcherTimer();
        private List<PlotInfoPriceChart> _realTimeData;
        private List<PlotInfoPriceChart> TRACK_RealTimeData = new List<PlotInfoPriceChart>();

        public vmOrderBookFlowAnalysis(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            EventAggregator.Instance.OnOrderBookDataReceived += LIMITORDERBOOK_OnDataReceived;

            timerUI.Interval = TimeSpan.FromMilliseconds(1);
            timerUI.Tick += TimerUI_Tick;
            timerUI.Start();
        }
        public vmOrderBookFlowAnalysis(vmOrderBook vm)
        {
            //this._providers = vm._providers;
            //this._dialogs = vm._dialogs;
            this._orderBook = vm.OrderBook;
            this._selectedSymbol = vm.SelectedSymbol;
            this._selectedProvider = vm.SelectedProvider;
            this._layerName = vm.SelectedLayer;
        }
        public void Dispose()
        {
            this.timerUI.Stop(); //stop timer
            HelperCommon.LIMITORDERBOOK.OnDataReceived -= LIMITORDERBOOK_OnDataReceived; //unsubscribe
        }

        private void TimerUI_Tick(object sender, EventArgs e)
        {
            var localRealTime = RealTimeData?.ToList();

            if (localRealTime != null && !TRACK_RealTimeData.SequenceEqual(localRealTime))
            {
                RaisePropertyChanged(nameof(RealTimeData));
                TRACK_RealTimeData = localRealTime;
            }

        }
        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            if (e == null)
                return;
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderCode != e.ProviderID)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --" || _selectedSymbol != e.Symbol)
                return;
            lock (MTX_ORDERBOOK)
            {
                if (_orderBook == null || _orderBook.ProviderID != e.ProviderID || _orderBook.Symbol != e.Symbol)
                {
                    _orderBook = e;
                    _realTimeData = new List<PlotInfoPriceChart>();
                }
                if (!_orderBook.LoadData(e.Asks, e.Bids))
                    return; //if nothing to update, then exit

                #region REAL TIME DATA
                if (_realTimeData != null && _orderBook != null)
                {
                    //Imbalance
                    PlotInfoPriceChart objWithHigherDate = _realTimeData.OrderBy(x => x.Date).LastOrDefault();
                    var objToAdd = new PlotInfoPriceChart() { Date = DateTime.Now, Volume = _orderBook.ImbalanceValue, MidPrice = _orderBook.MidPrice };
                    if (objWithHigherDate == null || objToAdd.Date.Subtract(objWithHigherDate.Date).TotalMilliseconds > 10)
                    {
                        _realTimeData.Add(objToAdd);
                    }
                    if (_realTimeData.Count > 300) //max chart points = 300
                        _realTimeData.RemoveAt(0);
                }
                #endregion


            }
        }
        private void Clear()
        {
            _realTimeData = null;
            _orderBook = null;
            RaisePropertyChanged(nameof(RealTimeData));
        }



        public List<PlotInfoPriceChart> RealTimeData
        {
            get
            {
                lock (MTX_ORDERBOOK)
                {
                    if (_realTimeData == null)
                        return null;
                    else
                        return _realTimeData.ToList();
                }
            }
        }
        public OrderBook OrderBook
        {
            get => _orderBook;
            set => SetProperty(ref _orderBook, value);
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());

        }
        public Provider SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value, onChanged: () => this.OrderBook = null);
        }
        public string SelectedLayer
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value, onChanged: () => this.OrderBook = null);
        }
        public ObservableCollection<Provider> Providers => _providers;
    }
}
