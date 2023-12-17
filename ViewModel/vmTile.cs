using Prism.Mvvm;
using System;
using System.Windows.Input;
using System.Windows;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.UserSettings;
using VisualHFT.ViewModels;
using System.Windows.Media;
using VisualHFT.Commons.Studies;
using VisualHFT.View;
using System.Collections.ObjectModel;
using QuickFix.Fields;

namespace VisualHFT.ViewModel
{
    public class vmTile : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private string _tile_id;
        private string _value;
        private string _valueToolTip;
        private string _title;
        private string _tooltip;
        private bool _isGroup;
        private UIUpdater uiUpdater;
        private System.Windows.Visibility _settingButtonVisibility;
        private System.Windows.Visibility _chartButtonVisibility;

        //*********************************************************
        //*********************************************************
        private IStudy _study;
        private IMultiStudy _multiStudy;
        //*********************************************************
        //*********************************************************

        private TileSettings _settings;
        private SolidColorBrush _valueColor = Brushes.White;

        public vmTile(IMultiStudy multiStudy)
        {
            IsGroup = true;

            _multiStudy = multiStudy;
            ChildTiles = new ObservableCollection<vmTile>();
            foreach (var study in _multiStudy.Studies)
            {
                ChildTiles.Add(new vmTile(study) { SettingButtonVisibility = Visibility.Hidden, ChartButtonVisibility = Visibility.Hidden });
            }

            _tile_id = ((PluginManager.IPlugin)_multiStudy).GetPluginUniqueID();
            _title = _multiStudy.TileTitle;
            _tooltip = _multiStudy.TileToolTip;
            _value = ".";
            _valueToolTip = "Waiting for data...";

            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            OpenChartCommand = new RelayCommand<vmTile>(OpenChartClick);
            uiUpdater = new UIUpdater(uiUpdaterAction, 300);

            RaisePropertyChanged(nameof(SelectedSymbol));
            RaisePropertyChanged(nameof(SelectedProviderName));

            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(Tooltip));
            RaisePropertyChanged(nameof(IsGroup));
            SettingButtonVisibility = Visibility.Hidden;
            ChartButtonVisibility = Visibility.Hidden;
        }
        public vmTile(IStudy study)
        {
            IsGroup = false;

            _study = study;
            _tile_id = ((PluginManager.IPlugin)_study).GetPluginUniqueID();
            _title = _study.TileTitle;
            _tooltip = _study.TileToolTip;
            _value = ".";
            _valueToolTip = "Waiting for data...";

            _study.OnCalculated += _study_OnCalculated;

            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            OpenChartCommand = new RelayCommand<vmTile>(OpenChartClick);
            uiUpdater = new UIUpdater(uiUpdaterAction, 300);

            RaisePropertyChanged(nameof(SelectedSymbol));
            RaisePropertyChanged(nameof(SelectedProviderName));

            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(Tooltip));
            RaisePropertyChanged(nameof(IsGroup));
            SettingButtonVisibility = Visibility.Visible;
            ChartButtonVisibility = Visibility.Visible;
        }

        private void _study_OnCalculated(object? sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
            if (_value == ".")
                _valueToolTip = "Waiting for data...";
            else if (!string.IsNullOrEmpty(e.Tooltip))
                _valueToolTip = e.Tooltip;
            else
                _valueToolTip = null;

            if (e.ValueColor != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _valueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(e.ValueColor));
                });

            }
        }


        ~vmTile() { Dispose(false); }

        private void uiUpdaterAction()
        {
            RaisePropertyChanged(nameof(Value));
            RaisePropertyChanged(nameof(ValueTooltip));
            RaisePropertyChanged(nameof(ValueColor));
        }
        public void UpdateAllUI()
        {
            uiUpdaterAction();
            RaisePropertyChanged(nameof(SelectedSymbol));
            RaisePropertyChanged(nameof(SelectedProviderName));
        }

        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenChartCommand { get; private set; }

        public string Value { get => _value; }
        public string ValueTooltip { get => _valueToolTip; }
        public SolidColorBrush ValueColor { get => _valueColor; }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
        public string SelectedSymbol
        {
            get
            {
                if (_study != null)
                    return ((VisualHFT.PluginManager.IPlugin)_study).Settings.Symbol;
                else if (_multiStudy != null)
                    return ((VisualHFT.PluginManager.IPlugin)_multiStudy).Settings.Symbol;
                else
                    return "";
            }
        }
        public string SelectedProviderName
        {
            get
            {
                if (_study != null)
                    return ((VisualHFT.PluginManager.IPlugin)_study).Settings.Provider.ProviderName;
                else if (_multiStudy != null)
                    return ((VisualHFT.PluginManager.IPlugin)_multiStudy).Settings.Provider.ProviderName;
                else
                    return "";
            }
        }
        public bool IsGroup { get => _isGroup; set => SetProperty(ref _isGroup, value); }
        public System.Windows.Visibility SettingButtonVisibility
        {
            get { return _settingButtonVisibility; }
            set { SetProperty(ref _settingButtonVisibility, value); }
        }
        public System.Windows.Visibility ChartButtonVisibility
        {
            get { return _chartButtonVisibility; }
            set { SetProperty(ref _chartButtonVisibility, value); }
        }

        public ObservableCollection<vmTile> ChildTiles { get; set; }

        private void OpenChartClick(object obj)
        {
            if (_study != null)
            {
                var winChart = new ChartStudy();
                winChart.DataContext = new vmChartStudy(_study);
                winChart.Show();
            }
            else if (_multiStudy != null)
            {
                var winChart = new ChartStudy();
                winChart.DataContext = new vmChartStudy(_multiStudy);
                winChart.Show();
            }
        }
        private void OpenSettings(object obj)
        {
            if (_study != null)
                PluginManager.PluginManager.SettingPlugin((PluginManager.IPlugin)_study);
            else if (_multiStudy != null)
            {
                PluginManager.PluginManager.SettingPlugin((PluginManager.IPlugin)_multiStudy);
                foreach (var child in ChildTiles)
                {
                    child.UpdateAllUI();
                }
            }
            RaisePropertyChanged(nameof(SelectedSymbol));
            RaisePropertyChanged(nameof(SelectedProviderName));

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_study != null)
                    {
                        _study.OnCalculated -= _study_OnCalculated;
                        _study.Dispose();
                    }

                    uiUpdater.Dispose();
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
