using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.ViewModel.StatisticsViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Prism.Mvvm;

namespace VisualHFT.ViewModel
{
    public class vmStrategyParametersBase<T> : BindableBase where T : INotifyPropertyChanged
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
        protected ObservableCollection<VisualHFT.Model.Position> _positions;
        protected Dictionary<string, Func<string, string, bool>> _dialogs;
        protected BackgroundWorker bwGetParameters = new BackgroundWorker();
        protected BackgroundWorker bwSetParameters = new BackgroundWorker();
        protected string _VolumeTraded;
        protected string _ExecutionLatencyLastHour;
        protected string _AckLatencyLastHour;
        protected string _OrderFillsBuys;
        protected string _OrderFillsSells;
        protected string _OrderFillsDiff;
        protected T _model;
        protected List<T> modelItems;
        public vmStrategyParametersBase(Dictionary<string, Func<string, string, bool>> dialogs)
        {

            _dialogs = dialogs;
            _vmStrategyOverview = new vmStrategyOverview(Helpers.HelperCommon.GLOBAL_DIALOGS);
            RaisePropertyChanged(nameof(vmStrategyOverview));



            this.IsActive = Visibility.Hidden;
            cmdStop = new RelayCommand<object>((args) => StartStop(false));
            cmdStart = new RelayCommand<object>((args) => StartStop(true));
            cmdUpdate = new RelayCommand<object>(DoUpdate);
            cmdSaveToDB = new RelayCommand<object>(DoSaveToDB);

            _positions = new ObservableCollection<VisualHFT.Model.Position>();
            RaisePropertyChanged(nameof(Positions));
        }
        public virtual void Load()
        {
            GetParameters();
            //HelperCommon.CLOSEDPOSITIONS.OnInitialLoad += CLOSEDPOSITIONS_OnInitialLoad;
            //HelperCommon.CLOSEDPOSITIONS.OnDataReceived += CLOSEDPOSITIONS_OnDataReceived;
            HelperCommon.STRATEGYPARAMS.OnDataUpdateReceived += STRATEGYPARAMS_OnDataUpdateReceived;
        }

        public virtual void Unload()
        {
            //HelperCommon.CLOSEDPOSITIONS.OnDataReceived -= CLOSEDPOSITIONS_OnDataReceived;
            HelperCommon.STRATEGYPARAMS.OnDataUpdateReceived -= STRATEGYPARAMS_OnDataUpdateReceived;
        }

        public virtual void OnUpdateToAllModelsIfAllSymbolsIsSelected() => throw new NotImplementedException();
        public virtual void OnSaveSettingsToDB() { }

        public Visibility IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
        public RelayCommand<object> cmdSaveToDB { get; set; }
        public RelayCommand<object> cmdStart { get; set; }
        public RelayCommand<object> cmdStop { get; set; }
        public RelayCommand<object> cmdUpdate { get; set; }
        public ObservableCollection<VisualHFT.Model.Position> Positions
        {
            get => _positions;
            set => SetProperty(ref _positions, value);
        }

