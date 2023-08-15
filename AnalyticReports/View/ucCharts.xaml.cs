using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualHFT.AnalyticReports.View
{
    /// <summary>
    /// Interaction logic for ucChartsR.xaml
    /// </summary>
    public partial class ucCharts : UserControl
    {
        public ucCharts()
        {
            InitializeComponent();
            this.DataContext = new VisualHFT.AnalyticReports.ViewModel.vmCharts();
        }
    }
}
