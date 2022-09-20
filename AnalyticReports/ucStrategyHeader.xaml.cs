using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace VisualHFT.AnalyticReport
{
    /// <summary>
    /// Interaction logic for ucStrategyHeader.xaml
    /// </summary>
    public partial class ucStrategyHeader : UserControl
    {
        public List<PositionEx> Signals { get; set; }
        

        public ucStrategyHeader()
        {
            InitializeComponent();
        }

        public void LoadData(List<PositionEx> signals)
        {
            this.Signals = signals;
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                if (this.Signals == null || this.Signals.Count == 0)
                    throw new Exception("No signals found.");
            }
            else
                return;

            
            try
            {
                
                //txtStrategyName.Content = "Strategies: " + string.Join(", ", this.Signals.Select(x => x.StrategyUsed.StrategyCode).Distinct().ToArray());
                //lblStrategyText.Content = " ----- ";
            }
            catch { }
            
        }
    }
}