        private void DoUpdate(object obj)
        {
            if (_model != null)
            {
                BackgroundWorker bwDoUpdate = new BackgroundWorker();

                bwDoUpdate.DoWork += (s, args) =>
                {
                    OnUpdateToAllModelsIfAllSymbolsIsSelected();
                    try
                    {
                        args.Result = RESTFulHelper.SetVariable(modelItems.ToList());
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
                BackgroundWorker bwDoUpdate = new BackgroundWorker();
                bwDoUpdate.DoWork += (s, args) =>
                {
                    OnUpdateToAllModelsIfAllSymbolsIsSelected();
                    OnSaveSettingsToDB();
                    try
                    {
                        args.Result = RESTFulHelper.SetVariable(modelItems.ToList());
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
            get => _model;
            set => SetProperty(ref _model, value);
        }
        public bool UpdateButtonIsEnabled
        {
            get => _updateButtonIsEnabled;
            set => SetProperty(ref _updateButtonIsEnabled, value);
        }
        public bool SaveDBButtonIsEnabled
        {
            get => _saveDBButtonIsEnabled;
            set => SetProperty(ref _saveDBButtonIsEnabled, value);
        }
        public string cmdStartImage
        {
            get => _cmdStartImage;
            set => SetProperty(ref _cmdStartImage, value);
        }
        public string cmdStopImage
        {
            get => _cmdStopImage;
            set => SetProperty(ref _cmdStopImage, value);
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

            RaisePropertyChanged(nameof(cmdStartIsEnable));
            RaisePropertyChanged(nameof(cmdStopIsEnable));
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

            BackgroundWorker bwDoStartStop = new BackgroundWorker();
            bwDoStartStop.DoWork += (s, args) =>
            {
                try
                {
                    OnSaveSettingsToDB();
                    args.Result = RESTFulHelper.SetVariable(modelItems.ToList());
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

            var closedPositions = new List<VisualHFT.Model.Position>(); //HelperCommon.CLOSEDPOSITIONS.Positions.Where(x => x.Symbol == _selectedSymbol && x.StrategyCode == _selectedStrategy).ToList();
            _positions = new ObservableCollection<VisualHFT.Model.Position>(closedPositions.OrderByDescending(x => x.CloseTimeStamp));
            _vmStrategyOverview.AddNewPositions(_positions);

            RecalculatePositionStatistics();
            RaisePropertyChanged(nameof(Positions));
        }
        private void CLOSEDPOSITIONS_OnInitialLoad(object sender, IEnumerable<VisualHFT.Model.Position> e)
        {
            _positions = new ObservableCollection<VisualHFT.Model.Position>(e);
            RaisePropertyChanged(nameof(Positions));
            RecalculatePositionStatistics();
        }
        private void CLOSEDPOSITIONS_OnDataReceived(object sender, IEnumerable<VisualHFT.Model.Position> e)
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
                List<Execution> allBuys = new List<Execution>();
                List<Execution> allSells = new List<Execution>();

                var lastHour = _positions.Last().CreationTimeStamp.AddHours(-1); //to get hour from server
                var allPositions = _positions.Where(x => x.CreationTimeStamp > lastHour && x.CloseTimeStamp > x.CreationTimeStamp);//.SelectMany(x => x.AllExecutions)
                allBuys = _positions.SelectMany(x => x.AllExecutions).Where(x => x.Side == ePOSITIONSIDE.Buy && (x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED)).ToList();
                allSells = _positions.SelectMany(x => x.AllExecutions).Where(x => x.Side == ePOSITIONSIDE.Sell && (x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED)).ToList();

                #region Process Latencies
                foreach (var pos in allPositions)
                {
                    var allExecutions = pos.AllExecutions.OrderBy(x => x.LocalTimeStamp);
                    Execution _new_sent = null;
                    Execution _cancel_sent = null;
                    Execution _replace_sent = null;
                    Execution _new_ack = null;
                    Execution _replace_ack = null;
                    Execution _cancel_ack = null;
                    Execution _filled = null;

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


                RaisePropertyChanged(nameof(VolumeTraded));
                RaisePropertyChanged(nameof(ExecutionLatencyLastHour));
                RaisePropertyChanged(nameof(AckLatencyLastHour));
                RaisePropertyChanged(nameof(OrderFillsBuys));
                RaisePropertyChanged(nameof(OrderFillsSells));
                RaisePropertyChanged(nameof(OrderFillsDiff));
            }
        }

        public string OrderFillsBuys => _OrderFillsBuys;
        public string OrderFillsSells => _OrderFillsSells;
        public string OrderFillsDiff => _OrderFillsDiff;
        public string VolumeTraded  => _VolumeTraded;
        public string ExecutionLatencyLastHour => _ExecutionLatencyLastHour;
        public string AckLatencyLastHour => _AckLatencyLastHour;

        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    if (IsThisMe(_selectedStrategy))
                    {
                        RaisePropertyChanged(nameof(SelectedSymbol));
                        SelectionChanged();
                    }
                }
            }
        }
        public string SelectedStrategy
        {
            get => _selectedStrategy;
            set
            {
                if (_selectedStrategy != value)
                {
                    _selectedStrategy = value;
                    if (IsThisMe(_selectedStrategy))
                    {
                        Load();
                        RaisePropertyChanged(nameof(SelectedStrategy));
                        RaisePropertyChanged(nameof(IsActive));
                        SelectionChanged();
                    }
                }
            }
        }
        public string SelectedLayer
        {
            get => _layerName;
            set
            {
                if (_layerName != value)
                {
                    if (IsThisMe(_selectedStrategy))
                    {
                        _layerName = value;
                        RaisePropertyChanged(nameof(SelectedLayer));
                        SelectionChanged();
                    }
                }
            }
        }

        public vmStrategyOverview vmStrategyOverview
        {
            get => _vmStrategyOverview;
            set => SetProperty(ref _vmStrategyOverview, value);
        }

    }




}
