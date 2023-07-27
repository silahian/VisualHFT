using Prism.Mvvm;
using System.ComponentModel;

namespace VisualHFT.Model
{
    public class StrategyVM : BindableBase
    {
        private string _strategyCode;
        public string StrategyCode 
        {
            get => _strategyCode;
            set => SetProperty(ref _strategyCode, value);
        }
    }
}
