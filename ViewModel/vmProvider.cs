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
using VisualHFT.Enums;

namespace VisualHFT.ViewModel
{
    public class vmProvider : BindableBase
    {
        //private VisualHFT.Model.Provider _selectedItem;
        private ObservableCollection<ViewModel.Model.Provider> _providers;

        private ICommand _cmdUpdateStatus;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        object _lock = new object();

        public vmProvider(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            _cmdUpdateStatus = new RelayCommand<object>(DoUpdateStatus);

            _providers = VisualHFT.ViewModel.Model.Provider.CreateObservableCollection();

            HelperProvider.Instance.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperProvider.Instance.OnStatusChanged += PROVIDERS_OnStatusChanged;

            RaisePropertyChanged(nameof(Providers));
        }

        private void PROVIDERS_OnDataReceived(object? sender, VisualHFT.Model.Provider e)
        {
            if (e == null || e.ProviderCode == -1)
                return;

            if (_providers.All(x => x.ProviderCode != e.ProviderCode))
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
        private void PROVIDERS_OnStatusChanged(object? sender, VisualHFT.Model.Provider e)
        {
            var existingItem = _providers.FirstOrDefault(x => x.ProviderCode == e.ProviderCode);
            if (existingItem != null && existingItem.Status != e.Status)
            {
                existingItem.LastUpdated = e.LastUpdated;
                existingItem.Status = e.Status;
                existingItem.UpdateUI();
            }
        }

        private void DoUpdateStatus(object obj)
        {
            var _selectedItem = obj as VisualHFT.ViewModel.Model.Provider;
            if (_selectedItem != null)
            {
                eSESSIONSTATUS statusToSend;
                if (_selectedItem.Status == eSESSIONSTATUS.CONNECTED || _selectedItem.Status == eSESSIONSTATUS.CONNECTING || _selectedItem.Status == eSESSIONSTATUS.CONNECTED_WITH_WARNINGS)
                    statusToSend = eSESSIONSTATUS.DISCONNECTED;
                else if (_selectedItem.Status == eSESSIONSTATUS.DISCONNECTED || _selectedItem.Status == eSESSIONSTATUS.DISCONNECTED_FAILED)
                    statusToSend = eSESSIONSTATUS.CONNECTED;
                else //no status 
                    return;

                string msg = "Are you sure want to" + (statusToSend == eSESSIONSTATUS.CONNECTED ? " connect " : " disconnect ") + "'" + _selectedItem.ProviderName + "' ?";
                if (_dialogs.ContainsKey("confirm") && _dialogs["confirm"](msg, "Updating..."))
                {
                    var _linkToPlugIn = _selectedItem.Plugin as IDataRetriever;
                    if (_linkToPlugIn != null)
                    {
                        Task.Run(() =>
                        {
                            if (statusToSend == eSESSIONSTATUS.CONNECTED)
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
    }
}
