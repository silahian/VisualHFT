using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using VisualHFT.Enums;
using VisualHFT.Helpers;

namespace VisualHFT.ViewModel.Model
{
    [AddINotifyPropertyChangedInterface]
    public class Provider : VisualHFT.Model.Provider, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static ObservableCollection<VisualHFT.ViewModel.Model.Provider> CreateObservableCollection()
        {

            return new ObservableCollection<Provider>(HelperProvider.Instance.ToList().Select(x => new ViewModel.Model.Provider(x)));

        }
        public Provider()
        {

        }
        public Provider(VisualHFT.Model.Provider p)
        {
            this.ProviderID = p.ProviderID;
            this.ProviderCode = p.ProviderCode;
            this.ProviderName = p.ProviderName;
            this.Status = p.Status;
            this.LastUpdated = p.LastUpdated;
            this.Plugin = p.Plugin;
        }
        public void UpdateUI()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tooltip"));
        }
    }
}