using VisualHFT.Extensions;
using VisualHFT.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        static BackgroundWorker bwDatabase = null;
        public event EventHandler<IEnumerable<PositionEx>> OnInitialLoad;
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


        public ObservableCollection<PositionEx> Positions { get ; set ; }
        public ePOSITION_LOADING_TYPE LoadingType { get; set; }
        public HelperPosition(ePOSITION_LOADING_TYPE loadingType)
        {

            this.Positions = new ObservableCollection<PositionEx>();
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
        private IEnumerable<PositionEx> GetPositions(long? lastPositionID)
        {
            try
            {
                using (var db = new HFTEntities())
                {
					/*var result = (from p in db.Positions.Where(x => !lastPositionID.HasValue || x.ID > lastPositionID.Value)
								  join op in db.OpenExecutions on p.ID equals op.PositionID
								  join oc in db.CloseExecutions on p.ID equals oc.PositionID
								  select p).ToList().Select(x => new PositionEx(x));
                    */
                    db.Database.CommandTimeout = 6000;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
					var result = db.Positions.AsNoTracking().Where(x => !lastPositionID.HasValue || x.ID > lastPositionID.Value).ToList().Select(x => new PositionEx(x));
					/*foreach (var p in result)
                    {
                        p.CloseExecutions = db.CloseExecutions.AsNoTracking().Where(x => x.PositionID == p.ID).ToList();
                        p.OpenExecutions = db.OpenExecutions.AsNoTracking().Where(x => x.PositionID == p.ID).ToList();
                    }*/

					return result;
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
            if (bwDatabase == null)
            {
                bwDatabase = new BackgroundWorker();
                bwDatabase.DoWork += (wbSender, args) =>
                {
                    long lastID = 0;
                    if (this.Positions != null && this.Positions.Any())
                        lastID = this.Positions.Max(x => x.ID);
                    if (this.Positions != null)
                    {
                        var res = GetPositions(lastID);
                        if (res != null)
                            args.Result = res.ToList();
                        else
                            args.Result = null;
                    }
                    else
                    {
                        var res = GetPositions(null);
                        if (res != null)
                            args.Result = res.ToList();
                        else
                            args.Result = null;
                    }
                };
                bwDatabase.RunWorkerCompleted += (wbSender, args) =>
                {
                    if (args.Error != null)
                    {
                        return;
                    }
                    var res = args.Result as List<PositionEx>;
                    if (res != null && res.Any())
                    {
                        foreach(var p in res)
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
                            this.Positions = new ObservableCollection<PositionEx>(res);
                            RaiseOnInitialLoad(this.Positions);
                        }
                        else
                        {
                            LoadNewPositions(res);                            
                        }
                    }
                };
            }
            if (!bwDatabase.IsBusy)
                bwDatabase.RunWorkerAsync();
        }
        public void LoadNewPositions(IEnumerable<PositionEx> positions)
        {
            if (positions == null || !positions.Any())
                return;
            foreach (var p in positions)
            {
                var posToUpdate = this.Positions.Where(x => x.PositionID == p.PositionID).FirstOrDefault();
                if (posToUpdate == null)
                {
                    foreach (var ex in p.AllExecutions)
                        ex.Symbol = p.Symbol;
					App.Current.Dispatcher.Invoke((Action)delegate
					{
						this.Positions.Add(p);
					});
                }
                //
            }
        }
    }
}
