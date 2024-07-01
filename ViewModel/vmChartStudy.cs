using VisualHFT.Model;
using Prism.Mvvm;
using VisualHFT.Helpers;
using System;
using System.Collections.Generic;
using VisualHFT.Commons.Studies;
using VisualHFT.UserSettings;
using VisualHFT.ViewModel;
using System.Windows.Input;
using System.Linq;
using System.Reflection;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using Legend = OxyPlot.Legends.Legend;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using VisualHFT.Commons.Helpers;


namespace VisualHFT.ViewModels
{
    public class vmChartStudy : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private List<IStudy> _studies = new List<IStudy>();
        private ISetting _settings;
        private PluginManager.IPlugin _plugin;
        private string _selectedSymbol;
        private VisualHFT.ViewModel.Model.Provider _selectedProvider;

        HelperCustomQueue<BaseStudyModel> _QUEUE;
        private Dictionary<string, AggregatedCollection<PlotInfo>> _dataByStudy;
        private Dictionary<string, OxyPlot.Series.LineSeries> _seriesByStudy;
        private OxyPlot.Series.LineSeries _seriesMarket;
        private bool _IS_DATA_AVAILABLE = false;

        private DateTimeAxis xAxe = null;
        private LinearAxis yAxe = null;
        private double _lastMarketMidPrice = 0;

        private int _MAX_ITEMS = 1300;
        private UIUpdater uiUpdater;
        private readonly object _LOCK = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public vmChartStudy(IStudy study)
        {
            _QUEUE = new HelperCustomQueue<BaseStudyModel>($"<BaseStudyModel>_{study.TileTitle}", QUEUE_onReadAction, QUEUE_onErrorAction);
            _studies.Add(study);
            _settings = ((PluginManager.IPlugin)study).Settings;
            _plugin = (PluginManager.IPlugin)study;
            StudyAxisTitle = study.TileTitle;

            RaisePropertyChanged(nameof(StudyAxisTitle));
            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            CreatePlotModel();
            InitializeData();
            uiUpdater = new UIUpdater(uiUpdaterAction);
        }
        public vmChartStudy(IMultiStudy multiStudy)
        {
            _QUEUE = new HelperCustomQueue<BaseStudyModel>($"<BaseStudyModel>_{multiStudy.TileTitle}", QUEUE_onReadAction, QUEUE_onErrorAction);
            foreach (var study in multiStudy.Studies)
            {
                _studies.Add(study);
            }
            _settings = ((PluginManager.IPlugin)multiStudy).Settings;
            _plugin = (PluginManager.IPlugin)multiStudy;
            StudyAxisTitle = multiStudy.TileTitle;

            RaisePropertyChanged(nameof(StudyAxisTitle));
            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            CreatePlotModel();
            InitializeData();
            uiUpdater = new UIUpdater(uiUpdaterAction);
        }

        ~vmChartStudy()
        {
            Dispose(false);
        }

