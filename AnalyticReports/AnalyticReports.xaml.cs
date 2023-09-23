using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Printing;
using VisualHFT.Model;
using VisualHFT.Helpers;
using VisualHFT.AnalyticReports.ViewModel;
using VisualHFT.AnalyticReports.View;

namespace VisualHFT.AnalyticReport
{
    /// <summary>
    /// Interaction logic for BackTestReport.xaml
    /// </summary>
    public partial class AnalyticReport : Window
    {
        public List<VisualHFT.Model.Position> Signals
        {
            get { return this.originalSignal.ToList(); }
            set
            {
                this.originalSignal = value;                    
            }
        }
        public List<VisualHFT.Model.Position> originalSignal { get; set; }

        public AnalyticReport()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMM();
            LoadData();
        }
        private void LoadMM()
        {
            //*************************************************************************************
            //LOAD MONEY MANAGEMENT systems
            //cboMoneyManagement.ItemsSource = MMBase.LoadAllMM();
            //cboMoneyManagement.DisplayMemberPath = "Key";
            //cboMoneyManagement.SelectedValuePath = "Value";
            //*************************************************************************************
        }        
        private void LoadData()
        {
            if (this.Signals != null)
                this.Signals = this.Signals.OrderBy(x => x.CreationTimeStamp).ToList();
            if (this.Signals.Count > 0)
            {
                this.Title = "HFT Analytics";

                try
                {
                    ((vmStrategyHeader)ucStrategyHeader1.DataContext).LoadData(this.Signals.ToList());
                    ((vmOverview)ucOverview1.DataContext).LoadData(this.Signals.ToList());
                    ((vmEquityChart)ucEquityChart1.DataContext).LoadData(this.Signals.ToList());
                    ((vmStats)ucStats1.DataContext).LoadData(this.Signals.ToList());
                    ((vmCharts)ucCharts1.DataContext).LoadData(this.Signals.ToList());
                    ((vmChartsStatistics)ucChartsStatistics1.DataContext).LoadData(this.Signals.ToList());
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString(), "ERRROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.Activate();
            }
            else
            {
                MessageBox.Show("No data.");
            }
        }

        private void cboPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPalette.SelectedItem != null)
                LoadData();
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {

            StackPanel toPrint = (scrollviewer1.Content as StackPanel);
            //ScrollViewer toPrint = scrollviewer1;
            //Window toPrint = this;

            double originalW = toPrint.ActualWidth;
            double originalH = toPrint.ActualHeight;

            PrintDialog printDlg = new PrintDialog();            
            printDlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
            printDlg.PrintTicket.PageMediaType = PageMediaType.Continuous;
            printDlg.PrintTicket.PageMediaSize = new PageMediaSize(11 * 96, 8 * 96);
            Nullable<Boolean> print = printDlg.ShowDialog();

            if (print == true)
            {
                
                //get selected printer capabilities
                System.Printing.PrintCapabilities capabilities = printDlg.PrintQueue.GetPrintCapabilities(printDlg.PrintTicket);
                //get scale of the print wrt to screen of WPF visual
                double scale = Math.Min(printDlg.PrintTicket.PageMediaSize.Width.Value / toPrint.ActualWidth, printDlg.PrintTicket.PageMediaSize.Height.Value / toPrint.ActualHeight);                


                //Transform the Visual to scale
                toPrint.LayoutTransform = new ScaleTransform(scale, scale);

                //get the size of the printer page
                Size sz = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
                //update the layout of the visual to the printer page size.
                toPrint.Measure(sz);
                toPrint.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                
                var paginator = new ProgramPaginator((scrollviewer1.Content as StackPanel));
                paginator.PageSize = new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);

                printDlg.PrintDocument(paginator, ucStrategyHeader1.txtStrategyName.Content.ToString());

                //update the layout of the visual to the printer page size.            
                sz = new Size(originalW, originalH);
                toPrint.Measure(sz);
                toPrint.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                
            }
             
        }

        private void butReload_Click(object sender, RoutedEventArgs e)
        {
            //if (cboMoneyManagement.SelectedItem != null && (((KeyValuePair<string, object>)cboMoneyManagement.SelectedItem).Value as MMBase) != null )
            //    ApplyMM(((KeyValuePair<string, object>)cboMoneyManagement.SelectedItem).Value as MMBase);
            //else
            //{
                if (cboPalette.SelectedItem != null)
                    LoadData();
            //}
        }

        private void cboMoneyManagement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        

    }
}
