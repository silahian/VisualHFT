using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Timers;
using System.Data.Entity;
using System.Threading.Tasks;

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
        protected DateTime? _sessionDate = null;

        System.Timers.Timer _timer;
        public event EventHandler<IEnumerable<PositionEx>> OnInitialLoad;
        public event EventHandler<IEnumerable<PositionEx>> OnDataReceived;

        protected virtual void RaiseOnInitialLoad(IEnumerable<PositionEx> pos)
        {
            EventHandler<IEnumerable<PositionEx>> _handler = OnInitialLoad;
            if (_handler != null)
                _handler(this, pos);
		}
        protected virtual void RaiseOnDataReceived(IEnumerable<PositionEx> pos)
        {
            EventHandler<IEnumerable<PositionEx>> _handler = OnDataReceived;
            if (_handler != null)
                _handler(this, pos);
        }


        public HelperPosition(ePOSITION_LOADING_TYPE loadingType)
        {

            _positions = new List<PositionEx>();
            this.LoadingType = loadingType;
            if (loadingType == ePOSITION_LOADING_TYPE.DATABASE)
            {
                _timer = new System.Timers.Timer();
                _timer.Interval = 5000;
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();
                _timer_Elapsed(null, null);
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IEnumerable<PositionEx> res = new List<PositionEx>();


            // Call an async method
            Task.Run(async () =>
            {
                res = await GetPositions();

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


            });

            


        }

        public List<PositionEx> Positions 
        { 
            get { return _positions; }
        }
        public ePOSITION_LOADING_TYPE LoadingType { get; set; }
        public DateTime? SessionDate {
            get { return _sessionDate; }
            set
            {
                if (value != _sessionDate)
                {
                    _sessionDate = value;
                    this.Positions.Clear();
                    _LAST_POSITION_ID = null;
                    _timer_Elapsed(null, null);
                }
            }
        }

        private async Task<IEnumerable<PositionEx>> GetPositions()
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
					var result = await db.Positions/*.AsNoTracking()*/.Include("OpenExecutions").Include("CloseExecutions").Where(x => x.CreationTimeStamp > SessionDate.Value && (!_LAST_POSITION_ID.HasValue || x.ID > _LAST_POSITION_ID.Value)).ToListAsync();

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
