using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.ViewModel
{
    public class vmProvider : INotifyPropertyChanged
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        public event PropertyChangedEventHandler PropertyChanged;
        private Provider _selectedItem;
        private ObservableCollection<Provider> _providers;
        private RelayCommand _cmdUpdateStatus;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        System.ComponentModel.BackgroundWorker bwGetProviders = new System.ComponentModel.BackgroundWorker();

        public vmProvider(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            _providers = new ObservableCollection<Provider>();
            _cmdUpdateStatus = new RelayCommand(DoUpdateStatus);

            //GetProviders();

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.IsEnabled = false;
            //GetProviders();
            dispatcherTimer.IsEnabled = true;
        }

        private void GetProviders()
        {
            if (!bwGetProviders.WorkerSupportsCancellation)
            {
                bwGetProviders.WorkerSupportsCancellation = true; //use it to know if it was already setup

                bwGetProviders.DoWork += (s, args) =>
            {
                try
                {
                    dispatcherTimer.IsEnabled = false; //avoid multiple calls while executing
					var task = Task.Run<List<Provider>>(async () => await RESTFulHelper.GetVariable<List<Provider>>());
					task.RunSynchronously();
					args.Result = task.Result;
				}
                catch (Exception ex)
                {                    
                    foreach (var p in _providers)
                    {
                        p.Status = eSESSIONSTATUS.BOTH_DISCONNECTED;
                    }
                    dispatcherTimer.IsEnabled = true;
                }
            };
                bwGetProviders.RunWorkerCompleted += (s, args) =>
                {
                    var fromAPP = args.Result as List<Provider>;
                    if (fromAPP != null)
                    {
                        if (_providers == null || _providers.Count == 0)
                            _providers = new ObservableCollection<Provider>(fromAPP);
                        else
                        {
                            foreach (var p in fromAPP) //update status
                            {
                                var provToUpdate = _providers.Where(x => x.ProviderID == p.ProviderID).FirstOrDefault();
                                if (provToUpdate != null)
                                {
                                    provToUpdate.Status = p.Status;
                                }
                            }
                        }
                        RaisePropertyChanged("Providers");
                    }
                    else if (fromAPP == null && _providers != null && _providers.Count > 0) //NOT GETTING INFORMATION: because is disconnected
                    {
                        foreach (var p in _providers) //update status
                        {
                            p.Status = eSESSIONSTATUS.BOTH_DISCONNECTED;
                        }
                        RaisePropertyChanged("Providers");
                    }
                    dispatcherTimer.IsEnabled = true; //avoid multiple calls while executing
                };
            }
            if (!bwGetProviders.IsBusy)
                bwGetProviders.RunWorkerAsync();
        }

        private void DoUpdateStatus(object obj)
        {
            _selectedItem = obj as Provider;
            if (_selectedItem != null)
            {
                eSESSIONSTATUS statusToSend;
                if (_selectedItem.Status != eSESSIONSTATUS.BOTH_DISCONNECTED)
                    statusToSend = eSESSIONSTATUS.BOTH_DISCONNECTED;
                else
                    statusToSend = eSESSIONSTATUS.BOTH_CONNECTED;
                string msg = "Are you sure want to " + (statusToSend == eSESSIONSTATUS.BOTH_CONNECTED ? " connect " : " disconnect ") + "'" + _selectedItem.ProviderName + "' ?";
                if (_dialogs.ContainsKey("confirm") && _dialogs["confirm"](msg, "Updating..."))
                {
                    var bw = new System.ComponentModel.BackgroundWorker();
                    bw.DoWork += (s, args) =>
                    {
                        try
                        {
                            _selectedItem.Status = statusToSend;
                            args.Result = RESTFulHelper.SetVariable<List<Provider>>(_providers.ToList());
                        }
                        catch { /*System.Threading.Thread.Sleep(5000);*/ }
                    };
                    bw.RunWorkerCompleted += (s, args) =>
                    {
                        try
                        {
                            bool bRes = (bool)args.Result;
                            if (_dialogs != null && _dialogs.ContainsKey("popup"))
                            {
                                if (bRes)
                                {
                                    _dialogs["popup"]("Status has been updated.", "");
                                }
                                else if (_dialogs != null && _dialogs.ContainsKey("error"))
                                    _dialogs["error"]("Looks like we are unable to get connected to the trading system.", "");
                                //GetProviders();
                            }
                        }
                        catch { }
                    };
                    if (!bw.IsBusy)
                        bw.RunWorkerAsync();
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
        public ObservableCollection<Provider> Providers
        {
            get
            {
                return _providers;
            }
            set
            {
				if (_providers != value)
				{
					_providers = value;
					RaisePropertyChanged("Providers");
				}
            }
        }

        public RelayCommand CmdUpdateStatus
        {
            get
            {
                return _cmdUpdateStatus;
            }

            set
            {
				if (_cmdUpdateStatus != value)
				{
					_cmdUpdateStatus = value;
				}
            }
        }

        public Provider SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
				if (_selectedItem != value)
				{
					_selectedItem = value;
				}
            }
        }

    }
}
