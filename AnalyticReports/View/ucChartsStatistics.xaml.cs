using System.Windows.Controls;

namespace VisualHFT.AnalyticReports.View
{
    /// <summary>
    /// Interaction logic for ucChartsStatistics.xaml
    /// </summary>
    public partial class ucChartsStatistics : UserControl
    {
        public ucChartsStatistics()
        {
            InitializeComponent();
            this.DataContext = new VisualHFT.AnalyticReports.ViewModel.vmChartsStatistics();
        }
    }
}
