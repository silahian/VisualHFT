using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using VisualHFT.Model;

namespace VisualHFT.View
{
    /// <summary>
    /// Interaction logic for ucPositions.xaml
    /// </summary>
    public partial class ucPositions : UserControl
    {


        public ucPositions()
        {
            InitializeComponent();
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
                /*if (result == true)
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
                }*/
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
                    /*if (filename != "")
                    {
                        ObservableCollection<OrderVM> _positions = ((VisualHFT.ViewModel.vmPosition)this.DataContext).AllOrders;
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
                    }*/
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
