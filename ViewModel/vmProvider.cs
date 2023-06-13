using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;

namespace VisualHFT.ViewModel
{
    public class vmProvider : BindableBase
    {
        private ProviderVM _selectedItem;
        private ObservableCollection<ProviderVM> _providers;
        private RelayCommand _cmdUpdateStatus;
        private Dictionary<string, Func<string, string, bool>> _dialogs;

        public vmProvider(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;            
            _cmdUpdateStatus = new RelayCommand(DoUpdateStatus);
            
            _providers = new ObservableCollection<ProviderVM>();
            RaisePropertyChanged(nameof(Providers));

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;

        }

        private void PROVIDERS_OnDataReceived(object sender, ProviderVM e)
        {
            if (e == null || e.ProviderID == -1)
                return;
            var existingProv = _providers.Where(x => x.ProviderID == e.ProviderID).FirstOrDefault();
            if (existingProv != null)
            {
                existingProv.Status = e.Status;
                existingProv.LastUpdated = e.LastUpdated;
            }
            else
            {
                _providers.Add(e);
            }
        }

        private void DoUpdateStatus(object obj)
        {
            _selectedItem = obj as ProviderVM;
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
                            args.Result = RESTFulHelper.SetVariable<List<ProviderVM>>(_providers.ToList());
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
        public ObservableCollection<ProviderVM> Providers
        {
            get => _providers;
            set => SetProperty(ref _providers, value); 
        }

        public RelayCommand CmdUpdateStatus
        {
            get => _cmdUpdateStatus;
            set => SetProperty(ref _cmdUpdateStatus, value);   
        }

        public ProviderVM SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

    }
}
