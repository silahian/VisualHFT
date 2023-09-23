using VisualHFT.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace VisualHFT.Helpers
{
    public class HelperActiveOrder: ConcurrentDictionary<string, VisualHFT.Model.Order>
    {
        public event EventHandler<VisualHFT.Model.Order> OnDataReceived;
        public event EventHandler<VisualHFT.Model.Order> OnDataRemoved;

        public HelperActiveOrder()
        {}
        ~HelperActiveOrder()
        {}

        protected virtual void RaiseOnDataReceived(List<VisualHFT.Model.Order> orders)
        {
            EventHandler<VisualHFT.Model.Order> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                foreach (var o in orders)
                    _handler(this, o);
            }
        }
        protected virtual void RaiseOnDataRemoved(List<VisualHFT.Model.Order> orders)
        {
            EventHandler<VisualHFT.Model.Order> _handler = OnDataRemoved;
            if (_handler != null && Application.Current != null)
            {
                foreach (var o in orders)
                    _handler(this, o);
            }
        }

        public bool TryFindOrder(int providerId, string symbol, double price, out VisualHFT.Model.Order order)
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
        public void UpdateData(IEnumerable<VisualHFT.Model.Order> orders)
        {
            var _listToRemove = new List<VisualHFT.Model.Order>();
            var _listToAdd = new List<VisualHFT.Model.Order>();
            _listToRemove = this.Where(x => !orders.Any(o => o.ClOrdId == x.Value.ClOrdId)).Select(x => x.Value).ToList();
            _listToAdd = orders.Where(x => !this.Any(o => o.Value.ClOrdId == x.ClOrdId)).ToList();


            foreach (var o in _listToRemove)
                this.TryRemove(o.ClOrdId, out var fakeOrder);
            if (_listToRemove.Any())
                RaiseOnDataRemoved(_listToRemove);

            var _listToAddOrUpdate = new List<VisualHFT.Model.Order>();
            foreach (var o in _listToAdd)
            {
                if (UpdateData(o))
                    _listToAddOrUpdate.Add(o);
            }
            if (_listToAddOrUpdate.Any())
                RaiseOnDataReceived(_listToAddOrUpdate);
        }
        private bool UpdateData(VisualHFT.Model.Order order)
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
                else
                {
                    return TryUpdate(order.ClOrdId, order, this[order.ClOrdId]);
                }
			}
            return false;
        }

    }
}
