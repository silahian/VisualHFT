using System.Collections.Generic;

namespace VisualHFT.AnalyticReports.ViewModel
{
    public class cTop20
    {
        public string Symbol { get; }
        public decimal WinsPnL { get; }
        public decimal LossesPnL { get; }
        public decimal TotalPnl { get; }
        public int WinsCount { get; }
        public int LossesCount { get; }
        public int TotalCount { get; }

        public cTop20(string symbol, decimal winsPnL, decimal lossesPnL, decimal totalPnl, int winsCount, int lossesCount, int totalCount)
        {
            Symbol = symbol;
            WinsPnL = winsPnL;
            LossesPnL = lossesPnL;
            TotalPnl = totalPnl;
            WinsCount = winsCount;
            LossesCount = lossesCount;
            TotalCount = totalCount;
        }

        public override bool Equals(object obj)
        {
            return obj is cTop20 other &&
                   Symbol == other.Symbol &&
                   WinsPnL == other.WinsPnL &&
                   LossesPnL == other.LossesPnL &&
                   TotalPnl == other.TotalPnl &&
                   WinsCount == other.WinsCount &&
                   LossesCount == other.LossesCount &&
                   TotalCount == other.TotalCount;
        }

        public override int GetHashCode()
        {
            int hashCode = -2079422780;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Symbol);
            hashCode = hashCode * -1521134295 + WinsPnL.GetHashCode();
            hashCode = hashCode * -1521134295 + LossesPnL.GetHashCode();
            hashCode = hashCode * -1521134295 + TotalPnl.GetHashCode();
            hashCode = hashCode * -1521134295 + WinsCount.GetHashCode();
            hashCode = hashCode * -1521134295 + LossesCount.GetHashCode();
            hashCode = hashCode * -1521134295 + TotalCount.GetHashCode();
            return hashCode;
        }
    }
}
