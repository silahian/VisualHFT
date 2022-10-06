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
    public class vmStrategyParameterFirmMM: vmStrategyParametersBase<StrategyParametersFirmMMVM>, INotifyPropertyChanged
    {

        public vmStrategyParameterFirmMM(Dictionary<string, Func<string, string, bool>> dialogs): base(dialogs)
		{            
            _strategyNameForThisControl = "FirmMM";
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
						args.Result = RESTFulHelper.SetVariable<List<StrategyParametersFirmMMVM>>(modelItems.ToList());
					}
					catch { }
				};
				bwSetParameters.RunWorkerCompleted += (s, args) =>
				{
					var res = args.Result as List<StrategyParametersFirmMMVM>;
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
				foreach (var setting in modelItems)
				{
					var existingItem = db.STRATEGY_PARAMETERS_FIRMMM.Where(x => x.Symbol == setting.Symbol && x.LayerName == setting.LayerName).FirstOrDefault();
					if (existingItem != null)
					{
						existingItem.PositionSize = setting.PositionSize;
						existingItem.MaximumExposure = setting.MaximumExposure;
						existingItem.LookUpBookForSize = setting.LookUpBookForSize;
						existingItem.PipsMarkupAsk = setting.PipsMarkupAsk;
						existingItem.PipsMarkupBid = setting.PipsMarkupBid;
						existingItem.MinPipsDiffToUpdatePrice = setting.MinPipsDiffToUpdatePrice;
						existingItem.MinSpread = setting.MinSpread;
						existingItem.PipsSlippage = setting.PipsSlippage;
						existingItem.AggressingToHedge = setting.AggressingToHedge;
						existingItem.PipsSlippageToHedge = setting.PipsSlippageToHedge;
						existingItem.PipsHedgeStopLoss = setting.PipsHedgeStopLoss;
						existingItem.PipsHedgeTakeProf = setting.PipsHedgeTakeProf;
						existingItem.PipsHedgeTrailing = setting.PipsHedgeTrailing;
						existingItem.PricingType = (int)setting.PricingType;
						existingItem.TickSample = setting.TickSample;
						existingItem.BollingerPeriod = setting.BollingerPeriod;
						existingItem.BollingerStdDev = setting.BollingerStdDev;
					}
					else
						db.STRATEGY_PARAMETERS_FIRMMM.Add(setting.ThisToDBObject());
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
				foreach (var setting in db.STRATEGY_PARAMETERS_FIRMMM.ToList())
				{
					modelItems.Add(new StrategyParametersFirmMMVM(setting));
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
					if (_model.MaximumExposure > 0)
						m.MaximumExposure = _model.MaximumExposure;
					if (_model.LookUpBookForSize > 0)
						m.LookUpBookForSize = _model.LookUpBookForSize;
					if (_model.PipsMarkupAsk > 0)
						m.PipsMarkupAsk = _model.PipsMarkupAsk;
					if (_model.PipsMarkupBid > 0)
						m.PipsMarkupBid = _model.PipsMarkupBid;
					if (_model.MinPipsDiffToUpdatePrice > 0)
						m.MinPipsDiffToUpdatePrice = _model.MinPipsDiffToUpdatePrice;
					if (_model.MinSpread > 0)
						m.MinSpread = _model.MinSpread;
					if (_model.PipsSlippage > 0)
						m.PipsSlippage = _model.PipsSlippage;
					if (_model.PipsSlippageToHedge > 0)
						m.PipsSlippageToHedge = _model.PipsSlippageToHedge;
					if (_model.PipsHedgeStopLoss > 0)
						m.PipsHedgeStopLoss = _model.PipsHedgeStopLoss;
					if (_model.PipsHedgeTakeProf > 0)
						m.PipsHedgeTakeProf = _model.PipsHedgeTakeProf;
					m.PipsHedgeTrailing = _model.PipsHedgeTrailing;
					if (_model.PricingType > 0)
						m.PricingType = _model.PricingType;
					if (_model.TickSample > 0)
						m.TickSample = _model.TickSample;
					if (_model.BollingerPeriod > 0)
						m.BollingerPeriod = _model.BollingerPeriod;
					if (_model.PositionSize > 0)
						m.PositionSize = _model.PositionSize;

				}
			}
		}

    }
}
