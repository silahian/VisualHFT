using System.Collections.ObjectModel;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class HelperStrategy : ObservableCollection<string>
    {
        protected object _LOCK = new object();
        public HelperStrategy()
        { }

        public void UpdateData(List<Strategy> data)
        {
            lock (_LOCK)
            {
                foreach (Strategy vm in data)
                {
                    if (!this.Any(x => x == vm.StrategyCode))
                    {
                        this.Add(vm.StrategyCode);
                    }
                }
            }
        }
    }
}