using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Printing;
using Telerik.Windows.Controls.ChartView;
using VisualHFT.Model;
using VisualHFT.Helpers;

namespace VisualHFT.AnalyticReport
{
    /// <summary>
    /// Interaction logic for BackTestReport.xaml
    /// </summary>
    public partial class AnalyticReport : Window
    {
        public List<PositionEx> Signals
        {
            get { return this.originalSignal.ToList(); }
            set
            {
                if (this.originalSignal == null)
                    this.originalSignal = value;
            }
        }
        public List<PositionEx> originalSignal { get; set; }

        public AnalyticReport()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMM();
            LoadPalettes();
            //Load data will be loaded from cboPalette_SelectionChanged
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
        private void LoadPalettes()
        {
            List<ChartPalette> palettes = new List<ChartPalette>();
            palettes.Add(ChartPalettes.Arctic);
            palettes.Add(ChartPalettes.Autumn);
            palettes.Add(ChartPalettes.Cold);
            palettes.Add(ChartPalettes.Flower);
            palettes.Add(ChartPalettes.Forest);
            palettes.Add(ChartPalettes.Grayscale);
            palettes.Add(ChartPalettes.Ground);
            palettes.Add(ChartPalettes.Lilac);
            palettes.Add(ChartPalettes.Natural);
            palettes.Add(ChartPalettes.Pastel);
            palettes.Add(ChartPalettes.Rainbow);
            palettes.Add(ChartPalettes.Spring);
            palettes.Add(ChartPalettes.Summer);
            palettes.Add(ChartPalettes.Warm);
            palettes.Add(ChartPalettes.Windows8);
            cboPalette.ItemsSource = palettes;
            cboPalette.SelectedIndex = 14;
        }
        private void LoadData(ChartPalette selectedPallet)
        {
            if (this.Signals != null)
                this.Signals = this.Signals.OrderBy(x => x.CreationTimeStamp).ToList();
            if (this.Signals.Count > 0)
            {
                this.Title = "HFT Analytics";

                try
                {
                    ucStrategyHeader1.LoadData(this.Signals.ToList());
                    ucOverview1.LoadData(this.Signals.ToList());
                    ucEquityChart1.LoadData(this.Signals.ToList(), selectedPallet);
                    ucStats1.LoadData(this.Signals.ToList());
                    ucCharts1.LoadData(this.Signals.ToList(), selectedPallet);
                    ucChartsStatistics1.LoadData(this.Signals.ToList(), selectedPallet);
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
                LoadData(cboPalette.SelectedItem as ChartPalette);
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
                    LoadData(cboPalette.SelectedItem as ChartPalette);
            //}
        }

        private void cboMoneyManagement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        

    }
}
