using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualHFT.Helpers
{
    public class HelperAnalytics
    {
        public static List<cEquity> GetEquityCurve(List<VisualHFT.Model.Position> aSignal)
        {
            return Helpers.HelperPositionAnalysis.GetEquityCurve(aSignal);
        }
        public static List<cEquity> GetEquityCurveByHour(List<VisualHFT.Model.Position> aSignal)
        {
            return Helpers.HelperPositionAnalysis.GetEquityCurveByHour(aSignal);
        }
        public static List<cEquity> GetEquityCurveByDay(List<VisualHFT.Model.Position> aSignal)
        {
            return Helpers.HelperPositionAnalysis.GetEquityCurveByDay(aSignal);
        }
        public static List<cBalance> GetBalanceCurve(List<VisualHFT.Model.Position> aSignal)
        {
            List<cBalance> aBalance = Helpers.HelperPositionAnalysis.GetBalanceCurve(aSignal);
            return aBalance;
        }
        public static List<KeyValuePair<decimal, VisualHFT.Model.Position>> GetMaximumAdversExcursion(List<VisualHFT.Model.Position> aSignal)
        {
            List<KeyValuePair<decimal, VisualHFT.Model.Position>> aRet = new List<KeyValuePair<decimal, VisualHFT.Model.Position>>();
            foreach (VisualHFT.Model.Position s in aSignal)
            {
                aRet.Add(new KeyValuePair<decimal, VisualHFT.Model.Position>(s.MaxDrowdown, s));
            }
            return aRet;
        }
        public static List<KeyValuePair<decimal, VisualHFT.Model.Position>> GetMaximumFavorableExcursion(List<VisualHFT.Model.Position> aSignal)
        {
            List<KeyValuePair<decimal, VisualHFT.Model.Position>> aRet = new List<KeyValuePair<decimal, VisualHFT.Model.Position>>();
            foreach (VisualHFT.Model.Position s in aSignal)
            {
                aRet.Add(new KeyValuePair<decimal, VisualHFT.Model.Position>(s.UnrealizedPnL, s));
            }
            return aRet;
        }

        public static double GetMaximumDrawdownPerc(List<VisualHFT.Model.Position> aSignal, bool calculateIntraday = true)
        {
            var drawDowns = GetDrawdowns(aSignal, calculateIntraday);
            if (drawDowns != null && drawDowns.Any())
            {
                return (double)drawDowns.Max(x => x.DrawDownPerc);
            }
            else
            {
                return 0;
            }
        }
        public static List<cDrawDown> GetDrawdowns(List<VisualHFT.Model.Position> aSignal, bool calculateIntraday = true)
        {
            List<cEquity> aTransactions = null;
			if (calculateIntraday)
			{
				aTransactions = GetEquityCurveByHour(aSignal);//must be by hour/day
				var chunks = aTransactions.GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day)).Select(x => new
				{
					Date = x.Key,
					Transactions = x.ToList()
				});
				List<cDrawDown> aDrawDowns = new List<cDrawDown>();
				if (chunks.Any())
				{
					foreach(var c in chunks)
					{
						var res = GetDrawdownsChunk(c.Transactions);
						if (res != null && res.Any())
							aDrawDowns.AddRange(res);
					}
				}
				return aDrawDowns;
			}
			else
			{
				aTransactions = GetEquityCurveByDay(aSignal);
				return GetDrawdownsChunk(aTransactions);
			}

        }
		private static List<cDrawDown> GetDrawdownsChunk(List<cEquity> aTransactions)
		{
			List<cDrawDown> aDrawDowns = new List<cDrawDown>();

			if (aTransactions != null && aTransactions.Count > 0)
			{
				foreach (cEquity c in aTransactions)
				{
					cDrawDown d = new cDrawDown();
					cEquity cNextHigher = aTransactions.Where(x => x.Date > c.Date).SkipWhile(x => x.Equity < c.Equity).FirstOrDefault();
					if (cNextHigher == null) //last has been reach with no new high
						cNextHigher = aTransactions.Last();

					cEquity cDrawDown = cEquity.GetMinBetween2Points(c, cNextHigher, aTransactions);
					if (cDrawDown != null && cDrawDown != c && cDrawDown != cNextHigher)
					{
						d.Date = cDrawDown.Date;
						d.DrawDownAmmount = Math.Abs(cDrawDown.Equity - c.Equity);
						d.DrawDownPerc = d.DrawDownAmmount / Math.Abs(c.Equity);
						d.DrawDownHours = cDrawDown.Date.Subtract(c.Date).TotalHours.ToInt();
						aDrawDowns.Add(d);
					}
				}
			}
			return aDrawDowns;
		}

		public static List<KeyValuePair<int, List<cEquity>>> GetStagnationsInHours(List<VisualHFT.Model.Position> aSignal)
        {
            List<cEquity> aTransactions = GetEquityCurveByHour(aSignal);
            List<KeyValuePair<int, List<cEquity>>> aStagnations = new List<KeyValuePair<int, List<cEquity>>>();
            if (aTransactions != null && aTransactions.Count > 0)
            {
                foreach (cEquity c in aTransactions)
                {
                    cEquity cNextHigher = aTransactions.Where(x => x.Date > c.Date).OrderBy(x => x.Date).SkipWhile(x => x.Equity <= c.Equity).FirstOrDefault();
                    if (cNextHigher != null)
                    {
                        int iHours = cNextHigher.Date.Subtract(c.Date).TotalHours.ToInt();
                        List<cEquity> stagnationPeriod = new List<cEquity>();
                        stagnationPeriod.Add(c); stagnationPeriod.Add(cNextHigher);
                        aStagnations.Add(new KeyValuePair<int, List<cEquity>>(iHours, stagnationPeriod));
                    }
                }
            }
            return aStagnations;
        }
        public static List<KeyValuePair<int, List<cEquity>>> GetStagnationsInMinutes(List<VisualHFT.Model.Position> aSignal)
        {
            List<cEquity> aTransactions = GetEquityCurve(aSignal);
            List<KeyValuePair<int, List<cEquity>>> aStagnations = new List<KeyValuePair<int, List<cEquity>>>();
            if (aTransactions != null && aTransactions.Count > 0)
            {
                foreach (cEquity c in aTransactions)
                {
                    cEquity cNextHigher = aTransactions.Where(x => x.Date > c.Date).OrderBy(x => x.Date).SkipWhile(x => x.Equity <= c.Equity).FirstOrDefault();
                    if (cNextHigher != null)
                    {
                        int iMinutes = cNextHigher.Date.Subtract(c.Date).TotalMinutes.ToInt();
                        List<cEquity> stagnationPeriod = new List<cEquity>();
                        stagnationPeriod.Add(c); stagnationPeriod.Add(cNextHigher);
                        aStagnations.Add(new KeyValuePair<int, List<cEquity>>(iMinutes, stagnationPeriod));
                    }
                }
            }
            return aStagnations;
        }

        public static double GetSharpeRatio(List<VisualHFT.Model.Position> aSignal)
        {
            List<cEquity> aEquity = GetEquityCurveByDay(aSignal);
            List<double> aPLs = new List<double>();
            cEquity prevItem = null;
            foreach (var eq in aEquity)
            {
                if (prevItem != null)
                    aPLs.Add((double)((eq.Equity - prevItem.Equity) / prevItem.Equity));
                prevItem = eq;
            }
            double riskFreeRate = 0.2 / 100;

            decimal totReturn = (aEquity.Last().Equity - aEquity.First().Equity) / aEquity.First().Equity;
            double dSTD = aPLs.StdDev(); //groupped monthly
            //return Math.Sqrt(aPLs.Count()) * (((double)totReturn - riskFreeRate) / dSTD); //SQRT of periods
            if (dSTD > 0)
                return (((double)totReturn - riskFreeRate) / dSTD);
            else
                return 0;
        }
        public static double GetIntradaySharpeRatio(List<VisualHFT.Model.Position> aSignal)
        {
            double qtyTrades = aSignal.Count();
            double meanPnL = aSignal.Average(x => (double)x.PipsPnLInCurrency.Value);
            double stdev = aSignal.Select(x => (double)x.PipsPnLInCurrency.Value).StdDev();

            double SHARPE = Math.Sqrt(qtyTrades) * (meanPnL / stdev);
            return SHARPE;
        }
        public static double GetAHPR(List<VisualHFT.Model.Position> aSignal)
        {
			var equity = GetEquityCurveByHour(aSignal);
			double aHPR = (double)((equity.Last().Equity - equity.First().Equity) / equity.First().Equity);
			return aHPR;
		}
        public static double GetCAGR(List<VisualHFT.Model.Position> aSignal)
        {
            List<cEquity> yearlyEquity = GetEquityCurve(aSignal);
            if (yearlyEquity != null)
            {
                double iniAmmount = 0;
                double endAmmount = (double)yearlyEquity.Where(x => x.Equity != 0).Last().Equity;
                int numYears = yearlyEquity.Count;
                return Math.Pow((endAmmount / iniAmmount), (1.00 / numYears)) - 1;
            }
            return double.NaN;
        }
        public static double GetZScore(List<VisualHFT.Model.Position> aSignal)
        {
            /*
             * METHOD from 
             * http://www.wisestocktrader.com/indicators/1784-z-score-for-backtesting
            */
            double winCount = aSignal.Where(s => s.PipsPnLInCurrency.Value >= 0).Count();
            double lossCount = aSignal.Where(s => s.PipsPnLInCurrency.Value < 0).Count();
            double totalRuns = 0, w = 0, l = 0;

            foreach (Position s in aSignal)
            {
                if (s.GetPipsPnL > 0)
                {
                    if (w == 0) totalRuns++;
                    w++;
                    l = 0;
                }
                else if (s.GetPipsPnL < 0)
                {
                    if (l == 0) totalRuns++;
                    l++;
                    w = 0;
                }
            }

            double N = aSignal.Count;
            double R = totalRuns;
            double WIN = winCount;
            double LOSS = lossCount;
            double X = 2 * WIN * LOSS;
            double Z_Score = (N * (R - 0.5) - X) / Math.Sqrt((X * (X - N)) / (N - 1));
            return Z_Score;
        }
        public static double GetZProbability(List<VisualHFT.Model.Position> aSignal)
        {
            double Z_MAX = 6;
            double z = Math.Abs(GetZScore(aSignal));
            if (z < -Z_MAX)
                z = -Z_MAX;
            if (z > Z_MAX)
                z = Z_MAX;

            double y, x, w;
            if (z == 0.0)
            {
                x = 0.0;
            }
            else
            {
                y = 0.5 * Math.Abs(z);
                if (y > (Z_MAX * 0.5))
                {
                    x = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    x = ((((((((0.000124818987 * w
                        - 0.001075204047) * w + 0.005198775019) * w
                        - 0.019198292004) * w + 0.059054035642) * w
                        - 0.151968751364) * w + 0.319152932694) * w
                        - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y -= 2.0;
                    x = (((((((((((((-0.000045255659 * y
                        + 0.000152529290) * y - 0.000019538132) * y
                        - 0.000676904986) * y + 0.001390604284) * y
                        - 0.000794620820) * y - 0.002034254874) * y
                        + 0.006549791214) * y - 0.010557625006) * y
                        + 0.011630447319) * y - 0.009279453341) * y
                        + 0.005353579108) * y - 0.002141268741) * y
                        + 0.000535310849) * y + 0.999936657524;
                }
            }
            double lp = z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5);
            double rp = 1 - lp;
            double tp = 2 * rp;

            return Math.Abs(1 - tp);
        }
        public static List<int> GetConsecutiveWins(List<VisualHFT.Model.Position> aSignal)
        {
            List<int> aRet = new List<int>();
            int iCount = 0;
            bool lastWasWin = false;
            foreach (Position s in aSignal)
            {
                if (lastWasWin && s.PipsPnLInCurrency.Value >= 0)
                    iCount++;
                else
                {
                    if (iCount > 0)
                        aRet.Add(iCount);
                    iCount = 0;
                }
                lastWasWin = s.PipsPnLInCurrency.Value >= 0;
            }
            return aRet;
        }
        public static List<int> GetConsecutiveLosses(List<VisualHFT.Model.Position> aSignal)
        {
            List<int> aRet = new List<int>();
            int iCount = 0;
            bool lastWasLoss = false;
            foreach (Position s in aSignal)
            {
                if (lastWasLoss && s.PipsPnLInCurrency.Value < 0)
                    iCount++;
                else
                {
                    if (iCount > 0)
                        aRet.Add(iCount);
                    iCount = 0;
                }
                lastWasLoss = s.PipsPnLInCurrency.Value < 0;
            }
            return aRet;
        }
        public static double GetExpectancy(List<VisualHFT.Model.Position> aSignal)
        {
			//(Average Winner x Win Rate) – (Average Loser x Loss Rate)
			var avgWinner = (double)aSignal.Where(x => x.PipsPnLInCurrency.HasValue && x.PipsPnLInCurrency.Value >= 0).DefaultIfEmpty(new VisualHFT.Model.Position() { PipsPnLInCurrency = 0 }).Average(x => x.PipsPnLInCurrency.Value);
            var avgLosser = (double) aSignal.Where(x => x.PipsPnLInCurrency.HasValue && x.PipsPnLInCurrency.Value < 0).DefaultIfEmpty(new VisualHFT.Model.Position() { PipsPnLInCurrency = 0 }).Average(x => Math.Abs(x.PipsPnLInCurrency.Value));
            var winRate = (double)aSignal.Where(x => x.PipsPnLInCurrency.HasValue && x.PipsPnLInCurrency.Value >= 0).Count() / aSignal.Count();
            var lossRate = (double)aSignal.Where(x => x.PipsPnLInCurrency.HasValue && x.PipsPnLInCurrency.Value < 0).Count() / aSignal.Count();

            return (avgWinner * winRate) - (avgLosser * lossRate);
        }        

		public static cEquity GetAverageProfitByHour(List<VisualHFT.Model.Position> aSignal)
		{
			List<cEquity> hourlyEquity = HelperAnalytics.GetEquityCurveByHour(aSignal);
			double avgHourlyPnL = 0;
			decimal lastEquity = 0;
			foreach (var e in hourlyEquity)
			{
				if (lastEquity != 0)
				{
					avgHourlyPnL += (double)((e.Equity - lastEquity) / Math.Abs(lastEquity));
				}
				lastEquity = e.Equity;
			}
			avgHourlyPnL = avgHourlyPnL / (hourlyEquity.Count - 1.0);

			return new cEquity()
			{
				Equity = (decimal)avgHourlyPnL,
				VolumeQty = hourlyEquity.Average(x => x.VolumeQty)
			};

		}
		public static cEquity GetAverageProfitByDay(List<VisualHFT.Model.Position> aSignal)
		{
			List<cEquity> hourlyEquity = HelperAnalytics.GetEquityCurveByDay(aSignal);
			double avgHourlyPnL = 0;
			decimal lastEquity = 0;
			foreach (var e in hourlyEquity)
			{
				if (lastEquity != 0)
				{
					avgHourlyPnL += (double)((e.Equity - lastEquity) / Math.Abs(lastEquity));
				}
				lastEquity = e.Equity;
			}
            if (hourlyEquity.Count > 1)
			    avgHourlyPnL = avgHourlyPnL / (hourlyEquity.Count - 1.0);

			return new cEquity()
			{
				Equity = (decimal)avgHourlyPnL,
				VolumeQty = hourlyEquity.Average(x => x.VolumeQty)
			};

		}



	}
}
