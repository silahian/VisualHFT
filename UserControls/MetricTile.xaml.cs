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
using System.Xml;
using VisualHFT.ViewModel.Studies;

namespace VisualHFT.UserControls
{
    /// <summary>
    /// Interaction logic for MetricTile.xaml
    /// </summary>
    public partial class MetricTile : UserControl
    {
        public MetricTile()
        {
            InitializeComponent();
        }

        private void OnSetupClick(object sender, RoutedEventArgs e)
        {
            SetupPopup.IsOpen = true;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            SetupPopup.IsOpen = false;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            SetupPopup.IsOpen = false;
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                ConvertHtmlToTextBlock(textBox.Text, txtToolTip);
            }
        }
        public TextBlock ConvertHtmlToTextBlock(string htmlText, TextBlock textBlock)
        {
            textBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
            textBlock.Width = 300;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root>" + htmlText + "</root>");

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    textBlock.Inlines.Add(new Run(node.Value));
                }
                else if (node.Name == "br")
                {
                    textBlock.Inlines.Add(new LineBreak());
                }
                else if (node.Name == "b")
                {
                    Run run = new Run(node.InnerText);
                    run.FontWeight = System.Windows.FontWeights.Bold;
                    textBlock.Inlines.Add(run);
                }
                else if (node.Name == "i")
                {
                    Run run = new Run(node.InnerText);
                    run.FontStyle = System.Windows.FontStyles.Italic;
                    textBlock.Inlines.Add(run);
                }
                // Add more formatting cases as needed
            }

            return textBlock;
        }


    }
}
