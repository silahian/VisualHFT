using System.Collections.ObjectModel;
using VisualHFT.Model;
using Prism.Mvvm;
using VisualHFT.Helpers;
using System;
using System.Collections.Generic;
using VisualHFT.Commons.Studies;
using VisualHFT.UserSettings;
using VisualHFT.ViewModel;
using System.Windows.Input;
using OxyPlot;
using VisualHFT.Commons.Pools;
using Microsoft.Extensions.ObjectPool;

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


        private Dictionary<string, Tuple<AggregatedCollection<PlotInfo>, OxyPlot.Series.LineSeries>> _allDataSeries;

        private ObservableCollection<AggregatedCollection<BaseStudyModel>> _rollingValues;
        private Dictionary<IStudy, AggregatedCollection<BaseStudyModel>> _studyToDataMap =
                new Dictionary<IStudy, AggregatedCollection<BaseStudyModel>>();

        private readonly object _locker = new object();

        private int _MAX_ITEMS = 500;
        private UIUpdater uiUpdater;

        public vmChartStudy(IStudy study)
        {
            _studies.Add(study);
            _settings = ((PluginManager.IPlugin)study).Settings;
            _plugin = (PluginManager.IPlugin)study;
            StudyAxisTitle = study.TileTitle;

            RaisePropertyChanged(nameof(StudyAxisTitle));
            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            InitializeChart();
            InitializeData();
            uiUpdater = new UIUpdater(uiUpdaterAction);
        }
        public vmChartStudy(IMultiStudy multiStudy)
        {
            foreach (var study in multiStudy.Studies)
            {
                _studies.Add(study);
            }
            _settings = ((PluginManager.IPlugin)multiStudy).Settings;
            _plugin = (PluginManager.IPlugin)multiStudy;
            StudyAxisTitle = multiStudy.TileTitle;

            RaisePropertyChanged(nameof(StudyAxisTitle));
            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            InitializeChart();
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

        public ObservableCollection<AggregatedCollection<BaseStudyModel>> ChartData
        {
            get
            {
                lock (_locker)
                    return _rollingValues;
            }
        }
        private void _study_OnCalculated(object? sender, BaseStudyModel e)
        {

            //need to link the incoming study with the _allDataSeries
            string key = ((IStudy)sender).TileTitle;
            var item = _allDataSeries[key].Item1.GetObjectPool().Get();
            item.Date = e.Timestamp;
            item.Value = (double)e.Value;
            _allDataSeries[key].Item1.Add(item);
            _allDataSeries[key].Item2.ItemsSource = _allDataSeries[key].Item1.Select(x => new OxyPlot.DataPoint(x.Date.Ticks, x.Value));


            key = "Market Mid Price";
            _allDataSeries[key].Item1.Add(new PlotInfo() { Date = e.Timestamp, Value = (double)e.MarketMidPrice });
            _allDataSeries[key].Item2.ItemsSource = _allDataSeries[key].Item1.Select(x => new OxyPlot.DataPoint(x.Date.Ticks, x.Value));
        }

        private void uiUpdaterAction()
        {
            RaisePropertyChanged(nameof(MyPlotModel));
            if (MyPlotModel != null)
                MyPlotModel.InvalidatePlot(true);
        }
        private void OpenSettings(object obj)
        {
            PluginManager.PluginManager.SettingPlugin(_plugin);
            InitializeData();


        }
        private void InitializeChart()
        {
            MyPlotModel = new OxyPlot.PlotModel();
            MyPlotModel.IsLegendVisible = true;

            var xAxe = new OxyPlot.Axes.DateTimeAxis()
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "Timestamp",
                TitleColor = OxyPlot.OxyColors.White,
                TextColor = OxyPlot.OxyColors.White,
                //StringFormat = "HH:mm:ss",

                AxislineColor = OxyPlot.OxyColors.White,
                TitleFontSize = 16,
            };
            var yAxe = new OxyPlot.Axes.LinearAxis()
            {
                Key = "yAxe",
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = this.StudyAxisTitle,
                TitleColor = OxyPlot.OxyColors.Green,
                TextColor = OxyPlot.OxyColors.Green,
                StringFormat = "N2",

                AxislineColor = OxyPlot.OxyColors.White,
                TitleFontSize = 16,
                FontSize = 12,
            };
            var yAxeMarket = new OxyPlot.Axes.LinearAxis()
            {
                Key = "yAxeMarket",
                Position = OxyPlot.Axes.AxisPosition.Right,
                Title = "Market Mid Price",
                TitleColor = OxyPlot.OxyColors.White,
                TextColor = OxyPlot.OxyColors.White,
                StringFormat = "N2",

                AxislineColor = OxyPlot.OxyColors.White,
                TitleFontSize = 16,
                FontSize = 12,
            };
            MyPlotModel.Axes.Add(xAxe);
            MyPlotModel.Axes.Add(yAxe);
            MyPlotModel.Axes.Add(yAxeMarket);
            MyPlotModel.InvalidatePlot(true);
        }
        private void InitializeData()
        {
            _allDataSeries = new Dictionary<string, Tuple<AggregatedCollection<PlotInfo>, OxyPlot.Series.LineSeries>>();

            _rollingValues = new ObservableCollection<AggregatedCollection<BaseStudyModel>>();
            _studyToDataMap.Clear();  // Clear the map when re-initializing


            MyPlotModel.Series.Clear();
            foreach (IStudy study in _studies)
            {
                study.OnCalculated += _study_OnCalculated;

                var series = new OxyPlot.Series.LineSeries
                {
                    DataFieldX = "Date",
                    DataFieldY = "Value",
                    Title = study.TileTitle,
                    YAxisKey = "yAxe",
                    Color = OxyColors.Green
                };

                _allDataSeries.Add(study.TileTitle, new Tuple<AggregatedCollection<PlotInfo>, OxyPlot.Series.LineSeries>(
                    new AggregatedCollection<PlotInfo>(_settings.AggregationLevel, _MAX_ITEMS, x => x.Date,
                    (PlotInfo existing, PlotInfo newItem) =>
                    {
                        existing.Value = newItem.Value;
                    }),
                    series)
                    );
                MyPlotModel.Series.Add(series);
            }

            //ADD MARKET SERIES
            {
                var mktSeries = new OxyPlot.Series.LineSeries
                {
                    Title = "Market Mid Price",
                    Color = OxyColors.White,
                    StrokeThickness = 5,
                    YAxisKey = "yAxeMarket",
                    TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4}"
                };
                _allDataSeries.Add(mktSeries.Title, new Tuple<AggregatedCollection<PlotInfo>, OxyPlot.Series.LineSeries>(
                    new AggregatedCollection<PlotInfo>(_settings.AggregationLevel, _MAX_ITEMS, x => x.Date,
                    (PlotInfo existing, PlotInfo newItem) =>
                    {
                        existing.Value = newItem.Value;
                    }),
                    mktSeries)
                    );
                MyPlotModel.Series.Add(mktSeries);
            }
            MyPlotModel.InvalidatePlot(true);  // Forces the plot to redraw


            StudyTitle = StudyAxisTitle + " " +
                _settings.Symbol + "-" +
                _settings.Provider?.ProviderName + " " +
                "[" + _settings.AggregationLevel.ToString() + "]";
            RaisePropertyChanged(nameof(StudyTitle));

            _selectedSymbol = _settings.Symbol;
            _selectedProvider = new ViewModel.Model.Provider(_settings.Provider);
            uiUpdaterAction();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _studyToDataMap.Clear();
                    uiUpdater.Dispose();
                    if (_studies != null)
                    {
                        foreach (var s in _studies)
                        {
                            s.OnCalculated -= _study_OnCalculated;
                            s.Dispose();
                        }
                    }
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
