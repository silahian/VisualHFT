using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public static class HelperPnLCalculator
    {
        public static double CalculateRealizedPnL(List<OrderVM> buys, List<OrderVM> sells, PositionManagerCalculationMethod method)
        {
            double realizedPnL = 0;
            var buyOrders = method == PositionManagerCalculationMethod.FIFO ? buys.OrderBy(o => o.CreationTimeStamp).ToList() : buys.OrderByDescending(o => o.CreationTimeStamp).ToList();
            var sellOrders = method == PositionManagerCalculationMethod.FIFO ? sells.OrderBy(o => o.CreationTimeStamp).ToList() : sells.OrderByDescending(o => o.CreationTimeStamp).ToList();
            double buyQuantity = 0;
            double sellQuantity = 0;

            while (buyOrders.Any() && sellOrders.Any())
            {
                var buy = buyOrders.First();
                var sell = sellOrders.First();

                var matchedQuantity = Math.Min(buy.Quantity, Math.Abs(sell.Quantity));

                realizedPnL += matchedQuantity * (sell.PricePlaced - buy.PricePlaced);
                buyQuantity = buy.Quantity;
                sellQuantity = sell.Quantity;


                buyQuantity -= matchedQuantity;
                sellQuantity += matchedQuantity; // Since sell quantity is negative, we add to reduce it

                if (buyQuantity == 0) buyOrders.RemoveAt(0);
                if (sellQuantity == 0) sellOrders.RemoveAt(0);
            }

            return realizedPnL;
        }

        public static double CalculateOpenPnL(List<OrderVM> buys, List<OrderVM> sells, PositionManagerCalculationMethod method, double currentMidPrice)
        {
            double openPnL = 0;
            var currentPrice = currentMidPrice;

            var totalUnsoldQuantity = buys.Sum(b => b.Quantity) - Math.Abs(sells.Sum(s => s.Quantity));

            openPnL = totalUnsoldQuantity * currentPrice;

            return openPnL;
        }
    }
}
