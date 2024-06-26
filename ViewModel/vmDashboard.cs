using VisualHFT.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using System.Linq;
using VisualHFT.Commons.Studies;
using System.Threading.Tasks;
using System.Windows;
using VisualHFT.Commons.Helpers;
using VisualHFT.ViewModels;

namespace VisualHFT.ViewModel
{
    public class vmDashboard : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private string _selectedSymbol;
        private string _selectedLayer;
        private string _selectedStrategy;
        private ObservableCollection<vmTile> _tiles;

        protected vmPosition _vmPosition;
        protected vmOrderBook _vmOrderBook;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public vmDashboard(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            CmdAbort = new RelayCommand<object>(DoAbort);

            this.Positions = new vmPosition(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.OrderBook = new vmOrderBook(Helpers.HelperCommon.GLOBAL_DIALOGS);
            this.NotificationsViewModel = new vmNotifications();

            Task.Run(LoadTilesAsync);
        }

        private async Task LoadTilesAsync()
        {
            while (!PluginManager.PluginManager.AllPluginsReloaded)
                await Task.Delay(1000); // allow plugins to be loaded in

            Tiles = new ObservableCollection<vmTile>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    //first, load single studies
                    foreach (var study in PluginManager.PluginManager.AllPlugins.Where(x => x is IStudy && x.GetCustomUI() == null))
                    {
                        Tiles.Add(new vmTile(study as IStudy));
                    }
                    //then, load multi-studies
                    foreach (var study in PluginManager.PluginManager.AllPlugins.Where(x => x is IMultiStudy && x.GetCustomUI() == null))
                    {
                        Tiles.Add(new vmTile(study as IMultiStudy));
                    }
                    //then, load custom UIs
                    foreach (var study in PluginManager.PluginManager.AllPlugins.Where(x => x is PluginManager.IPlugin && x.GetCustomUI() != null))
                    {
                        Tiles.Add(new vmTile(study as PluginManager.IPlugin));
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Tiles Loading", ex);
                    HelperNotificationManager.Instance.AddNotification("Tiles Loading",
                        ex.Message,
                        HelprNorificationManagerTypes.ERROR,
                        HelprNorificationManagerCategories.CORE,
                        ex);
                }
            });

        }


        public vmNotifications NotificationsViewModel { get; }
        public ObservableCollection<vmTile> Tiles
        {
            get => _tiles;
            set => SetProperty(ref _tiles, value);
        }

        public vmPosition Positions
        {
            get => _vmPosition;
            set => SetProperty(ref _vmPosition, value);
        }
        public vmOrderBook OrderBook
        {
            get => _vmOrderBook;
            set => SetProperty(ref _vmOrderBook, value);
        }

        public RelayCommand<object> CmdAbort { get; set; }


        private void DoAbort(object item)
        {
            if (_dialogs.ContainsKey("confirm"))
            {
                if (!_dialogs["confirm"]("Are you sure you want to abort the system?", ""))
                    return;
            }
            System.ComponentModel.BackgroundWorker bwDoAbort = new System.ComponentModel.BackgroundWorker();
            bwDoAbort.DoWork += (ss, args) =>
            {
                try
                {
                    //args.Result = RESTFulHelper.SetVariable<string>("ABORTSYSTEM");
                }
                catch { /*System.Threading.Thread.Sleep(5000);*/ }
            };
            bwDoAbort.RunWorkerCompleted += (ss, args) =>
            {
                if (args.Result == null)
                    _dialogs["popup"]("Message timeout.", "System Abort");
                else if (args.Result.ToBoolean())
                    _dialogs["popup"]("Message received OK.", "System Abort");
                else
                    _dialogs["popup"]("Message failed.", "System Abort");
            };
            if (!bwDoAbort.IsBusy)
                bwDoAbort.RunWorkerAsync();

        }


        public string SelectedStrategy
        {
            get => _selectedStrategy;

            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "";
                if (value.IndexOf(":") > -1)
                {
                    _selectedStrategy = value.Split(':')[0].Trim();
                    _selectedLayer = value.Split(':')[1].Trim();
                }
                else
                {
                    _selectedStrategy = value;
                    _selectedLayer = "";
                }
                if (value != "")
                {
                    _selectedSymbol = "-- All symbols --";
                    if (_vmPosition != null) _vmPosition.SelectedStrategy = value;

                    RaisePropertyChanged(nameof(SelectedStrategy));
                    RaisePropertyChanged(nameof(SelectedSymbol));
                    RaisePropertyChanged(nameof(SelectedLayer));
                };
            }
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    if (_vmPosition != null) _vmPosition.SelectedSymbol = value;
                    if (_vmOrderBook != null) _vmOrderBook.SelectedSymbol = value;

                    RaisePropertyChanged(nameof(SelectedSymbol));
                }
            }
        }
        public string SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                if (_selectedLayer != value)
                {
                    _selectedLayer = value;
                    if (_vmOrderBook != null) _vmOrderBook.SelectedLayer = value;

                    RaisePropertyChanged(nameof(SelectedLayer));
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_tiles != null)
                    {
                        foreach (var t in _tiles)
                        {
                            t.Dispose();
                        }
                    }
                    _tiles?.Clear();
                    _vmOrderBook?.Dispose();
                    _vmPosition?.Dispose();
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
