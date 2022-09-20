using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Helpers
{
    public class HelperPositionAnalysis
    {
        public static List<cEquity> GetEquityCurve(List<PositionEx> aPositions)
        {
            if (aPositions == null || aPositions.Count == 0)
                return null;
            List<cEquity> dRet = new List<cEquity>();

			//MAKE cummulative
			decimal dEquity = 0; // aPositions.First().GetOpenAvgPrice * aPositions.First().GetOpenQuantity;
            foreach (var p in aPositions.Where(x => x.PipsPnLInCurrency.HasValue).OrderBy(x => x.CreationTimeStamp))
            {
                dEquity += p.PipsPnLInCurrency.Value;
                cEquity e = new cEquity();
                e.Date = p.CreationTimeStamp;
                e.Commission = 0;
                e.Equity = dEquity;
                e.VolumeQty = p.OrderQuantity;
                dRet.Add(e);
            }
            return dRet;
        }
        public static List<cEquity> GetEquityCurveByHour(List<PositionEx> aPositions)
        {
            var curve = GetEquityCurve(aPositions);
            var hourlyPL = from c in curve.OrderBy(x => x.Date)
                           group c by new { date = new DateTime(c.Date.Year, c.Date.Month, c.Date.Day).AddHours(c.Date.Hour) } into g
                           select new cEquity
                           {
                               Date = g.Key.date,
                               Equity = g.Last().Equity,
                               VolumeQty = g.Sum(x => x.VolumeQty)
                           };
            return hourlyPL.ToList();
        }
        public static List<cEquity> GetEquityCurveByDay(List<PositionEx> aSignal)
        {
            List<cEquity> aEquity = GetEquityCurve(aSignal);
            var hourly = from x in aEquity.OrderBy(x => x.Date)
                         group x by new { date = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day) } into g
                         select new cEquity()
                         {
                             Date = g.Key.date,
                             Commission = g.Sum(x => x.Commission),
                             Equity = g.Last().Equity,
                             VolumeQty = g.Sum(s => s.VolumeQty)
                         };
            return hourly.ToList();
        }



        public static List<cBalance> GetBalanceCurve(List<PositionEx> aPositions)
        {
            if (aPositions == null || aPositions.Count == 0)
                return null;
            List<cBalance> dRet = new List<cBalance>();            
            foreach (var p in aPositions.Where(x => x.PipsPnLInCurrency.HasValue).OrderBy(x => x.CreationTimeStamp))
            {
                //OPEN
                dRet.Add(new cBalance() { Date = p.CreationTimeStamp, Balance = -(p.GetOpenAvgPrice * p.GetOpenQuantity) });
                //CLOSE
                if (p.CloseTimeStamp > p.CreationTimeStamp)
                    dRet.Add(new cBalance() { Date = p.CloseTimeStamp, Balance = -(p.GetCloseAvgPrice * p.GetCloseQuantity) });
            }
            //MAKE IT cummulative
            decimal dBalance = 0;
            foreach(var b in dRet)
            {
                dBalance += b.Balance;
                b.Balance = dBalance;
            }
            return dRet;
        }
        public static List<cBalance> GetBalanceCurveByHour(List<PositionEx> aPositions)
        {
            var curve = GetBalanceCurve(aPositions);
            var hourlyPL = from c in curve.OrderBy(x => x.Date)
                           group c by new { date = new DateTime(c.Date.Year, c.Date.Month, c.Date.Day).AddHours(c.Date.Hour) } into g
                           select new cBalance
                           {
                               Date = g.Key.date,
                               Balance = g.Last().Balance
                           };
            return hourlyPL.ToList();
        }

    }
}
