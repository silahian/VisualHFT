using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.ViewModel;

namespace VisualHFT.Model
{
    public class PositionManager : BindableBase
    {
        private string _symbol;
        private double _totBuy;
        private double _totSell;
        private double _wrkBuy;
        private double _wrkSell;
        private double _plTot;
        private double _plRealized;
        private double _plOpen;
        private double _currentMidPrice;
        private PositionManagerCalculationMethod _method;
        private List<VisualHFT.Model.Order> _buys;
        private List<VisualHFT.Model.Order> _sells;
        private DateTime _lastUpdated;

        public PositionManager(List<VisualHFT.Model.Order> orders, PositionManagerCalculationMethod method)
        {
            _method = method;
            //make sure orders are of the same symbol
            if (orders.Select(x => x.Symbol).Distinct().Count() > 1)
                throw new Exception("This class is not able to handle orders with multiple symbols.");

            _buys = orders.Where(x => x.Side == eORDERSIDE.Buy).DefaultIfEmpty(new VisualHFT.Model.Order()).ToList();
            _sells = orders.Where(x => x.Side == eORDERSIDE.Sell).DefaultIfEmpty(new VisualHFT.Model.Order()).ToList();

            Symbol = orders.First().Symbol;

            Recalculate();
        }
        private void Recalculate()
        {
            TotBuy = _buys.Sum(x => x.FilledQuantity);
            TotSell = _sells.Sum(x => x.FilledQuantity);

            WrkBuy = _buys.Sum(x => x.PendingQuantity);
            WrkSell = _sells.Sum(x => x.PendingQuantity);

            PLRealized = CalculateRealizedPnL();
            PLOpen = CalculateOpenPnl();
            PLTot = PLRealized + PLOpen;

            LastUpdated = HelperTimeProvider.Now;

        }
        private List<VisualHFT.Model.Order> Buys
        {
            get => _buys;
            set => SetProperty(ref _buys, value);
        }
        private List<VisualHFT.Model.Order> Sells
        {
            get => _sells;
            set => SetProperty(ref _sells, value);
        }
        private PositionManagerCalculationMethod Method
        {
            get => _method;
            set => SetProperty(ref _method, value);
        }
        public string Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }
        public double TotBuy
        {
            get => _totBuy;
            set => SetProperty(ref _totBuy, value);
        }
        public double TotSell
        {
            get => _totSell;
            set => SetProperty(ref _totSell, value);
        }
        public double WrkBuy
        {
            get => _wrkBuy;
            set => SetProperty(ref _wrkBuy, value);
        }
        public double WrkSell
        {
            get => _wrkSell;
            set => SetProperty(ref _wrkSell, value);
        }
        public double PLTot
        {
            get => _plTot;
            set => SetProperty(ref _plTot, value);
        }
        public double PLRealized
        {
            get => _plRealized;
            set => SetProperty(ref _plRealized, value);
        }
        public double PLOpen
        {
            get => _plOpen;
            set => SetProperty(ref _plOpen, value);
        }
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }
        public double NetPosition
        {
            get => _totBuy - _totSell;
        }
        public double Exposure
        {
            get => NetPosition * _currentMidPrice;
        }
        public double CurrentMidPrice
        {
            get => _currentMidPrice;
            set => SetProperty(ref _currentMidPrice, value);
        }
        private double CalculateRealizedPnL()
        {
            return HelperPnLCalculator.CalculateRealizedPnL(_buys, _sells, _method);
        }
        private double CalculateOpenPnl()
        {
            return HelperPnLCalculator.CalculateOpenPnL(_buys, _sells, _method, _currentMidPrice);
        }
        private void UpdateUI()
        {
            RaisePropertyChanged("NetPosition");
            RaisePropertyChanged("Exposure");
        }
        public void AddOrder(VisualHFT.Model.Order newOrder)
        {
            if (newOrder.Side == eORDERSIDE.Buy)
            {
                if (_buys == null)
                    _buys = new List<Order>();
                var existingOrder = _buys.Where(x => x.OrderID == newOrder.OrderID).FirstOrDefault();
                if (existingOrder == null)
                    _buys.Add(newOrder);
                else
                    existingOrder = newOrder;
            }
            else
            {
                if (_sells == null)
                    _sells = new List<Order>();
                var existingOrder = _sells.Where(x => x.OrderID == newOrder.OrderID).FirstOrDefault();
                if (existingOrder == null)
                    _sells.Add(newOrder);
                else
                    existingOrder = newOrder;
            }
            Recalculate();
            UpdateUI();
        }
        public void UpdateLastMidPrice(double newMidPrice)
        {
            _currentMidPrice = newMidPrice;
            PLOpen = CalculateOpenPnl();
            PLTot = _plRealized + _plOpen;
            UpdateUI();
            LastUpdated = HelperTimeProvider.Now;
        }
    }
}
