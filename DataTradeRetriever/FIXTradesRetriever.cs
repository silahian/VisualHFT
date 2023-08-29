using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VisualHFT.Helpers;
using VisualHFT.Model;
using QuickFix.Fields;
using QuickFix.DataDictionary;
using System.Windows.Shapes;


namespace VisualHFT.DataTradeRetriever
{
    public class FIXTradesRetriever : IDataTradeRetriever, IDisposable
    {
        private const int POLLING_INTERVAL = 5000;
        private readonly System.Timers.Timer _timer;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private long _lastReadPosition = 0; // to keep track of where we left off reading the file
        private readonly string _logFilePath; // path to the QuickFIX log file
        private List<PositionEx> _positions;
        private List<OrderVM> _orders;
        int _providerId;
        string _providerName;
        DateTime? _sessionDate = null;

        private bool _disposed = false;

        public event EventHandler<IEnumerable<OrderVM>> OnInitialLoad;
        public event EventHandler<IEnumerable<OrderVM>> OnDataReceived;
        protected virtual void RaiseOnInitialLoad(IEnumerable<OrderVM> ord) => OnInitialLoad?.Invoke(this, ord);
        protected virtual void RaiseOnDataReceived(IEnumerable<OrderVM> ord) => OnDataReceived?.Invoke(this, ord);
        public FIXTradesRetriever(string logFilePath, int providerId, string providerName)
        {
            _positions = new List<PositionEx>();
            _orders = new List<OrderVM>();
            _logFilePath = logFilePath;
            _providerId = providerId;
            _providerName = providerName;
            _timer = new System.Timers.Timer(POLLING_INTERVAL);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _timer_Elapsed(null, null);
        }
        ~FIXTradesRetriever()
        {
            Dispose(false);
        }
        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop(); // Stop the timer while the operation is running
            if (_cancellationTokenSource.IsCancellationRequested) return; // Check for cancellation

            try
            {
                // Efficiently read new lines from the log file since the last read
                var newLines = await ReadNewLinesFromFileAsync(_logFilePath);
                if (newLines.Any())
                {
                    var parsedTrades = ParseTradesFromLogLines(newLines);
                    var symbols = parsedTrades.Select(x => x.Symbol).Distinct().ToList();
                    if (symbols.Any())
                    {
                        foreach( var symbol in symbols)
                        {
                            if (!HelperCommon.ALLSYMBOLS.Contains(symbol))
                            {
                                //this collection needs to be updated in the UI thread
                                App.Current.Dispatcher.Invoke(() => HelperCommon.ALLSYMBOLS.Add(symbol));
                            }
                        }
                    }

                    if (this.Orders == null || !this.Orders.Any())
                    {

                        _orders.AddRange(parsedTrades);
                        RaiseOnInitialLoad(_orders);
                    }
                    else
                    {
                        _orders.AddRange(parsedTrades);
                        RaiseOnDataReceived(parsedTrades);
                    }
                }
            }
            catch (Exception ex)
            {

                await Console.Out.WriteLineAsync(ex.Message);
            }
            _timer.Start(); // Restart the timer once the operation is complete
        }
        public DateTime? SessionDate
        {
            get { return _sessionDate; }
            set
            {
                if (value != _sessionDate)
                {
                    _sessionDate = value;
                    _orders.Clear();
                    _lastReadPosition = 0;
                    RaiseOnInitialLoad(this.Orders);
                }
            }
        }
        public ReadOnlyCollection<OrderVM> Orders
        {
            get { return _orders.AsReadOnly(); }
        }
        public ReadOnlyCollection<PositionEx> Positions
        {
            get { return _positions.AsReadOnly(); }
        }

