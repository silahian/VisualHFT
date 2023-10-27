using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace VisualHFT.Helpers
{
    public static class ListViewItemSelectedBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ListViewItemSelectedBehavior), new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                listView.PreviewMouseLeftButtonDown -= ListView_PreviewMouseLeftButtonDown;

                if (e.NewValue != null)
                {
                    listView.PreviewMouseLeftButtonDown += ListView_PreviewMouseLeftButtonDown;
                }
            }
        }

        private static void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView)
            {
                var item = ItemsControl.ContainerFromElement(listView, e.OriginalSource as DependencyObject) as ListViewItem;
                if (item != null)
                {
                    listView.SelectedItem = item.Content;

                    var command = GetCommand(listView);
                    if (command?.CanExecute(listView.SelectedItem) == true)
                    {
                        command.Execute(listView.SelectedItem);
                    }
                }
            }
        }
    }

}
