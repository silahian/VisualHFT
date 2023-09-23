using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.IO;
using System.Windows.Data;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class HelperDataGrid
    {
        public static string ExportDataGrid(DataGrid dGrid)
        {
            if (true)
            {
                string strFormat = "CSV";
                StringBuilder strBuilder = new StringBuilder();
                if (dGrid.ItemsSource == null) return "";
                List<string> lstFields = new List<string>();
                if (dGrid.HeadersVisibility == DataGridHeadersVisibility.Column ||
                    dGrid.HeadersVisibility == DataGridHeadersVisibility.All)
                {
                    foreach (DataGridColumn dgcol in dGrid.Columns)
                        lstFields.Add(FormatField(dgcol.Header.ToString(), strFormat));
                    BuildStringOfRow(strBuilder, lstFields, strFormat);
                }
                foreach (object data in dGrid.ItemsSource)
                {
                    lstFields.Clear();
                    foreach (DataGridColumn col in dGrid.Columns)
                    {
                        string strValue = "";
                        Binding objBinding = null;
                        if (col is DataGridBoundColumn)
                            objBinding = (col as DataGridBoundColumn).Binding as Binding;
                        if (col is DataGridTemplateColumn)
                        {
                            //This is a template column...
                            //    let us see the underlying dependency object
                            DependencyObject objDO =
                              (col as DataGridTemplateColumn).CellTemplate.LoadContent();
                            FrameworkElement oFE = (FrameworkElement)objDO;
                            System.Reflection.FieldInfo oFI = oFE.GetType().GetField("TextProperty");
                            if (oFI != null)
                            {
                                if (oFI.GetValue(null) != null)
                                {
                                    if (oFE.GetBindingExpression(
                                           (DependencyProperty)oFI.GetValue(null)) != null)
                                        objBinding =
                                          oFE.GetBindingExpression(
                                          (DependencyProperty)oFI.GetValue(null)).ParentBinding;
                                }
                            }
                        }
                        if (objBinding != null)
                        {
                            if (objBinding.Path.Path != "")
                            {
                                Object objValue = GetNestedPropValue(objBinding.Path.Path, data);
                                if (objValue != null)
                                    strValue = objValue.ToString();
                                //System.Reflection.PropertyInfo pi = data.GetType().GetProperty(objBinding.Path.Path);
                                //if (pi != null) strValue = pi.GetValue(data, null).ToString();
                            }
                            if (objBinding.Converter != null)
                            {
                                if (strValue != "")
                                    strValue = objBinding.Converter.Convert(strValue,
                                      typeof(string), objBinding.ConverterParameter,
                                      objBinding.ConverterCulture).ToString();
                                else
                                    strValue = objBinding.Converter.Convert(data,
                                      typeof(string), objBinding.ConverterParameter,
                                      objBinding.ConverterCulture).ToString();
                            }
                        }
                        lstFields.Add(FormatField(strValue, strFormat));
                    }
                    BuildStringOfRow(strBuilder, lstFields, strFormat);
                }
                string strFilename = GetTempFile();
                StreamWriter sw = new StreamWriter(strFilename);
                if (strFormat == "XML")
                {
                    //Let us write the headers for the Excel XML
                    sw.WriteLine("<?xml version=\"1.0\" " +
                                 "encoding=\"utf-8\"?>");
                    sw.WriteLine("<?mso-application progid" +
                                 "=\"Excel.Sheet\"?>");
                    sw.WriteLine("<Workbook xmlns=\"urn:" +
                                 "schemas-microsoft-com:office:spreadsheet\">");
                    sw.WriteLine("<DocumentProperties " +
                                 "xmlns=\"urn:schemas-microsoft-com:" +
                                 "office:office\">");
                    sw.WriteLine("<Author>Arasu Elango</Author>");
                    sw.WriteLine("<Created>" +
                                 DateTime.Now.ToLocalTime().ToLongDateString() +
                                 "</Created>");
                    sw.WriteLine("<LastSaved>" +
                                 DateTime.Now.ToLocalTime().ToLongDateString() +
                                 "</LastSaved>");
                    sw.WriteLine("<Company>Atom8 IT Solutions (P) " +
                                 "Ltd.,</Company>");
                    sw.WriteLine("<Version>12.00</Version>");
                    sw.WriteLine("</DocumentProperties>");
                    sw.WriteLine("<Worksheet ss:Name=\"Silverlight Export\" " +
                       "xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
                    sw.WriteLine("<Table>");
                }
                sw.Write(strBuilder.ToString());
                if (strFormat == "XML")
                {
                    sw.WriteLine("</Table>");
                    sw.WriteLine("</Worksheet>");
                    sw.WriteLine("</Workbook>");
                }
                sw.Close();
                return strFilename;
            }
        }
        public static IEnumerable<System.Windows.Controls.DataGridRow> GetDataGridRows(System.Windows.Controls.DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.DataGridRow;
                if (null != row) yield return row;
            }
        }
        public static void FormatDecimals(System.Windows.Controls.DataGrid grid, string columnBrokerID, params string[] columnsDataToFormat)
        {
            //NO WORKING 
            var rows = GetDataGridRows(grid);
            
            foreach (DataGridRow r in rows)
            {
                var colBroker = grid.Columns.Where(x => x.SortMemberPath == columnBrokerID).FirstOrDefault();
                if (colBroker == null)
                    continue;
                int brokerID = Convert.ToInt16((colBroker.GetCellContent(r) as TextBlock).Text);
                string sFormat = "{0:F3}";

                foreach (DataGridColumn column in grid.Columns.Where(x => columnsDataToFormat.Contains(x.SortMemberPath)))
                {
                    if (column.GetCellContent(r) is TextBlock)
                    {
                        TextBlock cellContent = column.GetCellContent(r) as TextBlock;
                        cellContent.Text = string.Format(cellContent.Text, sFormat);
                    }
                }
            }
            grid.UpdateLayout();
        }
        
        private static void BuildStringOfRow(StringBuilder strBuilder,
                List<string> lstFields, string strFormat)
        {
            switch (strFormat)
            {
                case "XML":
                    strBuilder.AppendLine("<Row>");
                    strBuilder.AppendLine(String.Join("\r\n", lstFields.ToArray()));
                    strBuilder.AppendLine("</Row>");
                    break;
                case "CSV":
                    strBuilder.AppendLine(String.Join(",", lstFields.ToArray()));
                    break;
            }
        }
        private static string FormatField(string data, string format)
        {
            switch (format)
            {
                case "XML":
                    return String.Format("<Cell><Data ss:Type=\"String" +
                       "\">{0}</Data></Cell>", data);
                case "CSV":
                    return String.Format("\"{0}\"",
                      data.Replace("\"", "\"\"\"").Replace("\n",
                      "").Replace("\r", ""));
            }
            return data;
        }
        private static Object GetNestedPropValue(String name, Object obj)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                System.Reflection.PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }
        private static string GetTempFile()
        {
            string sFileName = "export";
            string sFile = System.IO.Path.GetTempPath() + @"\" + sFileName + ".csv";
            long iCont = 1;
            while (File.Exists(sFile))
            {
                sFile = System.IO.Path.GetTempPath() + @"\" + sFileName + iCont.ToString() + ".csv";
                iCont++;
            }
            return sFile;
        }

    }

    
    public class PnLStyle : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is VisualHFT.Model.Position)
            {
                VisualHFT.Model.Position pos = item as VisualHFT.Model.Position;
                if (pos.CloseProviderId < 0) //means that is an open position
                    return NullBackColorStyle;
                else if (pos.GetPipsPnL >= 0)
                {
                    return GreenBackColorStyle;
                }
                else if (pos.GetPipsPnL < 0)
                {
                    return RedBackColorStyle;
                }
            }
            return null;
        }
        public Style GreenBackColorStyle { get; set; }
        public Style RedBackColorStyle { get; set; }
        public Style NullBackColorStyle { get; set; }
    }
    
}
