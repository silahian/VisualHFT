using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemini.Net.Models
{
    public class GeminiSubscription
    {
        public string type { get { return "subscribe"; } }
        public List<Subscription> subscriptions { get; set; } = new List<Subscription>();
    }

    public class Subscription
    {
        public string name { get { return "l2"; } }
        public List<string> symbols { get; set; } = new List<string>();
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class GeminiResponseTrade
    {
        public long event_id { get; set; }
        public decimal price { get; set; }
        public decimal quantity { get; set; }
        public string side { get; set; }
        public string symbol { get; set; }
        public long tid { get; set; }
        public long timestamp { get; set; }
        public string type { get; set; }
    }

    public class GeminiResponseHeartHeat
    {
        public long timestamp { get; set; }
        public string type { get; set; }
    }

    public class GeminiResponseOrderBookChanges
    {
        public List<List<string>> changes { get; set; }
        public string symbol { get; set; }
        public string type { get; set; }
    }
    public class GeminiResponseInitial
    {
        public List<List<string>> changes { get; set; }
        public string symbol { get; set; }
        public List<GemeniTradeResponse> trades { get; set; }
        public string type { get; set; }
    }



    public class GemeniTradeResponse
    {
        public long event_id { get; set; }
        public decimal price { get; set; }
        public decimal quantity { get; set; }
        public string side { get; set; }
        public string symbol { get; set; }
        public long tid { get; set; }
        public long timestamp { get; set; }
        public string type { get; set; }
    }


    public class Ask
    {
        public double price { get; set; }
        public double amount { get; set; }
        public long timestamp { get; set; }
    }

    public class Bid
    {
        public double price { get; set; }
        public double amount { get; set; }
        public long timestamp { get; set; }
    }

    public class InitialResponse
    {
        public List<Bid> bids { get; set; }
        public List<Ask> asks { get; set; }
    }


    public class GeminiUsersData
    {
        public List<UserOrderData>  UserOrders{ get; set; }
    }
    public class UserOrderData
    {
        public double executed_amount;
        public double avg_execution_price;
        public string behavior;

        public string type { get; set; }
        public long order_id { get; set; }
        public string event_id { get; set; }
        public string account_name { get; set; }
        public string api_session { get; set; }
        public string symbol { get; set; }
        public string side { get; set; }
        public string client_order_id { get; set; }
        public string order_type { get; set; }
        public long timestamp { get; set; }
        public long timestampms { get; set; }
        public bool is_live { get; set; }
        public bool is_cancelled { get; set; }
        public bool is_hidden { get; set; }
        public double original_amount { get; set; }
        public double price { get; set; }
        public int socket_sequence { get; set; }
    }
}
