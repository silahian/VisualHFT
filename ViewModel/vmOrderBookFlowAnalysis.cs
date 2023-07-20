using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.ViewModel
{
    public class vmOrderBookFlowAnalysis: BindableBase
    {
        private Dictionary<string, Func<string, string, bool>> _dialogs;

        public vmOrderBookFlowAnalysis(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;
            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
        }

        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            throw new NotImplementedException();
        }
    }
}
