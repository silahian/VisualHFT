using System.Collections.ObjectModel;
using VisualHFT.Studies;
using VisualHFT.Model;
using Prism.Mvvm;
using VisualHFT.Helpers;
using System.Windows.Threading;
using System;
using System.Windows;
using System.Collections.Generic;
using VisualHFT.Commons.Studies;
using VisualHFT.UserSettings;
using VisualHFT.ViewModel;
using System.Windows.Input;

namespace VisualHFT.ViewModels
{
    public class vmChartStudy : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private IStudy _study;
        private ISetting _settings;
        private string _selectedSymbol;
        private VisualHFT.ViewModel.Model.Provider _selectedProvider;

        private AggregatedCollection<BaseStudyModel> _rollingValues;
        private readonly object _locker = new object();


        private int _MAX_ITEMS = 500;
        private UIUpdater uiUpdater;

        public vmChartStudy(IStudy study)
        {            
            _study = study;
            _settings = ((PluginManager.IPlugin)_study).Settings;
            InitializeData();

            StudyAxisTitle = _study.TileTitle;
            RaisePropertyChanged(nameof(StudyAxisTitle));


            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);

            _study.OnCalculated += _study_OnCalculated;
            uiUpdater = new UIUpdater(uiUpdaterAction);
        }

        ~vmChartStudy()
        {
            Dispose(false);
        }

        public string StudyTitle { get; set; }
        public string StudyAxisTitle { get; set; }
        public ICommand OpenSettingsCommand { get; set; }

        public IReadOnlyList<BaseStudyModel> ChartData
        {
            get
            {
                lock (_locker)
                    return _rollingValues.AsReadOnly();
            }
        }
        private void _study_OnCalculated(object? sender, BaseStudyModel e)
        {
            lock (_locker)
            {
                _rollingValues.Add(e);
            }
        }
        private void uiUpdaterAction()
        {
            RaisePropertyChanged(nameof(ChartData));
        }
        private void OpenSettings(object obj)
        {
            PluginManager.PluginManager.SettingPlugin((PluginManager.IPlugin)_study);
            _settings = ((PluginManager.IPlugin)_study).Settings;
            InitializeData();

            
        }
        private void InitializeData()
        {
            _rollingValues = new AggregatedCollection<BaseStudyModel>(_settings.AggregationLevel,
                _MAX_ITEMS,
                x => x.Timestamp,
                (BaseStudyModel existing, BaseStudyModel newItem) =>
                {
                    existing.MarketMidPrice = newItem.MarketMidPrice;
                    existing.Value = newItem.Value;
                });
            StudyTitle = _study.TileTitle + " " +
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
                    uiUpdater.Dispose();
                    if (_study != null)
                    {
                        _study.OnCalculated -= _study_OnCalculated;
                        _study.Dispose();
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
