using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.View.StatisticsView;
using VisualHFT.ViewModel.StatisticsViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace VisualHFT.ViewModel
{
	public class vmStrategyParametersBase<T> where T: INotifyPropertyChanged
    {
		protected string _layerName;
		protected string _selectedSymbol;
		protected string _selectedStrategy;
		protected string _strategyNameForThisControl;
		protected bool _updateButtonIsEnabled;
		protected bool _saveDBButtonIsEnabled;
		protected Visibility _isActive;
        private ucStrategyOverview ucStrategyOverview;
		protected vmStrategyOverview _vmStrategyOverview;
        protected string _cmdStartImage;
		protected string _cmdStopImage;
		protected RelayCommand _cmdSaveToDB;
		protected RelayCommand _cmdStart;
		protected RelayCommand _cmdStop;
		protected RelayCommand _cmdUpdate;
		protected ObservableCollection<PositionEx> _positions;
		protected Dictionary<string, Func<string, string, bool>> _dialogs;
		protected System.ComponentModel.BackgroundWorker bwGetParameters = new System.ComponentModel.BackgroundWorker();
		protected System.ComponentModel.BackgroundWorker bwSetParameters = new System.ComponentModel.BackgroundWorker();
		protected string _VolumeTraded;
		protected string _ExecutionLatencyLastHour;
		protected T _model;
		protected List<T> modelItems;

		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}



		public vmStrategyParametersBase(Dictionary<string, Func<string, string, bool>> dialogs, ucStrategyOverview uc)
		{			

            this._dialogs = dialogs;

            _vmStrategyOverview = new vmStrategyOverview(Helpers.HelperCommon.GLOBAL_DIALOGS);
			uc.DataContext = _vmStrategyOverview;
            this.UcStrategyOverview = uc;
			this.IsActive = Visibility.Hidden;

            cmdStop = new RelayCommand(DoStop);			
			cmdStart = new RelayCommand(DoStart);
			cmdUpdate = new RelayCommand(DoUpdate);
			cmdSaveToDB = new RelayCommand(DoSaveToDB);
		}
		public virtual void Load()
		{
			GetParameters();
			HelperCommon.CLOSEDPOSITIONS.Positions.CollectionChanged += Positions_CollectionChanged;			
			HelperCommon.STRATEGYPARAMS.OnDataUpdateReceived += STRATEGYPARAMS_OnDataUpdateReceived;
		}
		public virtual void Unload()
		{
			HelperCommon.CLOSEDPOSITIONS.Positions.CollectionChanged -= Positions_CollectionChanged;
			HelperCommon.STRATEGYPARAMS.OnDataUpdateReceived -= STRATEGYPARAMS_OnDataUpdateReceived;
		}

		public virtual void OnUpdateToAllModelsIfAllSymbolsIsSelected()
		{
			throw new NotImplementedException();
		}
		public virtual void OnSaveSettingsToDB() { }

		public Visibility IsActive
		{
			get { return _isActive; }
			set { _isActive = value;  RaisePropertyChanged("IsActive"); }
		}
		public RelayCommand cmdSaveToDB
		{
			get
			{
				return _cmdSaveToDB;
			}

			set
			{
				_cmdSaveToDB = value;
			}
		}
		public RelayCommand cmdStart
		{
			get
			{
				return _cmdStart;
			}

			set
			{
				_cmdStart = value;
			}
		}
		public RelayCommand cmdStop
		{
			get
			{
				return _cmdStop;
			}

			set
			{
				_cmdStop = value;
			}
		}
		public RelayCommand cmdUpdate
		{
			get
			{
				return _cmdUpdate;
			}

			set
			{
				_cmdUpdate = value;
			}
		}
		private void DoStop(object obj)
		{
			StartStop(false);
		}
		private void DoStart(object obj)
		{
			StartStop(true);
		}
		private void DoUpdate(object obj)
		{
			if (_model != null)
			{
				System.ComponentModel.BackgroundWorker bwDoUpdate = new System.ComponentModel.BackgroundWorker();

				bwDoUpdate.DoWork += (s, args) =>
				{
					OnUpdateToAllModelsIfAllSymbolsIsSelected();
					try
					{
						args.Result = RESTFulHelper.SetVariable<List<T>>(modelItems.ToList());
					}
					catch { }
				};
				bwDoUpdate.RunWorkerCompleted += (s, args) =>
				{
					try
					{
						bool bRes = (bool)args.Result;
						if (_dialogs != null && _dialogs.ContainsKey("popup"))
						{
							if (bRes)
								_dialogs["popup"]("Parameters for " + _selectedSymbol + " have been successfully updated.", "");
							else if (_dialogs != null && _dialogs.ContainsKey("error"))
								_dialogs["error"]("Looks like we are unable to get connected to the trading system.", "");
							GetParameters();
						}
					}
					catch { }
					this.UpdateButtonIsEnabled = true;
				};
				if (!bwDoUpdate.IsBusy)
				{
					this.UpdateButtonIsEnabled = false;
					bwDoUpdate.RunWorkerAsync();
				}

			}
		}
		private void DoSaveToDB(object obj)
		{
			if (_model != null)
			{
				System.ComponentModel.BackgroundWorker bwDoUpdate = new System.ComponentModel.BackgroundWorker();
				bwDoUpdate.DoWork += (s, args) =>
				{
					OnUpdateToAllModelsIfAllSymbolsIsSelected();
					OnSaveSettingsToDB();
					try
					{
						args.Result = RESTFulHelper.SetVariable<List<T>>(modelItems.ToList());
					}
					catch { }
				};
				bwDoUpdate.RunWorkerCompleted += (s, args) =>
				{
					if (args.Error == null)
					{
						if (_dialogs != null && _dialogs.ContainsKey("popup"))
						{
							_dialogs["popup"]("Parameters have been successfully saved.", "");
						}
					}
					else
					{
						if (_dialogs != null && _dialogs.ContainsKey("error"))
							_dialogs["error"]("Error trying to save parameters into the DB." + args.Error.ToString(), "");
					}
					this.SaveDBButtonIsEnabled = true;
				};
				if (!bwDoUpdate.IsBusy)
				{
					this.SaveDBButtonIsEnabled = false;
					bwDoUpdate.RunWorkerAsync();
				}
			}
		}
		public T Model
		{
			get { return _model; }
			set
			{
				if (!EqualityComparer<T>.Default.Equals(_model, value))
				{
					_model = value;
					RaisePropertyChanged("Model");
				}
			}
		}
		public bool UpdateButtonIsEnabled
		{
			get
			{
				return _updateButtonIsEnabled;
			}
			set
			{
				if (_updateButtonIsEnabled != value)
				{
					_updateButtonIsEnabled = value;
					RaisePropertyChanged("UpdateButtonIsEnabled");
				}
			}
		}
		public bool SaveDBButtonIsEnabled
		{
			get
			{
				return _saveDBButtonIsEnabled;
			}
			set
			{
				if (_saveDBButtonIsEnabled != value)
				{
					_saveDBButtonIsEnabled = value;
					RaisePropertyChanged("SaveDBButtonIsEnabled");
				}
			}
		}
		public string cmdStartImage
		{
			get
			{
				return _cmdStartImage;
			}

			set
			{
				if (_cmdStartImage != value)
				{
					_cmdStartImage = value;
					RaisePropertyChanged("cmdStartImage");
				}
			}
		}
		public string cmdStopImage
		{
			get
			{
				return _cmdStopImage;
			}

			set
			{
				if (_cmdStopImage != value)
				{
					_cmdStopImage = value;
					RaisePropertyChanged("cmdStopImage");
				}
			}
		}
		public bool cmdStartIsEnable
		{
			get
			{
				if (_model != null)
				{
					return !((IStrategyParameters)_model).IsStrategyOn;
				}
				return false;
			}
		}
		public bool cmdStopIsEnable
		{
			get
			{
				if (_model != null)
				{
					return ((IStrategyParameters)_model).IsStrategyOn;
				}
				return false;
			}

		}


		protected void UpdateStartStopBindings()
		{
			if (string.IsNullOrEmpty(_selectedStrategy))
			{
				this.cmdStartImage = "/Images/startD.png";
				this.cmdStopImage = "/Images/stopD.png";
			}
			else if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
			{
				this.cmdStartImage = "/Images/start.png";
				this.cmdStopImage = "/Images/stop.png";
			}
			else
			{
				if (_model != null && ((IStrategyParameters)_model).IsStrategyOn)
				{
					this.cmdStartImage = "/Images/startD.png";
					this.cmdStopImage = "/Images/stop.png";
				}
				else
				{
					this.cmdStartImage = "/Images/start.png";
					this.cmdStopImage = "/Images/stopD.png";
				}
			}

			RaisePropertyChanged("cmdStartIsEnable");
			RaisePropertyChanged("cmdStopIsEnable");
		}		
		protected bool IsThisMe(string selectedStrategy)
		{
			var isMe = selectedStrategy == _strategyNameForThisControl;
			this.IsActive = (isMe ? Visibility.Visible : Visibility.Hidden);
			return isMe;
		}
		private void StartStop(bool isStart)
		{
			if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
			{
				//STOP or START all
				foreach (var m in modelItems)
					((IStrategyParameters)m).IsStrategyOn = isStart;
			}
			else
				((IStrategyParameters)_model).IsStrategyOn = isStart;
			UpdateStartStopBindings();

			System.ComponentModel.BackgroundWorker bwDoStartStop = new System.ComponentModel.BackgroundWorker();
			bwDoStartStop.DoWork += (s, args) =>
			{
				try
				{
					OnSaveSettingsToDB();
					args.Result = RESTFulHelper.SetVariable<List<T>>(modelItems.ToList());
				}
				catch { }
			};
			bwDoStartStop.RunWorkerCompleted += (s, args) =>
			{
				try
				{
					bool bRes = (bool)args.Result;
					if (_dialogs != null && _dialogs.ContainsKey("popup"))
					{
						if (bRes)
						{
							if (isStart)
								_dialogs["popup"]("Strategy for " + _selectedSymbol + " has been started.", "");
							else
								_dialogs["popup"]("Strategy for " + _selectedSymbol + " has been stopped.", "");
						}
						else if (_dialogs != null && _dialogs.ContainsKey("error"))
							_dialogs["error"]("Looks like we are unable to get connected to the trading system.", "");
						GetParameters();
					}
				}
				catch { }
			};
			if (!bwDoStartStop.IsBusy)
				bwDoStartStop.RunWorkerAsync();
		}
		protected void GetParameters()
		{
			if (!HelperCommon.ACTIVESTRATEGIES.Contains(this._strategyNameForThisControl))
			{
				return;
			}
			if (!bwGetParameters.WorkerSupportsCancellation)
			{
				bwGetParameters.WorkerSupportsCancellation = true; //use it to know if it was already setup
				bwGetParameters.DoWork += (s, args) =>
				{
					try
					{
						var data = RESTFulHelper.GetVariable<List<T>>().Result;

						args.Result = data;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				};
				bwGetParameters.RunWorkerCompleted += (s, args) =>
				{
					var res = args.Result as List<T>;
					if (res == null)
						return;
					UpdateCurrentModel(res);
					UpdateStartStopBindings();
				};
			}
			if (!bwGetParameters.IsBusy)
				bwGetParameters.RunWorkerAsync();
		}
		protected void UpdateCurrentModel(List<T> updatedModels)
		{
			if (updatedModels == null || updatedModels.Count == 0)
				return;
			modelItems = updatedModels;
			var itemToUpdate = modelItems.Where(x => ((IStrategyParameters)x).Symbol == _selectedSymbol).FirstOrDefault();
			if (itemToUpdate != null)
				this.Model = itemToUpdate;
			else
				this.Model = default(T);

			if (this.Model == null)
			{
				this.SaveDBButtonIsEnabled = false;
				this.UpdateButtonIsEnabled = false;
			}
			else
			{
				this.SaveDBButtonIsEnabled = true;
				this.UpdateButtonIsEnabled = true;
			}
		}
		private void FindStrategyModelItemBySymbol(string layerName, string symbol)
		{
			if (!string.IsNullOrEmpty(layerName) && !string.IsNullOrEmpty(symbol) && modelItems != null)
			{
				string filterSymbol = (string.IsNullOrEmpty(_selectedSymbol) ? "" : (_selectedSymbol == "-- All symbols --" ? "" : _selectedSymbol));
				_model = modelItems.Where(x => ((IStrategyParameters)x).Symbol == filterSymbol && ((IStrategyParameters)x).LayerName == layerName).FirstOrDefault();
			}
		}
		private void SelectionChanged()
		{
			GetParameters();
			FindStrategyModelItemBySymbol(_layerName, _selectedSymbol);
			ReloadPositions(_selectedSymbol, _selectedStrategy);
		}

		private void STRATEGYPARAMS_OnDataUpdateReceived(object sender, string e)
		{
			try
			{
				Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings
				{
					Converters = new List<Newtonsoft.Json.JsonConverter> { new CustomDateConverter() },
					DateParseHandling = Newtonsoft.Json.DateParseHandling.None
				};
				var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(e, settings);
				if (data != null)
					UpdateCurrentModel(data);
			}
			catch (Exception ex)
			{ }
		}



		protected void ReloadPositions(string symbol, string strategyCode)
		{
			if (symbol == "-- All symbols --")
				symbol = "";
			var closedPositions = HelperCommon.CLOSEDPOSITIONS.Positions.Where(x => (string.IsNullOrEmpty(symbol) || x.Symbol == symbol) && (string.IsNullOrEmpty(strategyCode) || x.StrategyCode == strategyCode)).ToList();
			var openPositions = HelperCommon.OPENPOSITIONS.Positions.Where(x => (string.IsNullOrEmpty(symbol) || x.Symbol == symbol) && (string.IsNullOrEmpty(strategyCode) || x.StrategyCode == strategyCode));
			if (openPositions != null && openPositions.Any())
				closedPositions.AddRange(openPositions);
			_positions = new ObservableCollection<PositionEx>(closedPositions.OrderByDescending(x => x.CloseTimeStamp));
            _vmStrategyOverview.Positions = _positions;
			RaisePropertyChanged("Positions");
		}
		private void RecalculatePositionStatistics()
		{
			if (_positions != null && _positions.Any())
			{
				_VolumeTraded = HelperCommon.GetKiloFormatter(_positions.Sum(x => x.OrderQuantity.ToDouble() * 2));
				//EXECUTION LATENCIES
				var lastHour = _positions.Last().CreationTimeStamp.AddHours(-1); //to get hour from server
				var executionOpen = _positions.Where(x => x.CreationTimeStamp > lastHour && x.CloseTimeStamp > x.CreationTimeStamp && !x.IsOpenMM && x.OpenExecutions != null && x.OpenExecutions.Any()).Select(x => x.OpenExecutions.Where(s => s.Status == (int)ePOSITIONSTATUS.NEW || s.Status == (int)ePOSITIONSTATUS.FILLED).OrderBy(s => s.LocalTimeStamp)).Where(x => x.Count() > 1).ToList();
				var executionClose = _positions.Where(x => x.CreationTimeStamp > lastHour && x.CloseTimeStamp > x.CreationTimeStamp && !x.IsCloseMM && x.CloseExecutions != null && x.CloseExecutions.Any()).Select(x => x.CloseExecutions.Where(s => s.Status == (int)ePOSITIONSTATUS.NEW || s.Status == (int)ePOSITIONSTATUS.FILLED).OrderBy(s => s.LocalTimeStamp)).Where(x => x.Count() > 1).ToList();
				double open = 0;
				double close = 0;
				if (executionOpen.Any())
					open = executionOpen.Average(x => x.Last().ServerTimeStamp.Subtract(x.First().ServerTimeStamp).TotalMilliseconds);
				if (executionClose.Any())
					close = executionClose.Average(x => x.Last().ServerTimeStamp.Subtract(x.First().ServerTimeStamp).TotalMilliseconds);
				_ExecutionLatencyLastHour = HelperCommon.GetKiloFormatterTime((open + close) / 2);

				RaisePropertyChanged("VolumeTraded");
				RaisePropertyChanged("ExecutionLatencyLastHour");
				RaisePropertyChanged("OrderFillsBuys");
				RaisePropertyChanged("OrderFillsSells");
				RaisePropertyChanged("OrderFillsDiff");
			}
		}
		private void Positions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				for (int i =0; i < e.NewItems.Count; i++)
				{
					PositionEx pos = e.NewItems[i] as PositionEx;
					if ((string.IsNullOrEmpty(_selectedSymbol) || pos.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || pos.StrategyCode == _selectedStrategy))
					{
						if (_positions == null)
							_positions = new ObservableCollection<PositionEx>();
						_positions.Add(pos);
					}
				}
				RecalculatePositionStatistics();
				RaisePropertyChanged("Positions");
			}
		}
		public ObservableCollection<PositionEx> Positions
		{
			get
			{
				return _positions;
			}

			set
			{
				_positions = value;
				RaisePropertyChanged("Positions");
			}
		}
		public string OrderFillsBuys
		{
			get
			{
				if (_positions == null || !_positions.Any())
					return "";
				var allBuys = _positions.SelectMany(x => x.AllExecutions).Where(x => ((eORDERSTATUS)x.Status == eORDERSTATUS.FILLED || (eORDERSTATUS)x.Status == eORDERSTATUS.PARTIALFILLED) && (eORDERSIDE)x.Side == eORDERSIDE.Buy);
				if (allBuys != null && allBuys.Any())
				{
					var _value = (double)allBuys.Sum(x => x.Price.Value * x.QtyFilled.Value);
					return HelperCommon.GetKiloFormatter(allBuys.Sum(x => x.QtyFilled.Value)) + "x" + HelperCommon.GetKiloFormatter(_value);
				}
				return "";
			}
		}
		public string OrderFillsSells
		{
			get
			{
				if (_positions == null || !_positions.Any())
					return "";
				var allSells = _positions.SelectMany(x => x.AllExecutions).Where(x => ((eORDERSTATUS)x.Status == eORDERSTATUS.FILLED || (eORDERSTATUS)x.Status == eORDERSTATUS.PARTIALFILLED) && (eORDERSIDE)x.Side == eORDERSIDE.Sell);
				if (allSells != null && allSells.Any())
				{
					var _value = (double)allSells.Sum(x => x.Price.Value * x.QtyFilled.Value);
					return HelperCommon.GetKiloFormatter(allSells.Sum(x => x.QtyFilled.Value)) + "x" + HelperCommon.GetKiloFormatter(_value);
				}
				return "";
			}
		}
		public string OrderFillsDiff
		{
			get
			{
				if (_positions == null || !_positions.Any())
					return "";
				var allSells = _positions.SelectMany(x => x.AllExecutions).Where(x => ((eORDERSTATUS)x.Status == eORDERSTATUS.FILLED || (eORDERSTATUS)x.Status == eORDERSTATUS.PARTIALFILLED) && (eORDERSIDE)x.Side == eORDERSIDE.Sell);
				var allBuys = _positions.SelectMany(x => x.AllExecutions).Where(x => ((eORDERSTATUS)x.Status == eORDERSTATUS.FILLED || (eORDERSTATUS)x.Status == eORDERSTATUS.PARTIALFILLED) && (eORDERSIDE)x.Side == eORDERSIDE.Buy);
				if (allBuys != null && allBuys.Any() && allSells != null && allSells.Any())
				{
					var _valueBuy = (double)allBuys.Sum(x => x.Price.Value * x.QtyFilled.Value);
					var _valueSell = (double)allSells.Sum(x => x.Price.Value * x.QtyFilled.Value);
					var _diff = _valueSell - _valueBuy;
					return (_diff < 0 ? "-" : "") + HelperCommon.GetKiloFormatter(Math.Abs(_diff));
				}
				return "";
			}
		}
		public string VolumeTraded
		{
			get
			{
				return _VolumeTraded;
			}
		}
		public string ExecutionLatencyLastHour
		{
			get
			{
				return _ExecutionLatencyLastHour;
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
                    if (IsThisMe(_selectedStrategy))
					{						
						RaisePropertyChanged("SelectedSymbol");
						SelectionChanged();
					}
				}
			}
		}
		public string SelectedStrategy
		{
			get { return _selectedStrategy; }
			set
			{
				if (_selectedStrategy != value)
				{
                    _selectedStrategy = value;
                    if (IsThisMe(_selectedStrategy))
					{
						Load();
                        RaisePropertyChanged("SelectedStrategy");
                        RaisePropertyChanged("IsActive");
                        SelectionChanged();
					}
				}
			}
		}
		public string SelectedLayer
		{
			get { return _layerName; }
			set
			{
				if (_layerName != value)
				{
					if (IsThisMe(_selectedStrategy))
					{
						_layerName = value;
						RaisePropertyChanged("SelectedLayer");
						SelectionChanged();
					}
				}
			}
		}

        protected ucStrategyOverview UcStrategyOverview 
		{ 
			get => ucStrategyOverview; 
			set
			{
                ucStrategyOverview = value; 
				RaisePropertyChanged("UcStrategyOverview");
            }				
		}
        
    }




}
