using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
    public partial class Provider
    {
        public int ProviderID
        {
            get { return this.ProviderCode; }
            set { this.ProviderCode = value; }
        }
        public int ProviderCode { get; set; }
        public string ProviderName { get; set; }

        public eSESSIONSTATUS Status { get; set; }

        public VisualHFT.PluginManager.IPlugin Plugin { get; set; } //reference to a plugin (if any)
    }
}
