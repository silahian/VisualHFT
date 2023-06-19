using demoTradingCore.Models;
using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;
using System.Timers;
using Microsoft.Win32;

namespace demoTradingCore
{
    public class StrategyHeartBeatEventArgs : EventArgs
    {
        public string StrategyName;

    }
    public class StrategyExposureEventArgs : EventArgs
    {
        public string Symbol;
        public string StrategyName;
        public decimal SizeExposed;
        public decimal UnrealizedPL;
    }


    public class Strategy
    {
        const string _STRATEGY_NAME = "FirmMM";

        List<Exchange> _EXCHANGES = null;
        string _SYMBOL = "";
        Queue<decimal> _QUEUE_DELTA_ASKS = new Queue<decimal>();
        Queue<decimal> _QUEUE_DELTA_BIDS = new Queue<decimal>();
        int MAX_QUEUE_SIZE = 1000;
        decimal _EXPOSED_SIZE = 0;
        decimal _UNREALIZEDPL = 0;

        public event EventHandler<StrategyExposureEventArgs> OnStrategyExposure;
        public Strategy(List<Exchange> exchanges, string symbol)
        {
            _EXCHANGES = exchanges;
            _SYMBOL = symbol;
        }
        public string GetStrategyName()
        {
            return _STRATEGY_NAME;
        }
        public void UpdateSnapshot(ExchangeOrderBook ob)
        {
            lock (_EXCHANGES)
            {
                ExecuteStrategy();
            }
        }

