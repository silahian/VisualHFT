using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;
using OxyPlot;
using OxyPlot.Axes;
using VisualHFT.Commons.Pools;
using AxisPosition = OxyPlot.Axes.AxisPosition;
using Prism.Mvvm;
using VisualHFT.Commons.Helpers;
using VisualHFT.Enums;


namespace VisualHFT.ViewModel
{
    public class vmOrderBook : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private int _MAX_CHART_POINTS = 1300;
        private TimeSpan _MIN_UI_REFRESH_TS = TimeSpan.FromMilliseconds(300); //For the UI: do not allow less than this, since it is not noticeble for human eye

        private object MTX_PLOTS = new object();
        private object MTX_ORDERBOOK = new object();
        private object MTX_TRADES = new object();

        private Dictionary<string, Func<string, string, bool>> _dialogs;
        protected ObservableCollection<string> _symbols;
        private string _selectedSymbol;
        private VisualHFT.ViewModel.Model.Provider _selectedProvider = null;
        private AggregationLevel _aggregationLevelSelection;

        private CachedCollection<BookItem> _bidsGrid;
        private CachedCollection<BookItem> _asksGrid;
        private CachedCollection<BookItem> _depthGrid;

        private ObservableCollection<VisualHFT.ViewModel.Model.Provider> _providers;
        private double _maxOrderSize = 0;
        private double _minOrderSize = 0;

        private BookItem _AskTOB = new BookItem();
        private BookItem _BidTOB = new BookItem();
        private double _MidPoint;
        private double _Spread;
        private int _decimalPlaces;


        private Model.BookItemPriceSplit _BidTOB_SPLIT = null;
        private Model.BookItemPriceSplit _AskTOB_SPLIT = null;

        private double _lobImbalanceValue = 0;
        private int _switchView = 0;

        private UIUpdater uiUpdater;

        private Stack<VisualHFT.Model.Trade> _realTimeTrades;

        private VisualHFT.Commons.Pools.CustomObjectPool<OrderBookData> _objectPool_OrderBook;
        private VisualHFT.Commons.Pools.CustomObjectPool<BookItem> _objectPool_BookItem;
        private VisualHFT.Commons.Pools.CustomObjectPool<PlotInfoPriceChart> _objectPool_Price;
        private VisualHFT.Commons.Pools.CustomObjectPool<OrderBookLevel> _objectPool_OrderBookLevel;
        private HelperCustomQueue<OrderBookData> _QUEUE;
        private AggregatedCollection<PlotInfoPriceChart> _AGG_DATA;


        private bool _MARKETDATA_AVAILABLE = false;
        private bool _TRADEDATA_AVAILABLE = false;


        public vmOrderBook(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            RealTimePricePlotModel = new PlotModel();
            RealTimeSpreadModel = new PlotModel();
            CummulativeBidsChartModel = new PlotModel();
            CummulativeAsksChartModel = new PlotModel();

            _objectPool_OrderBook = new CustomObjectPool<OrderBookData>(_MAX_CHART_POINTS * 1000);
            _objectPool_BookItem = new CustomObjectPool<BookItem>(_MAX_CHART_POINTS * 1000);
            _objectPool_Price = new CustomObjectPool<PlotInfoPriceChart>(_MAX_CHART_POINTS * 10);
            _objectPool_OrderBookLevel = new CustomObjectPool<OrderBookLevel>(_MAX_CHART_POINTS * 1000);
            _QUEUE = new HelperCustomQueue<OrderBookData>($"<OrderBookData>_vmOrderBook", QUEUE_onReadAction, QUEUE_onErrorAction);


            _realTimeTrades = new Stack<VisualHFT.Model.Trade>();
            TradesDisplay = new ObservableCollection<Trade>();

            InitializeRealTimePriceChart();
            InitializeRealTimeSpreadChart();
            InitializeCummulativeCharts();

            _bidsGrid = new CachedCollection<BookItem>((x, y) => y.Price.GetValueOrDefault().CompareTo(x.Price.GetValueOrDefault()));
            _asksGrid = new CachedCollection<BookItem>((x, y) => x.Price.GetValueOrDefault().CompareTo(y.Price.GetValueOrDefault()));
            _depthGrid = new CachedCollection<BookItem>((x, y) => y.Price.GetValueOrDefault().CompareTo(x.Price.GetValueOrDefault()));

            _symbols = new ObservableCollection<string>(HelperSymbol.Instance);
            _providers = VisualHFT.ViewModel.Model.Provider.CreateObservableCollection();
            AggregationLevels = new ObservableCollection<Tuple<string, AggregationLevel>>();
            foreach (AggregationLevel level in Enum.GetValues(typeof(AggregationLevel)))
            {
                if (level >= AggregationLevel.Ms100) //do not load less than 100ms. In order to save resources, we cannot go lower than 100ms (//TODO: in the future we must include lower aggregation levels)
                    AggregationLevels.Add(new Tuple<string, AggregationLevel>(Helpers.HelperCommon.GetEnumDescription(level), level));
            }
            _aggregationLevelSelection = AggregationLevel.Ms100; //DEFAULT
            uiUpdater = new UIUpdater(uiUpdaterAction, _aggregationLevelSelection.ToTimeSpan().TotalMilliseconds);
            _AGG_DATA = new AggregatedCollection<PlotInfoPriceChart>(_aggregationLevelSelection, _MAX_CHART_POINTS,
                x => x.Date, _AGG_DATA_OnAggregating);
            _AGG_DATA.OnRemoving += _AGG_DATA_OnRemoving;

            HelperSymbol.Instance.OnCollectionChanged += ALLSYMBOLS_CollectionChanged;
            HelperProvider.Instance.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperProvider.Instance.OnStatusChanged += PROVIDERS_OnStatusChanged;

            HelperTrade.Instance.Subscribe(TRADES_OnDataReceived);
            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);


