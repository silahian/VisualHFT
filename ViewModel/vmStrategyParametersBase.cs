using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.ViewModel.StatisticsViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

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
        protected string _AckLatencyLastHour;
		protected string _OrderFillsBuys;
        protected string _OrderFillsSells;
		protected string _OrderFillsDiff;
        protected T _model;
		protected List<T> modelItems;

		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public vmStrategyParametersBase(Dictionary<string, Func<string, string, bool>> dialogs)
		{			

            _dialogs = dialogs;            
			_vmStrategyOverview = new vmStrategyOverview(Helpers.HelperCommon.GLOBAL_DIALOGS);
            RaisePropertyChanged("vmStrategyOverview");



            this.IsActive = Visibility.Hidden;
            cmdStop = new RelayCommand(DoStop);			
			cmdStart = new RelayCommand(DoStart);
			cmdUpdate = new RelayCommand(DoUpdate);
			cmdSaveToDB = new RelayCommand(DoSaveToDB);

			_positions = new ObservableCollection<PositionEx>();
			RaisePropertyChanged("Positions");
        }
		public virtual void Load()
		{
			GetParameters();
			HelperCommon.CLOSEDPOSITIONS.OnInitialLoad += CLOSEDPOSITIONS_OnInitialLoad;
            HelperCommon.CLOSEDPOSITIONS.OnDataReceived += CLOSEDPOSITIONS_OnDataReceived;
            HelperCommon.STRATEGYPARAMS.OnDataUpdateReceived += STRATEGYPARAMS_OnDataUpdateReceived;
		}

		public virtual void Unload()
		{
            HelperCommon.CLOSEDPOSITIONS.OnDataReceived -= CLOSEDPOSITIONS_OnDataReceived;
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
			ReloadPositions();
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



		protected void ReloadPositions()
		{
			if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --" || string.IsNullOrEmpty(_selectedStrategy))
			{
				_positions.Clear();
				_vmStrategyOverview.ClearPositions();
                return;
			}

			var closedPositions = HelperCommon.CLOSEDPOSITIONS.Positions.Where(x => x.Symbol == _selectedSymbol && x.StrategyCode == _selectedStrategy).ToList();
			_positions = new ObservableCollection<PositionEx>(closedPositions.OrderByDescending(x => x.CloseTimeStamp));
            _vmStrategyOverview.AddNewPositions(_positions);

            RecalculatePositionStatistics();
            RaisePropertyChanged("Positions");
		}
        private void CLOSEDPOSITIONS_OnInitialLoad(object sender, IEnumerable<PositionEx> e)
        {
			_positions = new ObservableCollection<PositionEx>(e);
            RaisePropertyChanged("Positions");
            RecalculatePositionStatistics();
        }
        private void CLOSEDPOSITIONS_OnDataReceived(object sender, IEnumerable<PositionEx> e)
		{
			foreach (var pos in e)
			{
                if ((string.IsNullOrEmpty(_selectedSymbol) || pos.Symbol == _selectedSymbol) && (string.IsNullOrEmpty(_selectedStrategy) || pos.StrategyCode == _selectedStrategy))
                {
                    _positions.Add(pos);
                    _vmStrategyOverview.AddNewPosition(pos);

                }
            }
            RecalculatePositionStatistics();

		}
        private void RecalculatePositionStatistics()
        {
			if (_positions != null && _positions.Any())
			{
                List<double> ack_latency = new List<double>();
                List<double> fill_latency = new List<double>();
                List<ExecutionVM> allBuys = new List<ExecutionVM>();
                List<ExecutionVM> allSells = new List<ExecutionVM>();

                var lastHour = _positions.Last().CreationTimeStamp.AddHours(-1); //to get hour from server
                var allPositions = _positions.Where(x => x.CreationTimeStamp > lastHour && x.CloseTimeStamp > x.CreationTimeStamp);//.SelectMany(x => x.AllExecutions)
				allBuys = _positions.SelectMany(x => x.AllExecutions).Where(x => x.Side == ePOSITIONSIDE.Buy && (x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED)).ToList();
                allSells = _positions.SelectMany(x => x.AllExecutions).Where(x => x.Side == ePOSITIONSIDE.Sell && (x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED)).ToList();

                #region Process Latencies
                foreach (var pos in allPositions)
				{
					var allExecutions = pos.AllExecutions.OrderBy(x => x.LocalTimeStamp);
					ExecutionVM _new_sent = null;
                    ExecutionVM _cancel_sent = null;
                    ExecutionVM _replace_sent = null;
                    ExecutionVM _new_ack = null;
                    ExecutionVM _replace_ack = null;
                    ExecutionVM _cancel_ack = null;
                    ExecutionVM _filled = null;

					foreach (var execution in allExecutions)
					{
                        if (execution.Status == ePOSITIONSTATUS.SENT)
							_new_sent = execution;
                        if (execution.Status == ePOSITIONSTATUS.REPLACESENT)
                            _replace_sent = execution;
                        if (execution.Status == ePOSITIONSTATUS.CANCELEDSENT)
                            _cancel_sent = execution;


                        if (execution.Status == ePOSITIONSTATUS.NEW)
							_new_ack = execution;
						if (execution.Status == ePOSITIONSTATUS.REPLACED || (_replace_sent != null && execution.Status == ePOSITIONSTATUS.REJECTED))
                            _replace_ack = execution;
                        if (execution.Status == ePOSITIONSTATUS.CANCELED || (_cancel_sent != null && execution.Status == ePOSITIONSTATUS.REJECTED))
                            _cancel_ack = execution;

						if (execution.Status == ePOSITIONSTATUS.FILLED || execution.Status == ePOSITIONSTATUS.PARTIALFILLED)
							_filled = execution;

                        //TODO: all latencies are measured in milliseconds (should we meassure them in microseconds instead?)
                        if (_new_sent != null && _new_ack != null) //process new/ack latency = how much time the exchange take to ack the new order sent
						{
							var _val = _new_ack.LocalTimeStamp.Subtract(_new_sent.LocalTimeStamp).TotalMilliseconds;
							if (_val > 0)
								ack_latency.Add(_val);
							_new_sent = null;
							//_new_ack = null;
                        }
						if (_cancel_sent != null && _cancel_ack != null) //process cancel/ack latency = how much time the exchange take to ack the order cancel request.
						{
							var _val = _cancel_ack.LocalTimeStamp.Subtract(_cancel_sent.LocalTimeStamp).TotalMilliseconds;
							if (_val > 0)
								ack_latency.Add(_val);
                            _cancel_sent = null;
							_cancel_ack = null;
                        }
                        if (_replace_sent != null && _replace_ack != null) //process replace/ack latency = how much time the exchange take to ack the order replace request.
                        {
							var _val = _replace_ack.LocalTimeStamp.Subtract(_replace_sent.LocalTimeStamp).TotalMilliseconds;
							if (_val > 0)
								ack_latency.Add(_val);
                            _replace_sent = null;
                            //_replace_ack = null;
                        }
						if ((_new_ack != null || _replace_ack != null) && _filled != null)
						{
							var ini_msg = (_new_ack != null ? _new_ack : _replace_ack);
							var _val = _filled.LocalTimeStamp.Subtract(ini_msg.LocalTimeStamp).TotalMilliseconds;
							if (_val > 0)
								fill_latency.Add(_val);
							if (_new_ack != null) _new_ack = null; else _replace_ack = null;
							_filled = null;
                        }
                    }

                }
                #endregion
                if (ack_latency.Any())
					_AckLatencyLastHour = HelperCommon.GetKiloFormatterTime(ack_latency.Sum() / ack_latency.Count());
				if (fill_latency.Any())
					_ExecutionLatencyLastHour = HelperCommon.GetKiloFormatterTime(fill_latency.Sum() / fill_latency.Count());

				decimal _volumeDiff = 0;
				if (allBuys.Any())
				{
					var totSize = allBuys.Sum(x => x.QtyFilled.Value);
					var totVol = allBuys.Sum(x => x.QtyFilled.Value * x.Price.Value);
					_volumeDiff += totVol;
                    _OrderFillsBuys = HelperCommon.GetKiloFormatter(totSize) + " x " + HelperCommon.GetKiloFormatter(totVol);
				}
				else
					_OrderFillsBuys = "";

                if (allSells.Any())
				{
					var totSize = allSells.Sum(x => x.QtyFilled.Value);
					var totVol = allSells.Sum(x => x.QtyFilled.Value * x.Price.Value);
                    _volumeDiff -= totVol;
                    _OrderFillsSells = HelperCommon.GetKiloFormatter(totSize) + " x " + HelperCommon.GetKiloFormatter(totVol);
				}
				else
                    _OrderFillsSells = "";
				if (_volumeDiff != 0)
					_OrderFillsDiff = (_volumeDiff < 0 ? "-" : "") + HelperCommon.GetKiloFormatter(Math.Abs(_volumeDiff));
				else
					_OrderFillsDiff = "";

                _VolumeTraded = HelperCommon.GetKiloFormatter(_positions.Sum(x => (x.GetOpenAvgPrice.ToDouble() * x.GetOpenQuantity.ToDouble()) + (x.GetCloseAvgPrice.ToDouble() * x.GetCloseQuantity.ToDouble())));
                

                RaisePropertyChanged("VolumeTraded");
                RaisePropertyChanged("ExecutionLatencyLastHour");
                RaisePropertyChanged("AckLatencyLastHour");
                RaisePropertyChanged("OrderFillsBuys");
                RaisePropertyChanged("OrderFillsSells");
                RaisePropertyChanged("OrderFillsDiff");
            }
        }

        public string OrderFillsBuys
		{
			get { return _OrderFillsBuys; }
		}
		public string OrderFillsSells
		{
			get { return _OrderFillsSells; }
		}
		public string OrderFillsDiff
		{
			get { return _OrderFillsDiff; }
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
        public string AckLatencyLastHour
        {
            get
            {
                return _AckLatencyLastHour;
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



        public vmStrategyOverview vmStrategyOverview
		{
			get => _vmStrategyOverview;
			set
			{
				_vmStrategyOverview = value;
				RaisePropertyChanged();

            }
		}

	}




}