        public string StudyTitle { get; set; }
        public string StudyAxisTitle { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public OxyPlot.PlotModel MyPlotModel { get; set; }

        private void _study_OnCalculated(object? sender, BaseStudyModel e)
        {
            /*
             * ***************************************************************************************************
             * TRANSFORM the incoming object (decouple it)
             * DO NOT hold this call back, since other components depends on the speed of this specific call back.
             * DO NOT BLOCK
               * IDEALLY, USE QUEUES TO DECOUPLE
             * ***************************************************************************************************
             */
            var newModel = new BaseStudyModel();
            newModel.copyFrom(e);
            newModel.Tag = ((IStudy)sender).TileTitle;
            _QUEUE.Add(newModel);
        }


        private void QUEUE_onReadAction(BaseStudyModel item)
        {
            try
            {
                lock (_LOCK)
                {
                    string keyTitle = item.Tag;
                    bool isAddSuccess = false;

                    if (!_dataByStudy.ContainsKey(keyTitle))
                    {
                        CreateNewSerie(keyTitle, OxyColors.Automatic);
                    }

                    if (item.MarketMidPrice > 0)
                        _lastMarketMidPrice = (double)item.MarketMidPrice;

                    //ADD CURRENT STUDY/SERIE
                    var pointToAdd = new PlotInfo() { Date = item.Timestamp, Value = (double)item.Value };
                    isAddSuccess = _dataByStudy[keyTitle].Add(pointToAdd);
                    //if is successfully added (according to its aggregation level), proceed with adding it into the series, and then all the other studies/series (to keep the same peace)
                    if (isAddSuccess)
                    {
                        foreach (var key in _dataByStudy.Keys)
                        {
                            var series = _seriesByStudy[key];
                            if (keyTitle == key) //if the incoming item is the same as current series, add it
                            {
                                series.Points.Add(new DataPoint(pointToAdd.Date.ToOADate(), pointToAdd.Value));
                                if (series.Points.Count > _MAX_ITEMS)
                                    series.Points.RemoveAt(0);
                            }
                            else
                            {
                                //for all the other studies, add the existing last value again, so all series keep up
                                var lastPoint = _dataByStudy[key].LastOrDefault();
                                if (lastPoint != null)
                                {
                                    _dataByStudy[key].Add(lastPoint);
                                    series.Points.Add(new DataPoint(pointToAdd.Date.ToOADate(), lastPoint.Value));
                                    if (series.Points.Count > _MAX_ITEMS)
                                        series.Points.RemoveAt(0);
                                }
                            }
                        }

                        //ADD MARKET PRICE if available
                        _seriesMarket.Points.Add(new DataPoint(item.Timestamp.ToOADate(), _lastMarketMidPrice));
                        if (_seriesMarket.Points.Count > _MAX_ITEMS)
                            _seriesMarket.Points.RemoveAt(0);
                        //END ADD MARKET PRICE

                        _IS_DATA_AVAILABLE = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var _error = $"{this.StudyTitle} Unhandled error in the Chart queue: {ex.Message}";
                log.Error(_error, ex);
                HelperNotificationManager.Instance.AddNotification(this.StudyTitle, _error, HelprNorificationManagerTypes.ERROR, HelprNorificationManagerCategories.CORE);
                Clear();
            }

        }
        private void QUEUE_onErrorAction(Exception ex)
        {
            var _error = $"{this.StudyTitle} Unhandled error in the Chart queue: {ex.Message}";
            log.Error(_error, ex);
            HelperNotificationManager.Instance.AddNotification(this.StudyTitle, _error, HelprNorificationManagerTypes.ERROR, HelprNorificationManagerCategories.CORE);
        }
        private void uiUpdaterAction()
        {
            if (!_IS_DATA_AVAILABLE)
                return;
            lock (_LOCK)
            {
                RaisePropertyChanged(nameof(MyPlotModel));
                MyPlotModel?.InvalidatePlot(true);
            }

            _IS_DATA_AVAILABLE = false;
        }
        private void OpenSettings(object obj)
        {
            PluginManager.PluginManager.SettingPlugin(_plugin);
            Clear();
        }
        private void InitializeData()
        {
            lock (_LOCK)
            {
                _dataByStudy = new Dictionary<string, AggregatedCollection<PlotInfo>>();
                _seriesByStudy = new Dictionary<string, LineSeries>();
                foreach (IStudy study in _studies)
                {
                    study.OnCalculated += _study_OnCalculated;
                }
            }
            CreateMarketSeries();
            RefreshSettingsUI();
        }
        private void RefreshSettingsUI()
        {
            StudyTitle = StudyAxisTitle + " " +
                         _settings.Symbol + "-" +
                         _settings.Provider?.ProviderName + " " +
                         "[" + _settings.AggregationLevel.ToString() + "]";
            RaisePropertyChanged(nameof(StudyTitle));

            _selectedSymbol = _settings.Symbol;
            _selectedProvider = new ViewModel.Model.Provider(_settings.Provider);
        }
        private void CreatePlotModel()
        {
            MyPlotModel = new PlotModel();
            MyPlotModel.IsLegendVisible = true;
            MyPlotModel.Title = "";
            MyPlotModel.TitleColor = OxyColors.White;
            MyPlotModel.TitleFontSize = 20;
            MyPlotModel.PlotAreaBorderColor = OxyColors.White;
            MyPlotModel.PlotAreaBorderThickness = new OxyThickness(0);

            xAxe = new DateTimeAxis()
            {
                Key = "xAxe",
                Position = AxisPosition.Bottom,
                Title = "Timestamp",
                StringFormat = "HH:mm:ss", // Format time as hours:minutes:seconds
                IntervalType = DateTimeIntervalType.Auto, // Automatically determine the appropriate interval type (seconds, minutes, hours)
                MinorIntervalType = DateTimeIntervalType.Auto, // Automatically determine the appropriate minor interval type
                FontSize = 10,
                AxislineColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false,

                TitleFontSize = 16,
            };
            yAxe = new LinearAxis()
            {
                Key = "yAxe",
                Position = AxisPosition.Left,
                Title = this.StudyAxisTitle,
                StringFormat = "N",
                FontSize = 10,
                TitleColor = OxyColors.Green,
                AxislineColor = OxyColors.Green,
                TicklineColor = OxyColors.Green,
                TextColor = OxyColors.Green,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false,

                TitleFontSize = 16,
            };
            var yAxeMarket = new OxyPlot.Axes.LinearAxis()
            {
                Key = "yAxeMarket",
                Position = OxyPlot.Axes.AxisPosition.Right,
                Title = "Market Mid Price",
                TitleColor = OxyPlot.OxyColors.White,
                TextColor = OxyPlot.OxyColors.White,
                StringFormat = "N",
                AxislineColor = OxyPlot.OxyColors.White,
                TicklineColor = OxyPlot.OxyColors.White,
                TitleFontSize = 16,
                FontSize = 12,
            };
            MyPlotModel.Axes.Add(xAxe);
            MyPlotModel.Axes.Add(yAxe);
            MyPlotModel.Axes.Add(yAxeMarket);

            MyPlotModel.Legends.Add(new Legend
            {
                LegendSymbolPlacement = LegendSymbolPlacement.Left,
                LegendItemAlignment = OxyPlot.HorizontalAlignment.Left,
                LegendPosition = LegendPosition.LeftTop,
                //TextColor = serieColor,
                //LegendTitleColor = serieColor,
                FontSize = 15,
                LegendFontSize = 15,
                LegendBorderThickness = 15,
                Selectable = false,
                LegendOrientation = LegendOrientation.Vertical,
                TextColor = OxyColors.WhiteSmoke,
                LegendTextColor = OxyColors.WhiteSmoke,
            });

        }
        private void CreateNewSerie(string title, OxyColor color)
        {
            //OxyColor serieColor = MapProviderCodeToOxyColor(providerId);

            //ADD The LINE SERIE
            var series = new OxyPlot.Series.LineSeries
            {
                Title = title,
                Color = color,
                MarkerType = MarkerType.None,
                DataFieldX = "Date",
                DataFieldY = "Value",
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
                XAxisKey = "xAxe",
                YAxisKey = "yAxe",
                StrokeThickness = 2
            };
            MyPlotModel.Series.Add(series);

            _dataByStudy.Add(title, new AggregatedCollection<PlotInfo>(_settings.AggregationLevel, _MAX_ITEMS, x => x.Date, Aggregation));
            _seriesByStudy.Add(title, series);
        }
        private void CreateMarketSeries()
        {
            //ADD The LINE SERIE
            if (MyPlotModel.Series.Any(x => x.Title == "Market Mid Price"))
                return;
            _seriesMarket = new OxyPlot.Series.LineSeries
            {
                Title = "Market Mid Price",
                Color = OxyColors.White,
                MarkerType = MarkerType.None,
                DataFieldX = "Date",
                DataFieldY = "Value",
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
                XAxisKey = "xAxe",
                YAxisKey = "yAxeMarket",
                StrokeThickness = 5
            };
            MyPlotModel.Series.Add(_seriesMarket);
        }
        private void Clear()
        {
            _QUEUE.Clear(); //make this outside the LOCK, otherwise we could run into a deadlock situation when calling back 

            lock (_LOCK)
            {
                uiUpdater?.Stop();
                uiUpdater?.Dispose();

                if (MyPlotModel.Series != null)
                {
                    foreach (var s in MyPlotModel.Series)
                    {
                        (s as OxyPlot.Series.LineSeries)?.Points.Clear();
                    }
                }

                if (_dataByStudy != null)
                {
                    foreach (var data in _dataByStudy)
                    {
                        data.Value.Clear();
                        data.Value.Dispose();
                        _dataByStudy[data.Key] = new AggregatedCollection<PlotInfo>(_settings.AggregationLevel, _MAX_ITEMS,
                            x => x.Date, Aggregation);
                    }
                }
                if (_seriesByStudy != null)
                {
                    foreach (var s in _seriesByStudy)
                    {
                        s.Value.Points.Clear();
                    }
                }
                _seriesMarket?.Points.Clear();
            }

            uiUpdater = new UIUpdater(uiUpdaterAction, _settings.AggregationLevel.ToTimeSpan().TotalMilliseconds);
            RefreshSettingsUI();
        }


        private static void Aggregation(PlotInfo existing, PlotInfo newItem)
        {
            //existing.Date = newItem.Date;
            existing.Value = newItem.Value;
        }
        private OxyColor MapProviderCodeToOxyColor(int providerCode)
        {
            // Get all the OxyColors from the OxyColors class
            var allColors = typeof(OxyColors).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(field => field.FieldType == typeof(OxyColor))
                .Select(field => (OxyColor)field.GetValue(null))
                .ToArray();

            // Exclude the Undefined and Automatic colors
            allColors = allColors.Where(color => color != OxyColors.Undefined && color != OxyColors.Automatic).ToArray();

            // Shuffle the colors using a seeded random number generator
            allColors = Shuffle(allColors, new Random(providerCode)).ToArray();

            // Return the first color from the shuffled array
            return allColors[0];
        }
        private IEnumerable<T> Shuffle<T>(IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _QUEUE?.Dispose();
                    uiUpdater.Stop();
                    uiUpdater.Dispose();


                    if (MyPlotModel.Series != null)
                    {
                        foreach (var s in MyPlotModel.Series)
                        {
                            (s as OxyPlot.Series.LineSeries)?.Points.Clear();
                        }
                    }

                    if (_dataByStudy != null)
                    {
                        foreach (var data in _dataByStudy)
                        {
                            data.Value.Clear();
                            data.Value.Dispose();
                        }
                        _dataByStudy.Clear();
                    }
                    if (_seriesByStudy != null)
                    {
                        foreach (var s in _seriesByStudy)
                        {
                            s.Value.Points.Clear();
                        }
                        _seriesByStudy.Clear();
                    }
                    if (_studies != null)
                    {
                        foreach (var s in _studies)
                        {
                            s.OnCalculated -= _study_OnCalculated;
                        }
                        _studies.Clear();
                    }
                    _seriesMarket?.Points.Clear();
                    MyPlotModel = null;
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
