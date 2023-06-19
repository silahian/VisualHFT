using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VisualHFT.View
{
    public class SeriesViewModel
	{
		public string Key { get; set; }
		public string SeriesType { get; set; }
		public ObservableCollection<PlotInfoPriceChart> Items { get; set; }
	}


	/// <summary>
	/// Interaction logic for PriceCharting.xaml
	/// </summary>
	public partial class PriceCharting : Window, INotifyPropertyChanged
	{
		private HashSet<string> propertyNamesSet;
		private UIActionDispatcher dispatcher;

		public event PropertyChangedEventHandler PropertyChanged;

		protected void DispatchPropertyChanged(string propertyName)
		{
			lock (this.propertyNamesSet)
			{
				this.propertyNamesSet.Add(propertyName);
			}

			this.dispatcher.Dispatch("RaisePropertyChanged", RaisePropertyChanged);
		}

		protected void DispatchAction(string actionKey, Action action)
		{
			this.dispatcher.Dispatch(actionKey, action);
		}
		private void RaisePropertyChanged()
		{
			List<string> propertyNamesList = new List<string>();
			lock (this.propertyNamesSet)
			{
				propertyNamesList = new List<string>(this.propertyNamesSet);
				this.propertyNamesSet.Clear();
			}

			var h = this.PropertyChanged;
			if (h != null)
			{
				foreach (string propertyName in propertyNamesList)
				{
					h(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}



		protected void RaisePropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		protected string _selectedSymbol;
		protected List<string> _selectedProviders;
		protected int REALTIME_ITEM_POINTS = 1000;
		protected vmPriceCharting _priceChartingVM;
		protected ObservableCollection<SeriesViewModel> _realTimePrices;
		protected Dictionary<string, ObservableCollection<PlotInfoPriceChart>> _realTimeSpread;
		static object PROVIDERS_OnDataReceived_LOCK = new object();

		public ObservableCollection<SeriesViewModel> RealTimePrices {
			get
			{
				return _realTimePrices;
			}
			set
			{
				_realTimePrices = value;
			}
		}
		public PriceCharting()
		{
			InitializeComponent();
			this.propertyNamesSet = new HashSet<string>();
			this.dispatcher = new UIActionDispatcher();
			this._priceChartingVM = new vmPriceCharting();
			DataContext = this;
			//LoadInitialData();

			//Helpers.HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
		}
		public vmPriceCharting PriceChartingDataContext
		{
			get
			{
				return _priceChartingVM;
			}
		}
		private void LoadInitialData()
		{
			//fill symbols
			cboSymbols.ItemsSource = HelperCommon.ALLSYMBOLS;

			//fill providers
			var allProviders = HelperCommon.PROVIDERS.Select(x => x.Value).ToList();
			allProviders.Insert(0, new Model.ProviderVM() { ProviderID = 0, ProviderName = "ALL" });
			lstProviders.ItemsSource = allProviders;

		}
		private DateTime Max(DateTime a, DateTime b)
		{
			return a > b ? a : b;
		}

		private List<string> GetSelectedProviders()
		{
			List<string> aRet = new List<string>();
			foreach (Provider item in lstProviders.Items)
			{
				ListBoxItem objectItem = (ListBoxItem)lstProviders.ItemContainerGenerator.ContainerFromItem(item);
				CheckBox chkProvider = HelperUtility.FindChild<CheckBox>(objectItem, "chkProvider");
				if (chkProvider != null && chkProvider.IsChecked.Value)
				{
					aRet.Add(chkProvider.Content.ToString());
				}
			}

			return aRet;
		}
		private void PROVIDERS_OnDataReceived(object sender, Model.Provider e)
		{
			try
			{
				var selectedProviders = GetSelectedProviders();
				if (!selectedProviders.Any()  || string.IsNullOrEmpty(_selectedSymbol))
				{
					return;
				}

				if (_realTimePrices == null)
				{
					_realTimePrices = new ObservableCollection<SeriesViewModel>();
					RaisePropertyChanged("RealTimePrices");
				}

				lock (PROVIDERS_OnDataReceived_LOCK)
				{
					int provIndex = 0;
					foreach (var _prov in HelperCommon.PROVIDERS)
					{
						if (selectedProviders.Any(x => x == "ALL") || selectedProviders.Any(x => x == _prov.Value.ProviderName))
						{
							string _key = _prov.Value.ProviderID.ToString() + "_" + _selectedSymbol;
							OrderBook orderBook;
							HelperCommon.LIMITORDERBOOK.TryGetValue(_key, out orderBook);
							if (orderBook == null)
								continue;
							
							var tobBID = orderBook.GetTOB(true);
							if (tobBID == null)
								tobBID = new BookItem();
							var tobASK = orderBook.GetTOB(false);
							if (tobASK == null)
								tobASK = new BookItem();
							if (tobBID.Price == 0 || tobASK.Price == 0)
							{
								continue;
							}
							if (!_realTimePrices.Any(x => x.Key == _prov.Value.ProviderName))
							{
								_realTimePrices.Add(new SeriesViewModel()
								{
									Key = _prov.Value.ProviderName,
									Items = new ObservableCollection<PlotInfoPriceChart>(),
									SeriesType = "Line"
								});
							}
							ObservableCollection<PlotInfoPriceChart> colPrices = _realTimePrices.First(x => x.Key == _prov.Value.ProviderName).Items;
							if (colPrices.Count > REALTIME_ITEM_POINTS)
							{
								colPrices.RemoveAt(0);
							}
							double MidPoint = 0;
							if (tobASK.Price.HasValue && tobBID.Price.HasValue)
							{
								MidPoint = (tobASK.Price.Value + tobBID.Price.Value) / 2;
							}

							DateTime maxDate = DateTime.Now;// Max(colPrices.DefaultIfEmpty(new PlotInfoPriceChart()).Max(d => d.Date), Max(tobASK.LocalTimeStamp, tobBID.LocalTimeStamp));
							colPrices.Add(new PlotInfoPriceChart()
							{
								Date = maxDate,
								MidPrice = MidPoint,
								AskPrice = tobASK.Price.Value,
								BidPrice = tobBID.Price.Value,
								Volume = tobASK.Size.Value + tobBID.Size.Value,
								//StrokeAsk = GetNextBrush(provIndex),
								//StrokeMiddle = GetNextBrush(provIndex),
								//StrokeBid = GetNextBrush(provIndex)
							});
							provIndex++;
						}
					}
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		private void CboSymbols_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedSymbol = cboSymbols.SelectedItem.ToString();
		}	
		private void LstProviders_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_selectedProviders == null)
			{
				_selectedProviders = new List<string>();
			}

		}

		private Brush GetNextBrush(int index)
		{
			return chart.Palette.SeriesEntries[index].First().Fill;
		}
	}
}
