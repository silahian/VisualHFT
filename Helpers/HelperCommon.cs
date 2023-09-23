using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VisualHFT.DataTradeRetriever;

namespace VisualHFT.Helpers
{
    public class HelperCommon
    {
        public static DateTime GetCurrentSessionIni(DateTime? currentSessionDate)
        {
            currentSessionDate = currentSessionDate.ToDate().AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
            if (!currentSessionDate.HasValue)
                currentSessionDate = DateTime.Now;
            DateTime DateAt00 = currentSessionDate.ToDate();
            DateTime DateAt500PM = DateAt00.AddHours(17);
            DateTime DateAt530PM = DateAt00.AddHours(17).AddMinutes(30);
            if (currentSessionDate > DateAt500PM && currentSessionDate < DateAt00.AddDays(1))
                return DateAt530PM;
            else if (currentSessionDate > DateAt00 && currentSessionDate < DateAt500PM)
                return DateAt530PM.AddDays(-1);
            else
                return DateTime.Now;
        }
        public static DateTime GetCurrentSessionEnd(DateTime? currentSessionDate)
        {
            currentSessionDate = currentSessionDate.ToDate().AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
            if (!currentSessionDate.HasValue)
                currentSessionDate = DateTime.Now;
            DateTime DateAt00 = currentSessionDate.ToDate();
            DateTime DateAt500PM = DateAt00.AddHours(17);
            DateTime DateAt530PM = DateAt00.AddHours(17).AddMinutes(30);
            if (currentSessionDate > DateAt500PM && currentSessionDate < DateAt00.AddDays(1))
                return DateAt500PM.AddDays(1);
            else if (currentSessionDate > DateAt00 && currentSessionDate < DateAt500PM)
                return DateAt500PM;
            else
                return DateTime.Now;
        }
        public static int TimerMillisecondsToGetVariables = 1000 * 10; //10 seconds
        
        public static ObservableCollection<string> ALLSYMBOLS = new ObservableCollection<string>();
        public static HelperProvider PROVIDERS = new HelperProvider();
        public static HelperOrderBook LIMITORDERBOOK = new HelperOrderBook();

        public static IDataTradeRetriever EXECUTEDORDERS = new EmptyTradesRetriever();
        //public static IDataTradeRetriever EXECUTEDORDERS = new MSSQLServerTradesRetriever();
        //public static IDataTradeRetriever EXECUTEDORDERS = new FIXTradesRetriever([path_to_fix_log_file], 1, "CME");

        public static HelperExposure EXPOSURES = new HelperExposure();
        public static HelperActiveOrder ACTIVEORDERS = new HelperActiveOrder();
        public static HelperStrategy ACTIVESTRATEGIES = new HelperStrategy();
        public static HelperStrategyParams STRATEGYPARAMS = new HelperStrategyParams();
        public static HelperTrade TRADES = new HelperTrade();

        public static Func<string, string, bool> GetPopup()
        {
            return (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK);
        }
        public static Func<string, string, bool> GetConfirmPopup()
        {
            return (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
        }
        public static Func<string, string, bool> GetValidationPopup()
        {
            return (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.OK, MessageBoxImage.Asterisk) == MessageBoxResult.Yes);
        }
        public static Func<string, string, bool> GetErrorPopup()
        {
            return (Func<string, string, bool>)((msg, capt) => MessageBox.Show(msg, capt, MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.Yes);
        }


        public static Dictionary<string, Func<string, string, bool>> GLOBAL_DIALOGS = new Dictionary<string, Func<string, string, bool>>()
                {
                    {"popup", GetPopup() },
                    {"confirm", GetConfirmPopup() },
                    {"validation", GetValidationPopup() },
                    {"error", GetErrorPopup() }
                };        
        public static void CreateCommonPopUpWindow(FrameworkElement ctrlToSendIntoNewWindow, Button butPopUp, object dataContext, string titleWindow = "New Window", int widthWindow = 800, int heightWindow = 600, ResizeMode resizeMode = ResizeMode.CanResize)
        {
            // Create a new window
            Window window = new Window
            {
                Width = widthWindow, // Set the width of the window
                Height = heightWindow, // Set the height of the window
                Title = titleWindow // Set the title of the window
                ,ResizeMode= resizeMode
            };

            Grid parent = (Grid)ctrlToSendIntoNewWindow.Parent;

            // Create a placeholder control
            var placeholder = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };



            // Handle the Loaded event of the new window
            window.Loaded += (s, e3) =>
            {
                // Remove the chart from its current parent                
                parent.Children.Remove(ctrlToSendIntoNewWindow);


                // Add a message to the placeholder
                placeholder.Children.Add(new TextBlock
                {
                    Text = "The chart is being displayed in a new window.",
                    TextAlignment = TextAlignment.Center
                });

                // Add a button to the placeholder
                var button = new Button
                {
                    Content = "Focus Window"
                };
                button.Click += (s1, e1) => window.Focus();
                placeholder.Children.Add(button);

                // Add the placeholder to the original parent
                parent.Children.Add(placeholder);


                // Add the chart to the window
                ctrlToSendIntoNewWindow.DataContext = dataContext;
                window.Content = ctrlToSendIntoNewWindow;
                butPopUp.Visibility = Visibility.Collapsed;
            };

            // Handle the Closed event of the new window
            window.Closed += (s, e3) =>
            {
                // Remove the chart from the window
                window.Content = null;

                // Remove the placeholder from the original parent
                parent.Children.Remove(placeholder);

                // Add the chart back to its original parent
                parent.Children.Add(ctrlToSendIntoNewWindow);
                butPopUp.Visibility = Visibility.Visible;
            };


            // Show the window
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }
        public static string GetKiloFormatter(int num)
        {
            return GetKiloFormatter((double)num);
        }
        public static string GetKiloFormatter(decimal num)
        {
            return GetKiloFormatter((double)num);
        }
        public static string GetKiloFormatter(double num)
        {
            if (num < 500)
                return num.ToString("N2");
            if (num < 10000)
                return num.ToString("N0");


            if (num >= 100000000)
                return (num / 1000000D).ToString("0.#M");
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##M");
            if (num >= 100000)
                return (num / 1000D).ToString("0.#k");
            if (num >= 10000)
                return (num / 1000D).ToString("0.##k");
            return num.ToString("#,0");
        }
        public static string GetKiloFormatterTime(double milliseconds)
        {
            double num = milliseconds;

            if (num >= 1000 * 60 * 60.0)
                return (num / (60.0 * 60.0 * 1000D)).ToString("0.0 hs");
            if (num >= 1000 * 60.0)
                return (num / (60.0 * 1000D)).ToString("0.0 min");
            if (num >= 1000)
                return (num / 1000D).ToString("0.0# sec");

            return num.ToString("#,0 ms");
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }
}

