using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.ViewModel
{
    public class vmMultiVenuePrices : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private Dictionary<int, AggregatedCollection<PlotInfo>> _allDataSeries;
        private ObservableCollection<string> _symbols;
        private string _selectedSymbol;
        private AggregationLevel _aggregationLevelSelection;
        private int _MAX_ITEMS = 500;
        private Dictionary<int, double> _latesPrice;
        private UIUpdater uiUpdater; 
        public vmMultiVenuePrices()
        {
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));
            HelperCommon.ALLSYMBOLS.CollectionChanged += ALLSYMBOLS_CollectionChanged;
            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            
            AggregationLevels = new ObservableCollection<Tuple<string, AggregationLevel>>();
            foreach (AggregationLevel level in Enum.GetValues(typeof(AggregationLevel)))
            {
                AggregationLevels.Add(new Tuple<string, AggregationLevel>(HelperCommon.GetEnumDescription(level), level));
            }
            AggregationLevelSelection = AggregationLevel.Ms100;            
            uiUpdater = new UIUpdater(uiUpdaterAction);

            _allDataSeries = new Dictionary<int, AggregatedCollection<PlotInfo>>();
            _latesPrice = new Dictionary<int, double>();
        }
        ~vmMultiVenuePrices()
        {
            Dispose(false);
        }
        private static void Aggregation(PlotInfo existing, PlotInfo newItem)
        {
            existing.Date = newItem.Date;
            existing.Value = newItem.Value;
        }
        public ObservableCollection<string> Symbols { get => _symbols; set => _symbols = value; }        

        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());

        }
        public AggregationLevel AggregationLevelSelection
        {
            get => _aggregationLevelSelection;
            set => SetProperty(ref _aggregationLevelSelection, value, onChanged: () => Clear());
        }
        public ObservableCollection<Tuple<string, AggregationLevel>> AggregationLevels { get; set; }
        public PlotModel MyPlotModel { get; private set; }


        private void uiUpdaterAction()
        {
            RaisePropertyChanged(nameof(MyPlotModel));
            if (MyPlotModel != null)
                MyPlotModel.InvalidatePlot(true);
        }
        private void ALLSYMBOLS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _symbols = new ObservableCollection<string>(HelperCommon.ALLSYMBOLS.ToList());
            RaisePropertyChanged(nameof(Symbols));
        }
        private void LIMITORDERBOOK_OnDataReceived(object? sender, OrderBook e)
        {
            if (_selectedSymbol  == "" || _selectedSymbol == "-- All symbols --" || _selectedSymbol != e.Symbol)
                return;


            if (!_allDataSeries.ContainsKey(e.ProviderID))
            {
                _allDataSeries.Add(e.ProviderID, new AggregatedCollection<PlotInfo>(_aggregationLevelSelection, _MAX_ITEMS, x => x.Date, Aggregation));
                _latesPrice.Add(e.ProviderID, 0);
                var series = new OxyPlot.Series.LineSeries
                {
                    Title = e.ProviderName,
                    Color = OxyColors.Beige,
                    ItemsSource = null
                };
                if (MyPlotModel == null)
                {
                    MyPlotModel = new PlotModel();
                    MyPlotModel.IsLegendVisible = true;

                    var xAxe = new DateTimeAxis()
                    {
                        Position = AxisPosition.Bottom,
                        Title = "Timestamp",
                        TitleColor = OxyColors.White,
                        TextColor = OxyColors.White,
                        //xAxe.StringFormat = "HH:mm:ss:fff"

                        AxislineColor = OxyColors.White,
                        TitleFontSize = 16,
                    };
                    var yAxe = new LinearAxis()
                    {
                        Position = AxisPosition.Right,
                        Title = "Price",
                        TitleColor = OxyColors.White,
                        TextColor = OxyColors.White,
                        StringFormat = "N2",
                        
                        AxislineColor = OxyColors.White,
                        TitleFontSize = 16,
                    };
                    
                    MyPlotModel.Axes.Add(xAxe);
                    MyPlotModel.Axes.Add(yAxe);

                }
                OxyColor serieColor = MapProviderCodeToOxyColor(e.ProviderID);
                MyPlotModel.Legends.Add(new Legend { 
                    LegendSymbolPlacement = LegendSymbolPlacement.Right,
                    LegendTextColor = serieColor,
                    LegendItemAlignment = HorizontalAlignment.Right,
                    TextColor = serieColor,
                });                
                series.Color = serieColor;
                MyPlotModel.Series.Add(series);
                MyPlotModel.InvalidatePlot(true); // This refreshes the plot
            }
            if (e.LoadData())
            {
                _latesPrice[e.ProviderID] = e.MidPrice;
                

                foreach (var key in _allDataSeries.Keys)
                    _allDataSeries[key].Add(new PlotInfo() { Date = DateTime.Now, Value = _latesPrice[key] });

                
                var currentSerie = MyPlotModel.Series.Where(x => x.Title == e.ProviderName).FirstOrDefault();
                if (currentSerie != null)
                {
                    ((OxyPlot.Series.LineSeries)currentSerie).ItemsSource = _allDataSeries[e.ProviderID].AsReadOnly().Select(x => new OxyPlot.DataPoint(x.Date.Ticks, x.Value));
                }
            }
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

        private void Clear()
        {
            MyPlotModel = null;
            if (_allDataSeries != null)
                _allDataSeries.Clear();
            if (_latesPrice != null)
                _latesPrice.Clear();
            RaisePropertyChanged(nameof(MyPlotModel));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Clear();
                    uiUpdater.Dispose();
                    HelperCommon.ALLSYMBOLS.CollectionChanged -= ALLSYMBOLS_CollectionChanged;
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