        private void ExecuteStrategy()
        {
            //Idea:
            /*
                Collect the prices from the two exchanges, and keep track of their usual price difference.
                Once, we can spot an unusual difference in prices, we act.
             */
            if (_EXCHANGES != null && _EXCHANGES.Count > 1)
            {
                //check bid/ask
                var tob_Exch0 = _EXCHANGES[0].GetTopOfBook(_SYMBOL);
                var tob_Exch1 = _EXCHANGES[1].GetTopOfBook(_SYMBOL);
                if (tob_Exch0 != null && tob_Exch1 != null)
                {

                    var diffBid = tob_Exch0.First().Price - tob_Exch1.First().Price;
                    var diffAsk = tob_Exch0.Last().Price - tob_Exch1.Last().Price;
                    var highestBid = (tob_Exch0.First().Price > tob_Exch1.First().Price ? tob_Exch0.First() : tob_Exch1.First());
                    var lowestAsk = (tob_Exch0.Last().Price < tob_Exch1.Last().Price ? tob_Exch0.Last() : tob_Exch1.Last());
                    if (_QUEUE_DELTA_ASKS.Count > MAX_QUEUE_SIZE * 0.8 && _QUEUE_DELTA_BIDS.Count > MAX_QUEUE_SIZE * 0.8)
                    {
                        var avgAskDiff = CalculateAverage(_QUEUE_DELTA_ASKS);
                        var avgBidDiff = CalculateAverage(_QUEUE_DELTA_BIDS);


                        if ((Math.Abs(diffBid) + CalculateStandardDeviation(_QUEUE_DELTA_BIDS) * 3 > Math.Abs(avgBidDiff)
                            || Math.Abs(diffAsk) + CalculateStandardDeviation(_QUEUE_DELTA_ASKS) * 3 > Math.Abs(avgAskDiff))
                            && highestBid.Price > lowestAsk.Price
                            )
                        {
                            //we want to sell on the higher bid 
                            // and sell on the lower ask
                            var profit = highestBid.Price - lowestAsk.Price;


                            string openExchange = (tob_Exch0.First().Price > tob_Exch1.First().Price ? _EXCHANGES[0].ExchangeName : _EXCHANGES[1].ExchangeName);
                            string closeExchange = (tob_Exch0.Last().Price < tob_Exch1.Last().Price ? _EXCHANGES[0].ExchangeName : _EXCHANGES[1].ExchangeName);
                            CreateNewPosition(openExchange, closeExchange, highestBid.Price, lowestAsk.Price);




                            Console.WriteLine($"Profit: {profit.ToString("c2")}");
                            _UNREALIZEDPL += profit;
                            _EXPOSED_SIZE += 0; //since we are doing arb, we are always closing the position. Hence, no exposure.

                            //Trigger event
                            var args = new StrategyExposureEventArgs { SizeExposed = _EXPOSED_SIZE, StrategyName = _STRATEGY_NAME, Symbol = _SYMBOL, UnrealizedPL = _UNREALIZEDPL };
                            OnStrategyExposure?.Invoke(this, args);


                            _QUEUE_DELTA_BIDS.Clear(); //reset
                            _QUEUE_DELTA_ASKS.Clear(); //reset
                        }
                    }




                    //add current deltas
                    AddToDeltaQueues(diffBid, true);
                    AddToDeltaQueues(diffAsk, false);
                }
            }
        }
        private void AddToDeltaQueues(decimal diffPrice, bool isBid) //keep the price difference in a queue
        {
            //do not add if the same

            if (isBid)
            {
                decimal peekValue = 0;
                if (_QUEUE_DELTA_BIDS.Any())
                    peekValue = _QUEUE_DELTA_BIDS.Last();
                if (!_QUEUE_DELTA_BIDS.Any() || peekValue != diffPrice)
                {
                    _QUEUE_DELTA_BIDS.Enqueue(diffPrice);
                    if (_QUEUE_DELTA_BIDS.Count > MAX_QUEUE_SIZE) { _QUEUE_DELTA_BIDS.Dequeue(); }
                }
            }
            else
            {
                decimal peekValue = 0;
                if (_QUEUE_DELTA_ASKS.Any())
                    peekValue = _QUEUE_DELTA_ASKS.Last();

                if (!_QUEUE_DELTA_ASKS.Any() || peekValue != diffPrice)
                {
                    _QUEUE_DELTA_ASKS.Enqueue(diffPrice);
                    if (_QUEUE_DELTA_ASKS.Count > MAX_QUEUE_SIZE) { _QUEUE_DELTA_ASKS.Dequeue(); }
                }
            }
        }
        private decimal CalculateAverage(Queue<decimal> values)
        {
            if (values.Count == 0)
                return 0;

            decimal sum = 0;
            foreach (decimal value in values)
            {
                sum += value;
            }
            decimal average = sum / values.Count;

            return average;
        }
        private decimal CalculateStandardDeviation(Queue<decimal> values)
        {
            if (values.Count == 0)
                return 0;

            decimal average = CalculateAverage(values);

            decimal squaredDifferencesSum = 0;
            foreach (decimal value in values)
            {
                decimal difference = value - average;
                squaredDifferencesSum += difference * difference;
            }
            double variance = (double)squaredDifferencesSum / values.Count;
            double standardDeviation = Math.Sqrt(variance);

            return (decimal)standardDeviation;
        }


        private void CreateNewPosition(string openExchange, string closingExchange, decimal openPrice, decimal closePrice)
        {
            int openingProviderID = openExchange.ToUpper() == "COINBASE" ? 14 : 23; //ID's took from the database
            int closingProviderID = openExchange.ToUpper() == "COINBASE" ? 14 : 23; // This is hardcoded and for demostration only

            /*using (var db = new VisualHFT.Model.HFTEntities())
            {
                db.Positions.Add(new VisualHFT.Model.Position()
                {
                    CreationTimeStamp = DateTime.Now,
                    CloseTimeStamp = DateTime.Now,
                    OpenClOrdId = Guid.NewGuid().ToString(),
                    CloseClOrdId = Guid.NewGuid().ToString(),
                    OpenProviderId = openingProviderID,
                    CloseProviderId = closingProviderID,
                    CloseStatus = 6,
                    OrderQuantity = 1,
                    GetCloseQuantity = 1,
                    GetOpenQuantity = 1,
                    GetOpenAvgPrice = openPrice,
                    GetCloseAvgPrice = closePrice,

                    IsCloseMM = false,
                    IsOpenMM = false,
                    Side = 1, //SELL
                    StrategyCode = _STRATEGY_NAME,
                    Symbol = _SYMBOL,
                    SymbolDecimals = 2,
                    SymbolMultiplier = 1
                });
                db.SaveChangesAsync();
            }*/
        }
    }
}
