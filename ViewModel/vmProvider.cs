using VisualHFT.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using VisualHFT.DataRetriever;
using System.Threading.Tasks;

namespace VisualHFT.ViewModel
{
    public class vmProvider : BindableBase
    {
        //private VisualHFT.Model.Provider _selectedItem;
        private ObservableCollection<ViewModel.Model.Provider> _providers;
        
        private ICommand _cmdUpdateStatus;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private DateTime? _lastHeartBeatReceived = null;
        private eSESSIONSTATUS _status;
        object _lock = new object();

        public vmProvider(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            _cmdUpdateStatus = new RelayCommand<object>(DoUpdateStatus);

            _providers = VisualHFT.ViewModel.Model.Provider.CreateObservableCollection();

            HelperProvider.Instance.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperProvider.Instance.OnHeartBeatFail += PROVIDERS_OnHeartBeatFail;

            RaisePropertyChanged(nameof(Providers));
        }

        private void PROVIDERS_OnDataReceived(object? sender, VisualHFT.Model.Provider e)
        {
            if (e == null || e.ProviderCode == -1)
                return;

            var existingProv = _providers.Where(x => x.ProviderCode == e.ProviderCode).FirstOrDefault();
            if (existingProv != null)
            {
                _status = e.Status;
                _lastHeartBeatReceived = e.LastUpdated;
                existingProv.Status = e.Status;
                existingProv.LastUpdated = e.LastUpdated;
                existingProv.UpdateUI();
            }
            else
            {
                //needs to be added in UI thread
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    lock (_lock)
                    {
                        if (!_providers.Any(x => x.ProviderCode == e.ProviderCode))
                        {
                            _providers.Add(new Model.Provider(e));
                        }
                    }
                }));
            }
        }
        private void PROVIDERS_OnHeartBeatFail(object? sender, VisualHFT.Model.Provider e)
        {
            var itemToUpdate = _providers.Where(x => x.ProviderCode == e.ProviderCode).FirstOrDefault();
            if (itemToUpdate != null)
            {
                itemToUpdate.LastUpdated = e.LastUpdated;
                itemToUpdate.Status = e.Status;
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    lock (_lock)
                    {
                        if (!_providers.Any(x => x.ProviderCode == e.ProviderCode))
                        {
                            _providers.Add(new Model.Provider(e));
                        }
                    }
                }));
            }
        }

        private void DoUpdateStatus(object obj)
        {
            var _selectedItem = obj as VisualHFT.ViewModel.Model.Provider;
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
                                _linkToPlugIn.StartAsync();
                            else
                                _linkToPlugIn.StopAsync();
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

        /*public VisualHFT.ViewModel.Model.Provider SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }*/


    }
}