            _BidTOB_SPLIT = new Model.BookItemPriceSplit();
            _AskTOB_SPLIT = new Model.BookItemPriceSplit();

            RaisePropertyChanged(nameof(Providers));
            RaisePropertyChanged(nameof(BidTOB_SPLIT));
            RaisePropertyChanged(nameof(AskTOB_SPLIT));
            RaisePropertyChanged(nameof(TradesDisplay));

            SwitchView = 0;


        }

        ~vmOrderBook()
        {
            Dispose(false);
        }

        private void InitializeRealTimePriceChart()
        {
            RealTimePricePlotModel.DefaultFontSize = 8.0;
            RealTimePricePlotModel.Title = "";
            RealTimePricePlotModel.TitleColor = OxyColors.White;
            RealTimePricePlotModel.PlotAreaBorderColor = OxyColors.White;
            RealTimePricePlotModel.PlotAreaBorderThickness = new OxyThickness(0);

            var xAxis = new OxyPlot.Axes.DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss", // Format time as hours:minutes:seconds
                IntervalType = DateTimeIntervalType.Auto, // Automatically determine the appropriate interval type (seconds, minutes, hours)
                MinorIntervalType = DateTimeIntervalType.Auto, // Automatically determine the appropriate minor interval type
                IntervalLength = 80, // Determines how much space each interval takes up, adjust as necessary
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineStyle = LineStyle.Solid,
                IsAxisVisible = false,
                IsPanEnabled = false,
                IsZoomEnabled = false,
            };

            var yAxis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                StringFormat = "N",

                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                IsAxisVisible = true
            };

            // Add a color axis to map quantity to color
            var RedColorAxis = new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Palette = OxyPalette.Interpolate(10, OxyColors.Pink, OxyColors.DarkRed),
                Minimum = 1,
                Maximum = 100,
                Key = "RedColorAxis",
                IsAxisVisible = false
            };

            var GreenColorAxis = new LinearColorAxis
            {
                Position = AxisPosition.Right,
                Palette = OxyPalette.Interpolate(10, OxyColors.LightGreen, OxyColors.DarkGreen),
                Minimum = 1,
                Maximum = 100,
                Key = "GreenColorAxis",
                IsAxisVisible = false
            };


            RealTimePricePlotModel.Axes.Add(xAxis);
            RealTimePricePlotModel.Axes.Add(yAxis);
            RealTimePricePlotModel.Axes.Add(RedColorAxis);
            RealTimePricePlotModel.Axes.Add(GreenColorAxis);


            //Add MID-PRICE Serie
            var lineMidPrice = new OxyPlot.Series.LineSeries
            {
                Title = "MidPrice",
                MarkerType = MarkerType.None,
                StrokeThickness = 2,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Gray,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,

            };
            var lineAsk = new OxyPlot.Series.LineSeries
            {
                Title = "Ask",
                MarkerType = MarkerType.None,
                StrokeThickness = 6,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Red,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
            };
            var lineBid = new OxyPlot.Series.LineSeries
            {
                Title = "Bid",
                MarkerType = MarkerType.None,
                StrokeThickness = 6,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Green,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
            };
            //SCATTER SERIES
            var scatterAsks = new OxyPlot.Series.ScatterSeries
            {
                Title = "ScatterAsks",
                ColorAxisKey = "RedColorAxis",
                MarkerType = MarkerType.Square,
                MarkerStrokeThickness = 0,
                //MarkerStroke = OxyColors.DarkRed,
                MarkerStroke = OxyColors.Transparent,
                //MarkerFill = OxyColor.Parse("#80FF0000"),
                MarkerSize = 10,
                RenderInLegend = false,
                Selectable = false,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,

                BinSize = 15 //smoothing the draw for speed performance
            };
            var scatterBids = new OxyPlot.Series.ScatterSeries
            {
                Title = "ScatterBids",
                ColorAxisKey = "GreenColorAxis",
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.Transparent,
                //MarkerStroke = OxyColors.Green,
                MarkerStrokeThickness = 0,
                //MarkerFill = OxyColor.Parse("#8000FF00"),
                MarkerSize = 10,
                RenderInLegend = false,
                Selectable = false,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
                BinSize = 15 //smoothing the draw for speed performance
            };


            // do not change the order of adding these series (The overlap between them will depend on the order they have been added)
            RealTimePricePlotModel.Series.Add(scatterBids);
            RealTimePricePlotModel.Series.Add(scatterAsks);
            RealTimePricePlotModel.Series.Add(lineMidPrice);
            RealTimePricePlotModel.Series.Add(lineAsk);
            RealTimePricePlotModel.Series.Add(lineBid);

        }
        private void InitializeRealTimeSpreadChart()
        {
            RealTimeSpreadModel.DefaultFontSize = 8.0;
            RealTimeSpreadModel.Title = "";
            RealTimeSpreadModel.TitleColor = OxyColors.White;
            RealTimeSpreadModel.PlotAreaBorderColor = OxyColors.White;
            RealTimeSpreadModel.PlotAreaBorderThickness = new OxyThickness(0);

            var xAxis = new OxyPlot.Axes.DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss", // Format time as hours:minutes:seconds
                IntervalType = DateTimeIntervalType.Auto, // Automatically determine the appropriate interval type (seconds, minutes, hours)
                MinorIntervalType = DateTimeIntervalType.Auto, // Automatically determine the appropriate minor interval type
                IntervalLength = 80, // Determines how much space each interval takes up, adjust as necessary
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false,
            };

            var yAxis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                StringFormat = "N",
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };
            RealTimeSpreadModel.Axes.Add(xAxis);
            RealTimeSpreadModel.Axes.Add(yAxis);



            //Add MID-PRICE Serie
            var lineSpreadSeries = new OxyPlot.Series.LineSeries
            {
                Title = "Spread",
                MarkerType = MarkerType.None,
                StrokeThickness = 4,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Blue,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,

            };

            RealTimeSpreadModel.Series.Add(lineSpreadSeries);
        }
        private void InitializeCummulativeCharts()
        {
            CummulativeBidsChartModel.DefaultFontSize = 8.0;
            CummulativeBidsChartModel.Title = "";
            CummulativeBidsChartModel.TitleColor = OxyColors.White;
            CummulativeBidsChartModel.PlotAreaBorderColor = OxyColors.White;
            CummulativeBidsChartModel.PlotAreaBorderThickness = new OxyThickness(0);
            CummulativeAsksChartModel.DefaultFontSize = 8.0;
            CummulativeAsksChartModel.Title = "";
            CummulativeAsksChartModel.TitleColor = OxyColors.White;
            CummulativeAsksChartModel.PlotAreaBorderColor = OxyColors.White;
            CummulativeAsksChartModel.PlotAreaBorderThickness = new OxyThickness(0);

            var xAxis = new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "N", // Format time as hours:minutes:seconds
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            var yAxis = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                StringFormat = "N",
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };
            var xAxis2 = new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "N", // Format time as hours:minutes:seconds
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            var yAxis2 = new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Right,
                StringFormat = "N",
                FontSize = 8,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };
            CummulativeBidsChartModel.Axes.Add(xAxis);
            CummulativeBidsChartModel.Axes.Add(yAxis);

            CummulativeAsksChartModel.Axes.Add(xAxis2);
            CummulativeAsksChartModel.Axes.Add(yAxis2);


            //AREA Series
            var areaSpreadSeriesBids = new OxyPlot.Series.TwoColorAreaSeries()
            {
                Title = "",
                MarkerType = MarkerType.None,
                StrokeThickness = 5,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.LightGreen,
                Fill = OxyColors.DarkGreen,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
            };
            var areaSpreadSeriesAsks = new OxyPlot.Series.TwoColorAreaSeries()
            {
                Title = "",
                MarkerType = MarkerType.None,
                StrokeThickness = 5,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Pink,
                Fill = OxyColors.DarkRed,
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
            };

            CummulativeBidsChartModel.Series.Add(areaSpreadSeriesBids);
            CummulativeAsksChartModel.Series.Add(areaSpreadSeriesAsks);
        }
        private void uiUpdaterAction()
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol))
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            if (_MARKETDATA_AVAILABLE)
            {
                // Perform property updates asynchronously
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    lock (MTX_ORDERBOOK)
                    {
                        _AskTOB_SPLIT?.RaiseUIThread();
                        _BidTOB_SPLIT?.RaiseUIThread();


                        RaisePropertyChanged(nameof(MidPoint));
                        RaisePropertyChanged(nameof(Spread));
                        RaisePropertyChanged(nameof(LOBImbalanceValue));

                    }
                });
                RaisePropertyChanged(nameof(Bids));
                RaisePropertyChanged(nameof(Asks));
                RaisePropertyChanged(nameof(Depth));


                //This is the most expensive calls. IT will freeze the UI thread if we don't de-couple
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    lock (MTX_PLOTS)
                    {
                        RealTimePricePlotModel.InvalidatePlot(true);
                        RealTimeSpreadModel.InvalidatePlot(true);
                        CummulativeBidsChartModel.InvalidatePlot(true);
                        CummulativeAsksChartModel.InvalidatePlot(true);
                    }

                });


                _MARKETDATA_AVAILABLE = false; //to avoid ui update when no new data is coming in
            }


            //TRADES
            if (_TRADEDATA_AVAILABLE)
            {
                lock (MTX_TRADES)
                {
                    while (_realTimeTrades.TryPop(out var itemToAdd))
                    {
                        TradesDisplay.Insert(0, itemToAdd);
                        if (TradesDisplay.Count > 100)
                        {
                            TradesDisplay.RemoveAt(TradesDisplay.Count - 1);
                        }
                    }

                }
                _TRADEDATA_AVAILABLE = false; //to avoid the ui updates when no new data is coming in
            }


        }



        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            /*
             * ***************************************************************************************************
             * TRANSFORM the incoming object (decouple it)
             * DO NOT hold this call back, since other components depends on the speed of this specific call back.
             * DO NOT BLOCK
               * IDEALLY, USE QUEUES TO DECOUPLE
             * ***************************************************************************************************
             */
            if (_selectedProvider == null || _selectedProvider.ProviderID <= 0 || _selectedProvider.ProviderID != e?.ProviderID)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol != e?.Symbol)
                return;

            e.CalculateMetrics();
            var localLOB = _objectPool_OrderBook.Get();
            localLOB.ShallowCopyFrom(e, _objectPool_BookItem);
            _QUEUE.Add(localLOB);
        }


        private void _AGG_DATA_OnRemoving(object? sender, PlotInfoPriceChart e)
        {
            e.AskLevelOrders.ForEach(x => _objectPool_OrderBookLevel.Return(x));
            e.BidLevelOrders.ForEach(x => _objectPool_OrderBookLevel.Return(x));
            _objectPool_Price?.Return(e);
        }
        private void _AGG_DATA_OnAggregating(PlotInfoPriceChart existing, PlotInfoPriceChart newItem)
        {
            existing.MidPrice = newItem.MidPrice;
            existing.AskPrice = newItem.AskPrice;
            existing.BidPrice = newItem.BidPrice;
            //existing.Date = newItem.Date;
            existing.Spread = newItem.Spread;
            existing.Volume = newItem.Volume;


            {
                //send existing items back to the pool
                existing.AskLevelOrders.ForEach(x => _objectPool_OrderBookLevel.Return(x));
                existing.AskLevelOrders.Clear();
                existing.BidLevelOrders.ForEach(x => _objectPool_OrderBookLevel.Return(x));
                existing.BidLevelOrders.Clear();

                //add new items
                existing.BidLevelOrders.AddRange(newItem.BidLevelOrders);
                existing.AskLevelOrders.AddRange(newItem.AskLevelOrders);
            }
        }
        private void QUEUE_onReadAction(OrderBookData ob)
        {
            PlotInfoPriceChart newPoint = null;
            lock (MTX_ORDERBOOK)
                newPoint = GenerateSinglePoint_RealTimePrice(ob);

            //Return objects to the pool
            for (int i = 0; i < ob.Bids.Count(); i++)//plain iteration performs better than enumerator or linq (by dotTrace)
                _objectPool_BookItem.Return(ob.Bids[i]);
            for (int i = 0; i < ob.Asks.Count(); i++)//plain iteration performs better than enumerator or linq (by dotTrace)
                _objectPool_BookItem.Return(ob.Asks[i]);

            _objectPool_OrderBook.Return(ob);
            if (!_AGG_DATA.Add(newPoint))
            {
                //object has been used to update the existing item (_AGG_DATA_OnAggregating), now we can discard it
                _objectPool_Price.Return(newPoint);
            }
            else
            {
                lock (MTX_PLOTS)
                {
                    Oxyplot_SeriesUpdate();
                }
            }
            _MARKETDATA_AVAILABLE = true; //to avoid ui update when no new data is coming in
        }
        private void QUEUE_onErrorAction(Exception ex)
        {
            Console.WriteLine("Error in queue processing: " + ex.Message);
            //throw ex;
        }



        private PlotInfoPriceChart GenerateSinglePoint_RealTimePrice(OrderBookData orderBook)
        {
            orderBook.CalculateAccummulated();

            BidAskGridUpdate(orderBook);
            _decimalPlaces = orderBook.PriceDecimalPlaces;


            _BidTOB = orderBook.GetTOB(true);
            _AskTOB = orderBook.GetTOB(false);
            _MidPoint = orderBook != null ? orderBook.MidPrice : 0;
            _Spread = orderBook != null ? orderBook.Spread : 0;
            _lobImbalanceValue = orderBook.ImbalanceValue;

            if (_AskTOB != null && _AskTOB.Price.HasValue && _AskTOB.Size.HasValue)
                _AskTOB_SPLIT.SetNumber(_AskTOB.Price.Value, _AskTOB.Size.Value, _decimalPlaces);
            if (_BidTOB != null && _BidTOB.Price.HasValue && _BidTOB.Size.HasValue)
                _BidTOB_SPLIT.SetNumber(_BidTOB.Price.Value, _BidTOB.Size.Value, _decimalPlaces);


            var objToAdd = _objectPool_Price.Get();
            objToAdd.Date = HelperTimeProvider.Now;
            objToAdd.MidPrice = _MidPoint;
            objToAdd.AskPrice = _AskTOB == null || !_AskTOB.Price.HasValue ? 0 : _AskTOB.Price.Value;
            objToAdd.BidPrice = _BidTOB == null || !_BidTOB.Price.HasValue ? 0 : _BidTOB.Price.Value;
            objToAdd.Volume = _AskTOB == null || _BidTOB == null || !_AskTOB.Size.HasValue || !_BidTOB.Size.HasValue ? 0 : _AskTOB.Size.Value + _BidTOB.Size.Value;
            objToAdd.Spread = _Spread;


            #region Aggregated Orders at different levels [SCATTER BUBBLES]
            var sizeMinMax = orderBook.GetMinMaxSizes();
            if (sizeMinMax.Item1 != 0)
                _minOrderSize = Math.Min(sizeMinMax.Item1, _minOrderSize);
            _maxOrderSize = Math.Max(sizeMinMax.Item2, _maxOrderSize);

            double minBubbleSize = 1; // Minimum size for bubbles in pixels
            double maxBubbleSize = 10; // Maximum size for bubbles in pixels
            if (orderBook.Bids != null)
            {
                foreach (var bid in orderBook.Bids)
                {
                    if (bid.Price.HasValue && bid.Price.Value > 0 && bid.Size.HasValue && bid.Size.Value > 0)
                    {
                        var _obj = _objectPool_OrderBookLevel.Get();
                        _obj.Date = objToAdd.Date;
                        _obj.Price = bid.Price.Value;
                        _obj.Size = minBubbleSize + (bid.Size.Value - _minOrderSize) / (_maxOrderSize - _minOrderSize) * (maxBubbleSize - minBubbleSize);

                        objToAdd.BidLevelOrders.Add(_obj);
                    }
                }
            }
            if (orderBook.Asks != null)
            {
                foreach (var ask in orderBook.Asks)
                {
                    if (ask.Price.HasValue && ask.Price.Value > 0 && ask.Size.HasValue && ask.Price.Value > 0)
                    {
                        var _obj = _objectPool_OrderBookLevel.Get();
                        _obj.Date = objToAdd.Date;
                        _obj.Price = ask.Price.Value;
                        _obj.Size = minBubbleSize + (ask.Size.Value - _minOrderSize) / (_maxOrderSize - _minOrderSize) * (maxBubbleSize - minBubbleSize);

                        objToAdd.AskLevelOrders.Add(_obj);
                    }
                }
            }
            #endregion

            return objToAdd;
        }

        private void Oxyplot_SeriesUpdate()
        {
            //clean series
            /*RealTimeSpreadModel.Series.OfType<OxyPlot.Series.LineSeries>().ToList().ForEach(x => x.Points.Clear());
            RealTimePricePlotModel.Series.OfType<OxyPlot.Series.LineSeries>().ToList().ForEach(x => x.Points.Clear());
            RealTimePricePlotModel.Series.OfType<OxyPlot.Series.ScatterSeries>().ToList().ForEach(x => x.Points.Clear());
            CummulativeAsksChartModel.Series.OfType<OxyPlot.Series.TwoColorAreaSeries>().ToList().ForEach(x => x.Points.Clear());
            CummulativeBidsChartModel.Series.OfType<OxyPlot.Series.TwoColorAreaSeries>().ToList().ForEach(x => x.Points.Clear());
            */

            if (_AGG_DATA.Count() > 0)
            {
                var point = _AGG_DATA.LastOrDefault();
                //REAL TIME PRICES
                foreach (var serie in RealTimePricePlotModel.Series)
                {
                    if (serie is OxyPlot.Series.LineSeries _serie)
                    {
                        if (point.MidPrice == 0 || point.AskPrice == 0 || point.BidPrice == 0)
                            break;
                        if (serie.Title == "MidPrice")
                            _serie.Points.Add(new DataPoint(point.Date.ToOADate(), point.MidPrice));
                        else if (serie.Title == "Ask")
                            _serie.Points.Add(new DataPoint(point.Date.ToOADate(), point.AskPrice));
                        else if (serie.Title == "Bid")
                            _serie.Points.Add(new DataPoint(point.Date.ToOADate(), point.BidPrice));
                    }
                    else if (serie is OxyPlot.Series.ScatterSeries _scatter)
                    {
                        if (_scatter.ColorAxis is LinearColorAxis colorAxis)
                        {
                            //we have defined min/max when normalizing the Size in "GenerateSinglePoint_RealTimePrice" method.
                            colorAxis.Minimum = 1;
                            colorAxis.Maximum = 10;
                        }
                        if (serie.Title == "ScatterAsks")
                        {
                            IEnumerable<OrderBookLevel> _levels;
                            _levels = point.AskLevelOrders.ToImmutableArray().Where(x => x.Price < point.MidPrice * 1.005);
                            foreach (var level in _levels)
                            {
                                if (level.Price > 0 && level.Size != 0)
                                {
                                    var newScatter = new OxyPlot.Series.ScatterPoint(point.Date.ToOADate(), level.Price,
                                        3, level.Size);
                                    _scatter.Points.Add(newScatter);
                                }
                            }
                        }
                        else if (serie.Title == "ScatterBids")
                        {
                            IEnumerable<OrderBookLevel> _levels;
                            _levels = point.BidLevelOrders.ToImmutableArray().Where(x => x.Price > point.MidPrice * 0.995);
                            foreach (var level in _levels)
                            {
                                if (level.Price > 0 && level.Size != 0)
                                {
                                    var newScatter = new OxyPlot.Series.ScatterPoint(point.Date.ToOADate(), level.Price,
                                        3, level.Size);
                                    _scatter.Points.Add(newScatter);
                                }
                            }
                        }
                    }
                }



                //REALTIME SPREAD
                var _spreadSeries = RealTimeSpreadModel.Series[0] as OxyPlot.Series.LineSeries;
                _spreadSeries.Points.Add(new DataPoint(point.Date.ToOADate(), point.Spread));


                //CUMULATIVE CHART
                double maxCummulativeVol = 0;
                CummulativeAsksChartModel.Series.OfType<OxyPlot.Series.TwoColorAreaSeries>().ToList().ForEach(x => x.Points.Clear());
                CummulativeBidsChartModel.Series.OfType<OxyPlot.Series.TwoColorAreaSeries>().ToList().ForEach(x => x.Points.Clear());
                var _cumSeriesBids = CummulativeBidsChartModel.Series[0] as OxyPlot.Series.TwoColorAreaSeries;
                var _cumSeriesAsks = CummulativeAsksChartModel.Series[0] as OxyPlot.Series.TwoColorAreaSeries;
                lock (MTX_ORDERBOOK)
                {
                    foreach (var item in _bidsGrid)
                    {
                        if (item.Price.HasValue && item.Price.Value > 0 && item.Size.HasValue && item.Size.Value > 0)
                        {
                            _cumSeriesBids.Points.Add(new DataPoint(item.Price.Value, item.CummulativeSize.Value));
                            maxCummulativeVol = Math.Max(maxCummulativeVol, item.CummulativeSize.Value);
                        }
                    }

                    foreach (var item in _asksGrid)
                    {
                        if (item.Price.HasValue && item.Price.Value > 0 && item.Size.HasValue && item.Size.Value > 0)
                        {
                            _cumSeriesAsks.Points.Add(new DataPoint(item.Price.Value, item.CummulativeSize.Value));
                            maxCummulativeVol = Math.Max(maxCummulativeVol, item.CummulativeSize.Value);
                        }
                    }
                }

                if (_cumSeriesAsks.YAxis != null)
                {
                    _cumSeriesAsks.YAxis.Maximum = maxCummulativeVol;
                    _cumSeriesBids.YAxis.Maximum = maxCummulativeVol;
                }
            }

            KeepPlotModelsSize();

        }
        private void KeepPlotModelsSize()
        {
            bool lineSeriePointIsRemoved = false;
            //remove line series first
            foreach (var _serie in RealTimePricePlotModel.Series.OfType<OxyPlot.Series.LineSeries>())
            {
                //check if the x-axis overflows
                while (_serie.Points.Count > _MAX_CHART_POINTS)
                {
                    _serie.Points.RemoveAt(0);
                    lineSeriePointIsRemoved = true;
                }
            }
            foreach (var _scatter in RealTimePricePlotModel.Series.OfType<OxyPlot.Series.ScatterSeries>())
            {
                if (lineSeriePointIsRemoved)
                {
                    //check if the x-axis overflows
                    var lineFirstPoint = RealTimePricePlotModel.Series.OfType<OxyPlot.Series.LineSeries>().First().Points[0];
                    _scatter.Points.RemoveAll(p => p.X < lineFirstPoint.X);
                    //_scatter.Points.RemoveAll(p => p.X > point.Date.ToOADate());
                }
            }


            var _spreadSeries = RealTimeSpreadModel.Series[0] as OxyPlot.Series.LineSeries;
            while (_spreadSeries.Points.Count > _MAX_CHART_POINTS)
            {
                _spreadSeries.Points.RemoveAt(0);
            }
        }
        private void Clear()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                lock (MTX_TRADES)
                {
                    _realTimeTrades.Clear();
                    TradesDisplay.Clear();
                    RaisePropertyChanged(nameof(TradesDisplay));
                }
            });


            _QUEUE.Clear(); //make this outside the LOCK, otherwise we could run into a deadlock situation when calling back 
            lock (MTX_PLOTS)
            {
                //clean series
                RealTimeSpreadModel?.Series.OfType<OxyPlot.Series.LineSeries>().ToList().ForEach(x => x.Points.Clear());
                RealTimePricePlotModel?.Series.OfType<OxyPlot.Series.LineSeries>().ToList().ForEach(x => x.Points.Clear());
                RealTimePricePlotModel?.Series.OfType<OxyPlot.Series.ScatterSeries>().ToList().ForEach(x => x.Points.Clear());
                CummulativeAsksChartModel?.Series.OfType<OxyPlot.Series.TwoColorAreaSeries>().ToList().ForEach(x => x.Points.Clear());
                CummulativeBidsChartModel?.Series.OfType<OxyPlot.Series.TwoColorAreaSeries>().ToList().ForEach(x => x.Points.Clear());
            }


            lock (MTX_ORDERBOOK)
            {
                _AskTOB = new BookItem();
                _BidTOB = new BookItem();
                _MidPoint = 0;
                _Spread = 0;

                _bidsGrid.Clear();
                _asksGrid.Clear();
                _depthGrid.Clear();

                _AskTOB_SPLIT.Clear();
                _BidTOB_SPLIT.Clear();

                _maxOrderSize = 0; //reset
                _minOrderSize = 0; //reset

                _objectPool_OrderBook = new CustomObjectPool<OrderBookData>(_MAX_CHART_POINTS * 1000);
                _objectPool_BookItem = new CustomObjectPool<BookItem>(_MAX_CHART_POINTS * 1000);
                _objectPool_Price = new CustomObjectPool<PlotInfoPriceChart>(_MAX_CHART_POINTS * 10);
                _objectPool_OrderBookLevel = new CustomObjectPool<OrderBookLevel>(_MAX_CHART_POINTS * 1000);

                if (_AGG_DATA != null)
                {
                    _AGG_DATA.OnRemoving -= _AGG_DATA_OnRemoving;
                    _AGG_DATA.Dispose();
                }
                _AGG_DATA = new AggregatedCollection<PlotInfoPriceChart>(_aggregationLevelSelection, _MAX_CHART_POINTS,
                    x => x.Date, _AGG_DATA_OnAggregating);
                _AGG_DATA.OnRemoving += _AGG_DATA_OnRemoving;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(() =>
            {
                uiUpdaterAction(); //update ui before the Timer starts again.
                if (uiUpdater != null)
                {
                    uiUpdater.Stop();
                    uiUpdater.Dispose();
                }

                var _aggregationForUI = _aggregationLevelSelection.ToTimeSpan();
                if (_aggregationForUI < _MIN_UI_REFRESH_TS)
                    _aggregationForUI = _MIN_UI_REFRESH_TS;
                uiUpdater = new UIUpdater(uiUpdaterAction, _aggregationForUI.TotalMilliseconds);
                uiUpdater.Start();
            });

        }


        /// <summary>
        /// Bids the ask grid update.
        /// Update our internal lists trying to re-use the current items on the list.
        /// Avoiding allocations as much as possible.
        /// </summary>
        /// <param name="orderBook">The order book.</param>
        private void BidAskGridUpdate(OrderBookData orderBook)
        {
            _asksGrid.ShallowUpdateFrom(orderBook.Asks);
            _bidsGrid.ShallowUpdateFrom(orderBook.Bids);


            //commented out for now
            /*if (_asksGrid != null && _bidsGrid != null)
            {
                _depthGrid.Clear();
                foreach (var item in _asksGrid)
                    _depthGrid.Add(item);
                foreach (var item in _bidsGrid)
                    _depthGrid.Add(item);
            }*/
        }

        private void ALLSYMBOLS_CollectionChanged(object? sender, string e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _symbols.Add(e);
            }));

        }
        private void TRADES_OnDataReceived(VisualHFT.Model.Trade e)
        {
            if (_selectedProvider == null || _selectedProvider.ProviderID <= 0 || _selectedProvider.ProviderID != e?.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol != e?.Symbol)
                return;

            lock (MTX_TRADES)
            {
                _realTimeTrades.Push(e);
                _TRADEDATA_AVAILABLE = true;
            }
        }
        private void PROVIDERS_OnDataReceived(object? sender, VisualHFT.Model.Provider e)
        {
            if (!_providers.Any(x => x.ProviderCode == e.ProviderCode))
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    var item = new ViewModel.Model.Provider(e);
                    if (!_providers.Any(x => x.ProviderCode == e.ProviderCode))
                        _providers.Add(item);
                    //if nothing is selected
                    if (_selectedProvider == null) //default provider must be the first who's Active
                        SelectedProvider = item;
                }));
            }
        }
        private void PROVIDERS_OnStatusChanged(object? sender, VisualHFT.Model.Provider e)
        {
            if (_selectedProvider == null || _selectedProvider.ProviderCode != e.ProviderCode)
                return;

            if (_selectedProvider.Status != e.Status)
            {
                _selectedProvider.Status = e.Status;
                Clear();
            }
        }

        public ObservableCollection<string> SymbolList => _symbols;
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());
        }
        public VisualHFT.ViewModel.Model.Provider SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value, onChanged: () => Clear());
        }
        public string SelectedLayer { get; set; }
        public ObservableCollection<Tuple<string, AggregationLevel>> AggregationLevels { get; set; }

        public AggregationLevel AggregationLevelSelection
        {
            get => _aggregationLevelSelection;
            set => SetProperty(ref _aggregationLevelSelection, value, onChanged: () => Clear());
        }

        public ObservableCollection<VisualHFT.ViewModel.Model.Provider> Providers => _providers;
        public Model.BookItemPriceSplit BidTOB_SPLIT => _BidTOB_SPLIT;
        public Model.BookItemPriceSplit AskTOB_SPLIT => _AskTOB_SPLIT;

        public double LOBImbalanceValue => _lobImbalanceValue;
        public double MidPoint => _MidPoint;
        public double Spread => _Spread;

        public IEnumerable<BookItem> Asks => _asksGrid.ToList();
        public IEnumerable<BookItem> Bids => _bidsGrid.ToList();
        public IEnumerable<BookItem> Depth => _depthGrid;
        public ObservableCollection<VisualHFT.Model.Trade> TradesDisplay { get; }

        public PlotModel RealTimePricePlotModel { get; set; }
        public PlotModel RealTimeSpreadModel { get; set; }
        public PlotModel CummulativeBidsChartModel { get; set; }
        public PlotModel CummulativeAsksChartModel { get; set; }


        public int SwitchView
        {
            get => _switchView;
            set => SetProperty(ref _switchView, value);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    uiUpdater?.Stop();
                    uiUpdater?.Dispose();

                    HelperSymbol.Instance.OnCollectionChanged -= ALLSYMBOLS_CollectionChanged;
                    HelperProvider.Instance.OnDataReceived -= PROVIDERS_OnDataReceived;
                    HelperProvider.Instance.OnStatusChanged -= PROVIDERS_OnStatusChanged;
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    HelperTrade.Instance.Unsubscribe(TRADES_OnDataReceived);

                    _dialogs = null;
                    _realTimeTrades?.Clear();
                    _depthGrid?.Clear();
                    _bidsGrid?.Clear();
                    _asksGrid?.Clear();
                    _providers?.Clear();
                    _objectPool_Price?.Dispose();
                    _objectPool_OrderBookLevel?.Dispose();
                    _QUEUE?.Dispose();

                    if (_AGG_DATA != null)
                        _AGG_DATA.OnRemoving -= _AGG_DATA_OnRemoving;
                    _AGG_DATA?.Dispose();
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