        private IEnumerable<OrderVM> ParseTradesFromLogLines(IEnumerable<string> logLines)
        {
            StringBuilder _symbolBuilder = new StringBuilder(30);
            var parsedOrders = new Dictionary<string, OrderVM>();
            List<Dictionary<int, string>> _arr = new List<Dictionary<int, string>>();

            try
            {
                foreach (var line in logLines)
                {
                    if (line.IndexOf("35=A") > -1
                        || line.IndexOf("35=5") > -1
                        || line.IndexOf("35=G") > -1
                        || line.IndexOf("35=9") > -1
                        || line.IndexOf("35=0") > -1)
                        continue;

                    // Extract the actual FIX message (remove the "OUT" prefix and timestamp if they are not part of the message)
                    string actualFixMsg = line.Substring(line.IndexOf("8=FIX."));
                    _arr.Add(ParseFixMessage(actualFixMsg));
                }


                foreach (var dicFIX in _arr)
                {
                    if (dicFIX[Tags.MsgType] == MsgType.NEWORDERSINGLE)  // New Order
                    {
                        var order = new OrderVM();

                        order.CreationTimeStamp = dicFIX[Tags.SendingTime].ToDateTime();
                        order.ClOrdId = dicFIX[Tags.ClOrdID];
                        if (dicFIX.ContainsKey(Tags.OrdStatus))
                            order.Status = ParseOrderStatus(dicFIX[Tags.OrdStatus]);
                        else
                            order.Status = eORDERSTATUS.SENT; // ParseOrderStatus(dicFIX[Tags.OrdStatus));

                        _symbolBuilder.Clear();
                        if (dicFIX.ContainsKey(Tags.SecurityExchange) && dicFIX[Tags.SecurityExchange] == "CME")
                        {
                            //"CME;FUT;ES;201812" (normalization)
                            _symbolBuilder.Insert(0, dicFIX[Tags.SecurityExchange]);
                            if (dicFIX.ContainsKey(Tags.SecurityType))
                            {
                                _symbolBuilder.Append(";");
                                _symbolBuilder.Append(dicFIX[Tags.SecurityType]);
                            }
                            _symbolBuilder.Append(";");
                            _symbolBuilder.Append(dicFIX[Tags.Symbol]);

                            if (dicFIX.ContainsKey(Tags.MaturityMonthYear))
                            {
                                _symbolBuilder.Append(";");
                                _symbolBuilder.Append(dicFIX[Tags.MaturityMonthYear]);
                            }
                        }
                        order.Symbol = _symbolBuilder.ToString();

                        order.Side = dicFIX[Tags.Side] == "2" ? eORDERSIDE.Sell : eORDERSIDE.Buy;
                        order.Quantity = dicFIX[Tags.OrderQty].ToDouble();
                        order.PricePlaced = (double)dicFIX[Tags.Price].ToDouble();
                        order.TimeInForce = ParseOrderTIF(dicFIX[Tags.TimeInForce]);
                        order.OrderType = ParseOrderType(dicFIX[Tags.OrdType]);
                        order.ProviderId = _providerId;
                        order.ProviderName = _providerName;
                        order.Executions = new List<ExecutionVM>();
                        order.LastUpdated = System.DateTime.Now;
                        parsedOrders.Add(order.ClOrdId, order);
                    }
                    else if (dicFIX[Tags.MsgType] == MsgType.ORDER_CANCEL_REPLACE_REQUEST)
                    {
                        //REPLACE/CANCEL SENT
                        string _clordId = dicFIX[Tags.ClOrdID];
                        string _orig_clordId = dicFIX[Tags.OrigClOrdID];

                        OrderVM order = null;
                        if (parsedOrders.TryGetValue(_orig_clordId, out order))
                        {
                            var exec = new OpenExecution() { Price = 0, QtyFilled = 0 };
                            if (dicFIX.ContainsKey(Tags.ExecID))
                                exec.ExecID = dicFIX[Tags.ExecID];
                            exec.ClOrdId = _clordId;
                            exec.LocalTimeStamp = dicFIX[Tags.SendingTime].ToDateTime();
                            exec.ServerTimeStamp = exec.LocalTimeStamp;
                            exec.ProviderID = _providerId;
                            exec.Status = (int)eORDERSTATUS.REPLACESENT;
                            exec.Side = (int)order.Side;
                            exec.Price = 0;
                            exec.QtyFilled = 0;
                            order.LastUpdated = System.DateTime.Now;
                            order.Executions.Add(new ExecutionVM(exec, order.Symbol));
                        }
                        else
                        {
                            throw new Exception("Order not found = " + _clordId);
                        }
                    }
                    else if (dicFIX[Tags.MsgType] == MsgType.EXECUTION_REPORT)  // Execution Report
                    {
                        string _clordId = dicFIX[Tags.ClOrdID];
                        string _orig_clordId = "";
                        if (dicFIX.ContainsKey(Tags.OrigClOrdID))
                            _orig_clordId = dicFIX[Tags.OrigClOrdID];

                        OrderVM order = null;
                        if (parsedOrders.TryGetValue(_orig_clordId == "" ? _clordId : _orig_clordId, out order))
                        {
                            var exec = new OpenExecution() { Price = 0, QtyFilled = 0 };
                            if (dicFIX.ContainsKey(Tags.ExecID))
                                exec.ExecID = dicFIX[Tags.ExecID];
                            exec.ClOrdId = _clordId;
                            exec.LocalTimeStamp = dicFIX[Tags.SendingTime].ToDateTime();
                            exec.ServerTimeStamp = exec.LocalTimeStamp;
                            exec.ProviderID = _providerId;
                            if (dicFIX.ContainsKey(Tags.LastPx))
                                exec.Price = dicFIX[Tags.LastPx].ToDecimal();
                            if (dicFIX.ContainsKey(Tags.LastShares))
                                exec.QtyFilled = dicFIX[Tags.LastShares].ToDecimal();
                            exec.Side = (int)order.Side;
                            exec.Status = (int)ParseOrderStatus(dicFIX[Tags.OrdStatus]);

                            order.Executions.Add(new ExecutionVM(exec, order.Symbol));

                            order.Status = ParseOrderStatus(dicFIX[Tags.OrdStatus]);
                            order.LastUpdated = System.DateTime.Now;

                            if (order.Status == eORDERSTATUS.REPLACED)
                            {                                
                                order.ClOrdId = _clordId;
                                parsedOrders.Add(_clordId, order);
                                parsedOrders.Remove(_orig_clordId);
                            }
                            else if (exec.Status == (int)eORDERSTATUS.FILLED || exec.Status == (int)eORDERSTATUS.CANCELED)
                            {
                                var execFilled = order.Executions.Where(x => x.Status == ePOSITIONSTATUS.FILLED || x.Status == ePOSITIONSTATUS.PARTIALFILLED);
                                if (execFilled != null && execFilled.Any())
                                {
                                    order.GetAvgPrice = (double)execFilled.Average(x => x.Price);
                                    order.FilledQuantity = (double)execFilled.Average(x => x.QtyFilled);
                                    order.GetQuantity = order.FilledQuantity;
                                    order.FilledPercentage = 100 * (order.FilledQuantity / order.Quantity);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Order not found = " + _clordId);
                        }
                    }
                    else
                    {
                        throw new Exception("Message not implemented = " + dicFIX[Tags.MsgType]);
                    }
                    // Handle other message types (e.g., cancels, rejects) similarly...
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Error: {ex.Message}");
            }

            return parsedOrders.Select(x => x.Value).ToList();
        }

        private async Task<List<string>> ReadNewLinesFromFileAsync(string filePath)
        {
            const int bufferSize = 8192; // 8 KB
            char[] buffer = new char[bufferSize];
            StringBuilder sb = new StringBuilder();
            List<string> newLines = new List<string>();  

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                fs.Seek(_lastReadPosition, SeekOrigin.Begin);

                while (!sr.EndOfStream)
                {
                    int bytesRead = await sr.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);

                    for (int i = 0; i < bytesRead; i++)
                    {
                        if (buffer[i] == '\n')
                        {
                            newLines.Add(sb.ToString());
                            sb.Clear();
                        }
                        else if (buffer[i] != '\r') // Skip '\r'
                        {
                            sb.Append(buffer[i]);
                        }
                    }
                }

                _lastReadPosition = fs.Position;
            }

            // Add any remaining text
            if (sb.Length > 0)
            {
                newLines.Add(sb.ToString());
            }

            return newLines;
        }

        private eORDERSTATUS ParseOrderStatus(string fixStatus)
        {
            // Convert the FIX status to your application's order status
            switch (fixStatus[0])
            {
                case OrdStatus.NEW:                 return eORDERSTATUS.NEW;
                case OrdStatus.PARTIALLY_FILLED:    return eORDERSTATUS.PARTIALFILLED;
                case OrdStatus.FILLED:              return eORDERSTATUS.FILLED;
                case OrdStatus.CANCELED:            return eORDERSTATUS.CANCELED;
                case OrdStatus.REJECTED:            return eORDERSTATUS.REJECTED;
                case OrdStatus.REPLACED:            return eORDERSTATUS.REPLACED;
                case OrdStatus.EXPIRED:             return eORDERSTATUS.REJECTED;
                case OrdStatus.PENDING_REPLACE:     return eORDERSTATUS.REPLACESENT;
                case OrdStatus.PENDING_CANCEL:      return eORDERSTATUS.CANCELEDSENT;
                default:
                    return eORDERSTATUS.NONE;
            }
        }
        private eORDERTYPE ParseOrderType(string fixOrdType)
        {
            switch(fixOrdType[0])
            {
                case OrdType.MARKET: return eORDERTYPE.MARKET;
                case OrdType.LIMIT: return eORDERTYPE.LIMIT;
                default: return eORDERTYPE.NONE;
            }
        }
        private eORDERTIMEINFORCE ParseOrderTIF(string fixTIF)
        {
            switch(fixTIF[0])
            {
                case QuickFix.Fields.TimeInForce.IMMEDIATE_OR_CANCEL: return eORDERTIMEINFORCE.IOC;
                case QuickFix.Fields.TimeInForce.FILL_OR_KILL: return eORDERTIMEINFORCE.FOK;
                case QuickFix.Fields.TimeInForce.GOOD_TILL_DATE: return eORDERTIMEINFORCE.GTC;
                default: return eORDERTIMEINFORCE.GTC;
            }
        }
        private Dictionary<int, string> ParseFixMessage(string fixMessage)
        {
            Dictionary<int, string> parsedMessage = new Dictionary<int, string>();
            int start = 0;
            int end = 0;

            while ((end = fixMessage.IndexOf('\u0001', start)) >= 0)
            {
                string field = fixMessage.Substring(start, end - start);
                int equalsPos = field.IndexOf('=');
                if (equalsPos > 0)
                {
                    int tag = field.Substring(0, equalsPos).ToInt();
                    string value = field.Substring(equalsPos + 1);
                    parsedMessage[tag] = value;
                }
                start = end + 1;
            }

            return parsedMessage;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timer.Elapsed -= _timer_Elapsed;
                    _timer?.Stop();
                    _timer?.Dispose();
                    _cancellationTokenSource?.Cancel(); // Cancel any ongoing operations
                    _cancellationTokenSource?.Dispose();
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
