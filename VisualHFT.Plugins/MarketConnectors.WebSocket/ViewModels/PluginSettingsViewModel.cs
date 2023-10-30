using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisualHFT.DataRetriever;
using VisualHFT.Helpers;

namespace MarketConnectors.WebSocket.ViewModel
{
    public class PluginSettingsViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _hostName;
        private int _port;
        private int _providerId;
        private string _providerName;
        private string _validationMessage;
        private string _successMessage;
        private Action _actionCloseWindow;
        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public Action UpdateSettingsFromUI{ get; set; }

        public PluginSettingsViewModel(Action actionCloseWindow)
        {
            OkCommand = new RelayCommand<object>(ExecuteOkCommand, CanExecuteOkCommand);
            CancelCommand = new RelayCommand<object>(ExecuteCancelCommand);
            _actionCloseWindow = actionCloseWindow;
        }
        public string HostName
        {
            get => _hostName;
            set { _hostName = value; OnPropertyChanged(nameof(HostName)); }
        }
        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }
        public int ProviderId
        {
            get => _providerId;
            set
            {
                _providerId = value;
                OnPropertyChanged(nameof(ProviderId));
                RaiseCanExecuteChanged();
            }
        }
        public string ProviderName
        {
            get => _providerName;
            set
            {
                _providerName = value;
                OnPropertyChanged(nameof(ProviderName));
                RaiseCanExecuteChanged();
            }
        }
        public string ValidationMessage
        {
            get { return _validationMessage; }
            set { _validationMessage = value; OnPropertyChanged(nameof(ValidationMessage));}
        }

        public string SuccessMessage
        {
            get { return _successMessage; }
            set { _successMessage = value; OnPropertyChanged(nameof(SuccessMessage)); }
        }
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    /*case nameof(ApiKey):
                        if (string.IsNullOrWhiteSpace(ApiKey))
                            return "API Key cannot be empty.";
                        break;

                    case nameof(ApiSecret):
                        if (string.IsNullOrWhiteSpace(ApiSecret))
                            return "API Secret cannot be empty.";
                        break;*/

                    case nameof(ProviderId):
                        if (ProviderId <= 0)
                            return "Provider ID should be a positive integer.";
                        break;

                    case nameof(ProviderName):
                        if (string.IsNullOrWhiteSpace(ProviderName))
                            return "Provider name cannot be empty.";
                        break;

                    default:
                        return null;
                }
                return null;
            }
        }

        private void ExecuteOkCommand(object obj)
        {            
            SuccessMessage = "Settings saved successfully!";
            UpdateSettingsFromUI?.Invoke();
            _actionCloseWindow?.Invoke();
        }
        private void ExecuteCancelCommand(object obj)
        {
            _actionCloseWindow?.Invoke();
        }
        private bool CanExecuteOkCommand(object obj)
        {
            // This checks if any validation message exists for any of the properties
            return string.IsNullOrWhiteSpace(this[nameof(HostName)]) &&
                   string.IsNullOrWhiteSpace(this[nameof(Port)]) &&
                   string.IsNullOrWhiteSpace(this[nameof(ProviderId)]) &&
                   string.IsNullOrWhiteSpace(this[nameof(ProviderName)]);
        }
        private void RaiseCanExecuteChanged()
        {
            (OkCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
