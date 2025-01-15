﻿namespace VisualHFT.Helpers
{
    public class HelperSymbol : List<string>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly HelperSymbol instance = new HelperSymbol();
        public static HelperSymbol Instance => instance;

        public event EventHandler<string> OnCollectionChanged;


        public void UpdateData(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return;
            if (this.Contains(symbol)) return;
            this.Add(symbol);
            OnCollectionChanged?.Invoke(this, symbol);
        }
        public void UpdateData(IEnumerable<string> symbols)
        {
            foreach (string symbol in symbols)
            {
                UpdateData(symbol);
            }
        }
    }
}