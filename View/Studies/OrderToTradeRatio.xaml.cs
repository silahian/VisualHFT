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
using System.Windows.Shapes;

namespace VisualHFT.View
{
    /// <summary>
    /// Interaction logic for TradeToOrderRatio.xaml
    /// </summary>
    public partial class OrderToTradeRatio : Window
    {
        public OrderToTradeRatio()
        {
            InitializeComponent();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
