using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using VisualHFT.DataRetriever;
using System.Threading.Tasks;

namespace VisualHFT.ViewModel
{
    public class vmProvider : BindableBase
    {
        private VisualHFT.ViewModel.Model.Provider _selectedItem;
        private ObservableCollection<VisualHFT.ViewModel.Model.Provider> _providers;
        private ICommand _cmdUpdateStatus;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private DateTime? _lastHeartBeatReceived = null;
        private eSESSIONSTATUS _status;
        object _lock = new object();

        public vmProvider(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            _cmdUpdateStatus = new RelayCommand<object>(DoUpdateStatus);

            _providers = new ObservableCollection<VisualHFT.ViewModel.Model.Provider>();
            RaisePropertyChanged(nameof(Providers));

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.PROVIDERS.OnHeartBeatFail += PROVIDERS_OnHeartBeatFail;
        }

        private void PROVIDERS_OnDataReceived(object sender, VisualHFT.ViewModel.Model.Provider e)
        {
            if (e == null || e.ProviderCode == -1)
                return;

            var existingProv = _providers.Where(x => x.ProviderCode == e.ProviderCode).FirstOrDefault();
            if (existingProv != null)
            {
                _status = e.Status;
                _lastHeartBeatReceived = e.LastUpdated;
            }
            else
            {
                //needs to be added in UI thread
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    lock (_lock)
                        _providers.Add(e);
                }));
            }
        }
        private void PROVIDERS_OnHeartBeatFail(object sender, VisualHFT.ViewModel.Model.Provider e)
        {
            var itemToUpdate = _providers.Where(x => x.ProviderCode == e.ProviderCode).FirstOrDefault();
            if (itemToUpdate != null)
            {
                itemToUpdate.LastUpdated = e.LastUpdated;
                itemToUpdate.Status = e.Status;
                itemToUpdate.CheckValuesUponHeartbeatReceived();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    lock (_lock)
                    {
                        if (!_providers.Any(x => x.ProviderCode == e.ProviderCode))
                        {
                            _providers.Add(e);
                            e.CheckValuesUponHeartbeatReceived();
                        }
                    }
                }));
            }
        }

        private void DoUpdateStatus(object obj)
        {
            _selectedItem = obj as VisualHFT.ViewModel.Model.Provider;
            if (_selectedItem != null)
            {
                eSESSIONSTATUS statusToSend;
                if (_selectedItem.Status != eSESSIONSTATUS.BOTH_DISCONNECTED)
                    statusToSend = eSESSIONSTATUS.BOTH_DISCONNECTED;
                else
                    statusToSend = eSESSIONSTATUS.BOTH_CONNECTED;
                string msg = "Are you sure want to" + (statusToSend == eSESSIONSTATUS.BOTH_CONNECTED ? " connect " : " disconnect ") + "'" + _selectedItem.ProviderName + "' ?";
                if (_dialogs.ContainsKey("confirm") && _dialogs["confirm"](msg, "Updating..."))
                {
                    var _linkToPlugIn = _selectedItem.Plugin as IDataRetriever;
                    if (_linkToPlugIn != null)
                    {
                        Task.Run(() =>
                        {
                            if (statusToSend == eSESSIONSTATUS.BOTH_CONNECTED)
                                _linkToPlugIn.Start();
                            else
                                _linkToPlugIn.Stop();
                        });



                    }
                }
            }
        }
        public ObservableCollection<VisualHFT.ViewModel.Model.Provider> Providers
        {
            get => _providers;
            set => SetProperty(ref _providers, value);
        }

        public ICommand CmdUpdateStatus
        {
            get => _cmdUpdateStatus;
            set => SetProperty(ref _cmdUpdateStatus, value);
        }

        public VisualHFT.ViewModel.Model.Provider SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }


    }
}
