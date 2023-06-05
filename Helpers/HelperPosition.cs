using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public enum ePOSITION_LOADING_TYPE
    {
        WEBSOCKETS,
        DATABASE
    }

    public class HelperPosition
    {
        protected long? _LAST_POSITION_ID = null;
        protected List<PositionEx> _positions;

        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        public event EventHandler<IEnumerable<PositionEx>> OnInitialLoad;
        public event EventHandler<IEnumerable<PositionEx>> OnDataReceived;

        protected virtual void RaiseOnInitialLoad(IEnumerable<PositionEx> pos)
        {
            EventHandler<IEnumerable<PositionEx>> _handler = OnInitialLoad;
            if (_handler != null)
            {
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
					_handler(this, pos);
				}));
			}
		}
        protected virtual void RaiseOnDataReceived(IEnumerable<PositionEx> pos)
        {
            EventHandler<IEnumerable<PositionEx>> _handler = OnDataReceived;
            if (_handler != null)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    _handler(this, pos);
                }));
            }
        }


        public HelperPosition(ePOSITION_LOADING_TYPE loadingType)
        {

            _positions = new List<PositionEx>();
            this.LoadingType = loadingType;
            if (loadingType == ePOSITION_LOADING_TYPE.DATABASE)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Start();
                dispatcherTimer_Tick(null, null);
            }
        }


        
        public List<PositionEx> Positions 
        { 
            get { return _positions; }
        }
        public ePOSITION_LOADING_TYPE LoadingType { get; set; }
        public DateTime? SessionDate { get; set; }

        private IEnumerable<PositionEx> GetPositions()
        {
            if (!SessionDate.HasValue)
                return null;


            try
            {
                using (var db = new HFTEntities())
                {
                    db.Database.CommandTimeout = 6000;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    var allProviders = db.Providers.ToList();
					var result = db.Positions/*.AsNoTracking()*/.Include("OpenExecutions").Include("CloseExecutions").Where(x => x.CreationTimeStamp > SessionDate.Value && (!_LAST_POSITION_ID.HasValue || x.ID > _LAST_POSITION_ID.Value)).ToList();

                    if (result.Any())
                    {
                        _LAST_POSITION_ID = result.Max(x => x.ID);

                        var ret = result.Select(x => new PositionEx(x)).ToList(); //convert to our model
                                                                                  //find provider's name
                        ret.ForEach(x =>
                        {
                            x.CloseProviderName = allProviders.Where(p => p.ProviderCode == x.CloseProviderId).DefaultIfEmpty(new Provider()).FirstOrDefault().ProviderName;
                            x.OpenProviderName = allProviders.Where(p => p.ProviderCode == x.OpenProviderId).DefaultIfEmpty(new Provider()).FirstOrDefault().ProviderName;

                            x.CloseExecutions.ForEach(ex => ex.ProviderName = x.CloseProviderName);
                            x.OpenExecutions.ForEach(ex => ex.ProviderName = x.OpenProviderName);
                        });
                        return ret;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var res = GetPositions();
            if (res != null && res.Any())
            {
                foreach (var p in res)
                {
                    if (!p.PipsPnLInCurrency.HasValue || p.PipsPnLInCurrency == 0)
                    {
                        p.PipsPnLInCurrency = (p.GetCloseQuantity * p.GetCloseAvgPrice) - (p.GetOpenQuantity * p.GetOpenAvgPrice);
                        if (p.Side == ePOSITIONSIDE.Sell)
                        {
                            p.PipsPnLInCurrency *= -1;
                        }
                    }
                    if (!HelperCommon.ALLSYMBOLS.Contains(p.Symbol))
                    {
                        HelperCommon.ALLSYMBOLS.Add(p.Symbol);
                    }
                }
                if (this.Positions == null || !this.Positions.Any())
                {
                    _positions = new List<PositionEx>(res);
                    RaiseOnInitialLoad(this.Positions);
                }
                else
                {
                    foreach (var p in res)
                        _positions.Insert(0, p);
                    RaiseOnDataReceived(res);
                }
            }

        }
        public void LoadNewPositions(IEnumerable<PositionEx> positions)
        {
            if (positions == null || !positions.Any())
                return;
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                foreach (var p in positions)
                {
                    var posToUpdate = this.Positions.Where(x => x.PositionID == p.PositionID).FirstOrDefault();
                    if (posToUpdate == null)
                    {
                        foreach (var ex in p.AllExecutions)
                            ex.Symbol = p.Symbol;
                        this.Positions.Add(p);
                    }
                }
            });

        }
    }
}
