using Prism.Mvvm;
using System;
using System.Collections.Generic;
using VisualHFT.Model;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class vmStrategyHeader : BindableBase
    {
        public List<VisualHFT.Model.Position> Signals { get; set; }
        public string StrategyName { get; private set; }
        public string StrategyText { get; private set; }

        public void LoadData(List<VisualHFT.Model.Position> signals)
        {
            this.Signals = signals;
            if (this.Signals == null || this.Signals.Count == 0)
                    throw new Exception("No signals found.");
            
            try
            {
                //StrategyName = "Strategies: " + string.Join(", ", this.Signals.Select(x => x.StrategyUsed.StrategyCode).Distinct().ToArray());
                //StrategyText = " ----- ";
            }
            catch { }

            RaisePropertyChanged(String.Empty);
        }
    }
}
