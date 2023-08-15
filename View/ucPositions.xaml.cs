using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using VisualHFT.Model;
using System.Web.UI.WebControls;

namespace VisualHFT.View
{
    /// <summary>
    /// Interaction logic for ucPositions.xaml
    /// </summary>
    public partial class ucPositions : UserControl
    {
        public static readonly DependencyProperty ucPositionsSymbolProperty = DependencyProperty.Register(
            "SelectedSymbol",
            typeof(string), typeof(ucPositions),
            new UIPropertyMetadata("", new PropertyChangedCallback(symbolChangedCallBack))
            );
        public static readonly DependencyProperty ucPositionsSelectedStrategyProperty = DependencyProperty.Register(
            "SelectedStrategy",
            typeof(string), typeof(ucPositions),
            new UIPropertyMetadata("", new PropertyChangedCallback(strategyChangedCallBack))
            );
        public static readonly DependencyProperty ucPositionsPositionProperty = DependencyProperty.Register(
            "Positions",
            typeof(ObservableCollection<Position>), typeof(ucPositions),
            new UIPropertyMetadata(new ObservableCollection<Position>(), new PropertyChangedCallback(positionChangedCallBack))
            );


        public ucPositions()
        {
            InitializeComponent();
            this.DataContext = new VisualHFT.ViewModel.vmPosition(Helpers.HelperCommon.GLOBAL_DIALOGS);

        }

        //private void SetFilter(Telerik.Windows.Controls.GridViewColumn col, string value)
        //{
        //    Telerik.Windows.Controls.GridView.IColumnFilterDescriptor colFilter = col.ColumnFilterDescriptor;
        //    // Suspend the notifications to avoid multiple data engine updates
        //    colFilter.SuspendNotifications();
        //    if (value == "-- All symbols --" || string.IsNullOrEmpty(value))
        //        colFilter.FieldFilter.Clear();
        //    else
        //    {
        //        colFilter.FieldFilter.Filter1.Operator = Telerik.Windows.Data.FilterOperator.IsEqualTo;
        //        colFilter.FieldFilter.Filter1.Value = value;
        //    }
        //    // Resume the notifications to force the data engine to update the filter.
        //    colFilter.ResumeNotifications();
        //}

        public string SelectedSymbol
        {
            get { return (string)GetValue(ucPositionsSymbolProperty); }
            set {
                SetValue(ucPositionsSymbolProperty, value);
                ((VisualHFT.ViewModel.vmPosition)this.DataContext).SelectedSymbol = value;

                //SetFilter(grdPositions.Columns[5], value);
                //SetFilter(grdTradeBottler.Columns[2], value);
            }
        }
        public string SelectedStrategy
        {
            get { return (string)GetValue(ucPositionsSelectedStrategyProperty); }
            set {
                SetValue(ucPositionsSelectedStrategyProperty, value);
                ((VisualHFT.ViewModel.vmPosition)this.DataContext).SelectedStrategy = value;

                //SetFilter(grdPositions.Columns[4], value);
            }
        }
        public ObservableCollection<Position> Positions
        {
            get { return (ObservableCollection<Position>)GetValue(ucPositionsPositionProperty); }
            set { SetValue(ucPositionsPositionProperty, value); /*((VisualHFT.ViewModel.vmPosition)this.DataContext).Positions = value;*/ }
        }



        static void symbolChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ucPositions ucSelf = (ucPositions)property;
            ucSelf.SelectedSymbol = (string)args.NewValue;
        }
        static void strategyChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ucPositions ucSelf = (ucPositions)property;
            ucSelf.SelectedStrategy = (string)args.NewValue;
        }
        static void positionChangedCallBack(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ucPositions ucSelf = (ucPositions)property;
            ucSelf.Positions = (ObservableCollection<Position>)args.NewValue;
        }

        private void butLoadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".pos";
                dlg.Filter = "POS Positions (*.pos)|*.pos|All (*.*)|*.*";
                
                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;
                    if (filename != "")
                    {
                        using (Stream stream = File.Open(filename, FileMode.Open))
                        {
                            var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            List<Position> tmpList = new List<Position>();
                            while (stream.Position != stream.Length)
                            {
                                tmpList.Add((Position)bformatter.Deserialize(stream));
                            }
                            //Helpers.HelperCommon.AllPositions = new ObservableCollection<Position>(tmpList);
                        }
                    }
                }
                MessageBox.Show("File has been succesfuly loaded.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void butSaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".pos";
                dlg.Filter = "POS Positions (*.pos)|*.pos|All (*.*)|*.*";
                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;
                    if (filename != "")
                    {
                        ObservableCollection<PositionEx> _positions = ((VisualHFT.ViewModel.vmPosition)this.DataContext).Positions;
                        if (_positions != null && _positions.Count > 0)
                        {
                            //serialize
                            string dir = Environment.CurrentDirectory;
                            using (Stream stream = File.Open(filename, FileMode.Create))
                            {
                                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                                bformatter.Serialize(stream, _positions.ToList());
                            }
                        }
                    }
                }
                MessageBox.Show("File has been succesfuly saved.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void butExportCsv_Click(object sender, RoutedEventArgs e)
        {
            string extension = "csv";
            SaveFileDialog dialog = new SaveFileDialog()
            {
                DefaultExt = extension,
                Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, "CSV File"),
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (Stream stream = dialog.OpenFile())
                    {
                        /*grdExecutions.Export(stream,
                         new GridViewExportOptions()
                         {
                             Format = ExportFormat.Csv,
                             ShowColumnHeaders = true,
                             ShowColumnFooters = true,
                             ShowGroupFooters = false,
                         });*/
                    }
                    MessageBox.Show("File has been succesfuly saved.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
    }
}
