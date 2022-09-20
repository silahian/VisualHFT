using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Helpers
{
    public class YahooStock
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double PreviousClose { get; set; }
        public double LastTradePrice { get; set; }
        public double Volume { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
    }
    public class HelperYahoo
    {
        static public YahooStock GetStock(string symbol)
        {
            //s -> symbol
            //g -> days low
            //h -> days high
            //c -> change
            //o -> open
            //p -> previous close
            //v -> volume
            //l1 -> last trade (price only) -> close
            //n -> name
            string baseURL = "http://finance.yahoo.com/d/quotes.csv?s={0}&f=npl1vab";
            string url = string.Format(baseURL, symbol);

            //Get page showing the table with the chosen indices
            HttpWebRequest request = null;

            //csv content
            string docText = string.Empty;
            YahooStock stock = null;
            try
            {
                request = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                request.Timeout = 300000;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (StreamReader stReader = new StreamReader(response.GetResponseStream()))
                {
                    string output = stReader.ReadLine();
                    //"\"Apple Inc.\",587.44,572.98,36820544"

                    string[] sa = output.Split(new char[] { ',' });

                    stock = new YahooStock();
                    stock.Symbol = symbol;
                    stock.Name = sa[0];
                    stock.PreviousClose = double.Parse(sa[1]);
                    stock.LastTradePrice = double.Parse(sa[2]);
                    stock.Volume = double.Parse(sa[3]);
                    stock.Ask = double.Parse(sa[4]);
                    stock.Bid = double.Parse(sa[5]);
                }
            }
            catch (Exception )
            {
                //Throw some exception here.
            }
            return stock;
        }

    }
}
