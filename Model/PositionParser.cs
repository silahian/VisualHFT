using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hftWPFDashboard.Model;

namespace hftWPFDashboard.Classes
{
    public class PositionParser
    {
        public static List<Position> Parse(List<GetTodaysSessionTrades_Result> dbTrades)
        {
            List<Position> ret = new List<Position>();
            long positionID = 0;
            int _openExecutionID = 0;
            int _closeExecutionID = 0;
            Position pos = new Position();
            /*
            foreach(var r in dbTrades)
            {

                if (positionID == r.PositionID) //ID
                {
                    if (_openExecutionID != r["openExecutionID"].ToInt())
                    {
                        _openExecutionID = r["openExecutionID"].ToInt();
                        OrderExecutionReports er = new OrderExecutionReports();
                        er.ClOrdId = r["openClOrdId"].ToString();
                        er.ExecId = r["openExecId"].ToString();
                        er.LocalTimeStamp = (DateTime)r["openLocalTimeStamp"];
                        er.Price = r["openPrice"].ToDouble();
                        er.ProviderID = r["openProviderID"].ToInt();
                        er.QtyFilled = r["openQtyFilled"].ToInt();
                        er.ServerTimeStamp = (DateTime)r["openServerTimeStamp"];
                        er.Side = (ePOSITIONSIDE)r["openSide"].ToInt();
                        er.Status = (ePOSITIONSTATUS)r["openStatus"].ToInt();
                        er.isOpen = r["openisOpen"].ToBoolean();
                        if (pos.OpenExecutions == null)
                            pos.OpenExecutions = new List<OrderExecutionReports>();
                        pos.OpenExecutions.Add(er);
                    }
                    if (_closeExecutionID != r["closeExecutionID"].ToInt())
                    {
                        _closeExecutionID = r["closeExecutionID"].ToInt();
                        OrderExecutionReports er = new OrderExecutionReports();
                        er.ClOrdId = r["closeClOrdId"].ToString();
                        er.ExecId = r["closeExecId"].ToString();
                        er.LocalTimeStamp = (DateTime)r["closeLocalTimeStamp"];
                        er.Price = r["closePrice"].ToDouble();
                        er.ProviderID = r["closeProviderID"].ToInt();
                        er.QtyFilled = r["closeQtyFilled"].ToInt();
                        er.ServerTimeStamp = (DateTime)r["closeServerTimeStamp"];
                        er.Side = (ePOSITIONSIDE)r["closeSide"].ToInt();
                        er.Status = (ePOSITIONSTATUS)r["closeStatus"].ToInt();
                        er.isOpen = r["closeisOpen"].ToBoolean();
                        if (pos.CloseExecutions == null)
                            pos.CloseExecutions = new List<OrderExecutionReports>();
                        pos.CloseExecutions.Add(er);
                    }
                }
                else
                {
                    _ID = r.GetInt32(0);
                    _openExecutionID = 0;
                    _closeExecutionID = 0;
                    pos = new Position();
                    #region Fill Position Object
                    pos.ID = _ID;
                    pos.PositionID = Convert.ToInt64(r["PositionID"]);
                    pos.AttemptsToClose = r["AttemptsToClose"].ToInt();
                    pos.CloseAvgPrice = r["CloseAvgPrice"].ToDouble();
                    pos.CloseClOrdId = r["CloseClOrdId"].ToString();
                    pos.CloseProviderId = r["CloseProviderId"].ToInt();
                    pos.CloseQuoteID = r["CloseQuoteID"].ToInt();
                    pos.CloseQuoteLocalTimeStamp = (DateTime)r["CloseQuoteLocalTimeStamp"];
                    pos.CloseQuoteServerTimeStamp = (DateTime)r["CloseQuoteServerTimeStamp"];
                    pos.CloseStatus = (ePOSITIONSTATUS) r["CloseStatus"].ToInt();
                    pos.CloseTimeStamp = (DateTime)r["CloseTimeStamp"];
                    pos.CreationTimeStamp = (DateTime)r["CreationTimeStamp"];
                    pos.Currency = r["Currency"].ToString();
                    pos.FreeText = r["FreeText"].ToString();
                    pos.FutSettDate = r["FutSettDate"].ToString();
                    pos.GetCloseAvgPrice = r["GetCloseAvgPrice"].ToDouble();
                    pos.GetCloseQuantity = (uint)r["GetCloseQuantity"].ToInt();
                    pos.GetOpenAvgPrice = r["GetOpenAvgPrice"].ToDouble();
                    pos.GetOpenQuantity = (uint)r["GetOpenQuantity"].ToInt();
                    pos.GetPipsPnL = r["GetPipsPnL"].ToDouble();
                    pos.IsAlreadyReadAndSaved = r["IsAlreadyReadAndSaved"].ToBoolean();
                    pos.IsCloseMM = r["IsCloseMM"].ToBoolean();
                    pos.IsOpenMM = r["IsOpenMM"].ToBoolean();
                    pos.MaxDrowdown = r["MaxDrowdown"].ToDouble();
                    pos.OpenAvgPrice = r["OpenAvgPrice"].ToDouble();
                    pos.OpenClOrdId = r["OpenClOrdId"].ToString();
                    pos.OpenProviderId = r["OpenProviderId"].ToInt();
                    pos.OpenQuoteID = r["OpenQuoteID"].ToInt();
                    pos.OpenQuoteLocalTimeStamp = (DateTime)r["OpenQuoteLocalTimeStamp"];
                    pos.OpenQuoteServerTimeStamp = (DateTime)r["OpenQuoteServerTimeStamp"];
                    pos.OpenStatus = (ePOSITIONSTATUS)r["OpenStatus"].ToInt();
                    pos.OrderQuantity = r["OrderQuantity"].ToInt();
                    pos.PipsTrail = r["PipsTrail"].ToDouble();
                    pos.Side = (ePOSITIONSIDE)r["Side"].ToInt();
                    pos.StopLoss = r["StopLoss"].ToDouble();
                    pos.StrategyCode = r["StrategyCode"].ToString();
                    pos.Symbol = r["Symbol"].ToString();
                    pos.SymbolDecimals = r["SymbolDecimals"].ToInt();
                    pos.SymbolMultiplier = r["SymbolMultiplier"].ToInt();
                    pos.TakeProfit = r["TakeProfit"].ToDouble();
                    pos.UnrealizedPnL = r["UnrealizedPnL"].ToDouble();
                    pos.GetPipsPnLInCurrency = r["GetPipsPnLInCurrency"].ToDouble();
                    #endregion
                    ret.Add(pos);
                }
            }
            */


            return ret;
        }
    }
}
