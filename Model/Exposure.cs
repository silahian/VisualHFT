using Prism.Mvvm;

namespace VisualHFT.Model
{

    public class Exposure : BindableBase
    {
        private string _symbol;
        private string _strategyName;
        private double _sizeExposed;
        private double _unrealizedPL;

        public Exposure(Exposure p)
        {
            Symbol = p.Symbol;
            StrategyName = p.StrategyName;
            SizeExposed = p.SizeExposed;
            UnrealizedPL = p.UnrealizedPL;
        }
        public string Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }
        public string StrategyName
        {
            get => _strategyName;
            set => SetProperty(ref _strategyName, value);
        }
        public double SizeExposed
        {
            get => _sizeExposed;
            set => SetProperty(ref _sizeExposed, value);
        }
        public double UnrealizedPL
        {
            get => _unrealizedPL;
            set => SetProperty(ref _unrealizedPL, value, onChanged: () => SetExposureRowColor(value));
        }

        private void SetExposureRowColor(double value)
        {
            if (value == 0)
            {
                ExposureRowColor = "White";
            }
            else if (value < 0)
            {
                ExposureRowColor = "Red";
            }
            else
            {
                ExposureRowColor = "Green";
            }
        }

        public string ExposureRowColor { get; private set; }

    }
}
