using VisualHFT.Model;
using VisualHFT.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperActiveOrder: ConcurrentDictionary<string, OrderVM>
    {
        public event EventHandler<OrderVM> OnDataReceived;
        public event EventHandler<OrderVM> OnDataRemoved;

        public HelperActiveOrder()
        {}
        ~HelperActiveOrder()
        {}

        protected virtual void RaiseOnDataReceived(List<OrderVM> orders)
        {
            EventHandler<OrderVM> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    foreach (OrderVM o in orders)
                        _handler(this, o);
                }));
            }
        }
        protected virtual void RaiseOnDataRemoved(List<OrderVM> orders)
        {
            EventHandler<OrderVM> _handler = OnDataRemoved;
            if (_handler != null && Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    foreach (OrderVM o in orders)
                        _handler(this, o);
                }));
            }
        }

        public bool TryFindOrder(int providerId, string symbol, double price, out OrderVM order)
        {
            
            var o = this.Select(x => x.Value).Where(x => x.ProviderId == providerId && x.Symbol == symbol && x.PricePlaced == price).FirstOrDefault();
            if (o != null)
            {
                return this.TryGetValue(o.ClOrdId, out order);
            }
            else
            {
                order = null;
                return false;
            }

        }
        public void UpdateData(IEnumerable<OrderVM> orders)
        {
            List<OrderVM> _listToRemove = new List<OrderVM>();
            List<OrderVM> _listToAdd = new List<OrderVM>();
            _listToRemove = this.Where(x => !orders.Any(o => o.ProviderId == x.Value.ProviderId && o.PricePlaced == x.Value.PricePlaced)).Select(x => x.Value).ToList();
            _listToAdd = orders.Where(x => !_listToRemove.Any(o => o.ProviderId == x.ProviderId && o.PricePlaced == x.PricePlaced)).ToList();
            _listToAdd = orders.Where(x => !this.Any(o => o.Value.ProviderId == x.ProviderId && o.Value.PricePlaced == x.PricePlaced)).ToList();


            foreach (var o in _listToRemove)
                this.TryRemove(o.ClOrdId, out var fakeOrder);
            foreach (var o in _listToAdd)
                UpdateData(o);

            RaiseOnDataRemoved(_listToRemove);
            RaiseOnDataReceived(_listToAdd);
        }
        private bool UpdateData(OrderVM order)
        {            
            if (order != null)
            {
                //Check provider
                if (!this.ContainsKey(order.ClOrdId))
                {
                    order.CreationTimeStamp = DateTime.Now;
                    if (HelperCommon.PROVIDERS.ContainsKey(order.ProviderId))
                        order.ProviderName = HelperCommon.PROVIDERS[order.ProviderId].ProviderName;
                    return this.TryAdd(order.ClOrdId, order);
                }
			}
            return false;
        }

    }
}
