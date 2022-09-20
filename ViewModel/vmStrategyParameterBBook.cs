using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using VisualHFT.Helpers;
using VisualHFT.View.StatisticsView;
using System.Data;
using VisualHFT.Model;
using System.Runtime.CompilerServices;

namespace VisualHFT.ViewModel
{
    public class vmStrategyParameterBBook : vmStrategyParametersBase<StrategyParametersBBookVM>, INotifyPropertyChanged
	{
        
        public vmStrategyParameterBBook(Dictionary<string, Func<string, string, bool>> dialogs, ucStrategyOverview uc) : base(dialogs, uc)
		{
			_strategyNameForThisControl = "BBook";
		}


        private void SetParameters()
        {

            if (!bwSetParameters.WorkerSupportsCancellation)
            {
                bwSetParameters.WorkerSupportsCancellation = true; //use it to know if it was already setup
                bwSetParameters.DoWork += (s, args) =>
                {
                    try
                    {
                        args.Result = RESTFulHelper.SetVariable<List<StrategyParametersBBookVM>>(modelItems.ToList());
                    }
                    catch { }
                };
                bwSetParameters.RunWorkerCompleted += (s, args) =>
                {
                    var res = args.Result as List<StrategyParametersBBookVM>;
                    if (res == null)
                        return;
                };
            }
            if (!bwSetParameters.IsBusy)
                bwSetParameters.RunWorkerAsync();
        }
		public override void OnSaveSettingsToDB()
        {
            if (modelItems == null)
                return;
            using (var db = new HFTEntities())
            {
                foreach(var setting in modelItems)
                {
                    var existingItem = db.STRATEGY_PARAMETERS_BBOOK.Where(x => x.Symbol == setting.Symbol && x.LayerName == setting.LayerName).FirstOrDefault();
                    if (existingItem != null)
                    {
                        existingItem.PositionSize = setting.PositionSize;
                        existingItem.PipsArb = setting.PipsArb;
                        existingItem.MillisecondsToWaitBeofreClosing = setting.MillisecondsToWaitBeofreClosing;
						existingItem.PNLoverallPositionToClose = setting.PNLoverallPositionToClose;
						existingItem.ClosingWaitingBBook = setting.ClosingWaitingBBook;
						existingItem.ClosingWaitingTime = setting.ClosingWaitingTime;
						existingItem.AfterCloseWaitForMillisec = setting.AfterCloseWaitForMillisec;

                        existingItem.PipsHedgeStopLoss = setting.PipsHedgeStopLoss;
                        existingItem.PipsHedgeTakeProf = setting.PipsHedgeTakeProf;
                        existingItem.PipsHedgeTrailing = setting.PipsHedgeTrailing;
                    }
                    else
                        db.STRATEGY_PARAMETERS_BBOOK.Add(setting.ThisToDBObject());
                    db.SaveChanges();
                }
            }
        }
        private bool LoadSettingsFromDB()
        {
            bool bLoadedFromDB = false;
            using (var db = new HFTEntities())
            {
                modelItems.Clear();
                foreach(var setting in db.STRATEGY_PARAMETERS_BBOOK.ToList())
                {
                    modelItems.Add(new StrategyParametersBBookVM(setting));
                    bLoadedFromDB = true;
                }
            }
            return bLoadedFromDB;
        }
		public override void OnUpdateToAllModelsIfAllSymbolsIsSelected()
		{
			if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
			{
				foreach (var m in modelItems)
				{
					if (_model.PositionSize > 0)
						m.PositionSize = _model.PositionSize;
					if (_model.PipsArb > 0)
						m.PipsArb = _model.PipsArb;
					if (_model.MillisecondsToWaitBeofreClosing > 0)
						m.MillisecondsToWaitBeofreClosing = _model.MillisecondsToWaitBeofreClosing;

					if (_model.PNLoverallPositionToClose > 0)
						m.PNLoverallPositionToClose = _model.PNLoverallPositionToClose;
					if (_model.AfterCloseWaitForMillisec > 0)
						m.AfterCloseWaitForMillisec = _model.AfterCloseWaitForMillisec;

					m.ClosingWaitingTime = _model.ClosingWaitingTime;
					m.ClosingWaitingBBook = _model.ClosingWaitingBBook;

					if (_model.PipsHedgeStopLoss > 0)
						m.PipsHedgeStopLoss = _model.PipsHedgeStopLoss;
					if (_model.PipsHedgeTakeProf > 0)
						m.PipsHedgeTakeProf = _model.PipsHedgeTakeProf;
					if (_model.PipsHedgeTrailing > 0)
						m.PipsHedgeTrailing = _model.PipsHedgeTrailing;
					if (_model.PositionSize > 0)
						m.PositionSize = _model.PositionSize;
				}
			}
		}

	}
}
