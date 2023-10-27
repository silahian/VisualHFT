using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.Studies;
using VisualHFT.UserSettings;
using Prism.Commands;
using VisualHFT.ViewModels;
using System.Windows.Media;
using VisualHFT.Commons.Studies;
using log4net.Plugin;
using VisualHFT.PluginManager;
using VisualHFT.View;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VisualHFT.ViewModel
{
    public class vmTile : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private string _tile_id;
        private string _value;
        private string _title;
        private string _tooltip;        
        private UIUpdater uiUpdater;

        //*********************************************************
        //*********************************************************
        private IStudy _study;
        //*********************************************************
        //*********************************************************

        private TileSettings _settings;
        private SolidColorBrush _valueColor = Brushes.White;

        public vmTile(IStudy study)
        {
            _study = study;

            _tile_id = ((PluginManager.IPlugin)_study).GetPluginUniqueID();
            _title = _study.TileTitle;
            _tooltip = _study.TileToolTip;
            _value = ".";
            _study.OnCalculated += _study_OnCalculated;

            OpenSettingsCommand = new RelayCommand<vmTile>(OpenSettings);
            OpenChartCommand = new RelayCommand<vmTile>(OpenChartClick);
            uiUpdater = new UIUpdater(uiUpdaterAction, 300);

            RaisePropertyChanged(nameof(SelectedSymbol));
            RaisePropertyChanged(nameof(SelectedProviderName));
        }

        private void _study_OnCalculated(object? sender, BaseStudyModel e)
        {
            _value = e.ValueFormatted;
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
            RaisePropertyChanged(nameof(ValueColor));
        }


        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenChartCommand { get; private set; }

        public string Value { get => _value; }
        public SolidColorBrush ValueColor { get => _valueColor; }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
        public string SelectedSymbol { get => ((VisualHFT.PluginManager.IPlugin)_study).Settings.Symbol; }
        public string SelectedProviderName { get => ((VisualHFT.PluginManager.IPlugin)_study).Settings.Provider.ProviderName; }

        private void OpenChartClick(object obj)
        {
            if (_study != null)
            {
                var winChart = new ChartStudy();
                winChart.DataContext = new vmChartStudy(_study);
                winChart.Show();

            }
        }
        private void OpenSettings(object obj)
        {
            PluginManager.PluginManager.SettingPlugin((PluginManager.IPlugin)_study);
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
