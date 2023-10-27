using Newtonsoft.Json;
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
        public DateTime LastUpdated { get; set; }
        public string StatusImage
        {
            get
            {
                if (Status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "/Images/imgGreenBall.png";
                else if (Status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "/Images/imgRedBall.png";
                else
                    return "/Images/imgYellowBall.png";
            }
        }
        public string Tooltip
        {
            get
            {
                if (Status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "Connected";
                else if (Status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "Disconnected";
                else if (Status == eSESSIONSTATUS.PRICE_CONNECTED_ORDER_DISCONNECTED)
                    return "Price connected. Order disconnected";
                else if (Status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                    return "Price disconnected. Order connected";
                else
                    return "";
            }
        }

        [JsonIgnore]
        public VisualHFT.PluginManager.IPlugin Plugin { get; set; } //reference to a plugin (if any)
    }
}
