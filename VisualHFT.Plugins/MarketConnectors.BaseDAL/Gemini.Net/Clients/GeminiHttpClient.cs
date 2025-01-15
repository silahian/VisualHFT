using Gemini.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gemini.Net.Clients
{
    public  class GeminiHttpClient
    {
        private  string? APIKey;
        private  string? APISecret;
        private static HttpClient geminiClient = new HttpClient();
         

        static GeminiHttpClient()
        {
            geminiClient.BaseAddress = new Uri("https://api.gemini.com/v1/");
        } 

        private   bool IsAuthHeaderPresent { get { return geminiClient.DefaultRequestHeaders.Contains("X-GEMINI-APIKEY"); } }

        public   void SetAuthentication(string apikey, string apisecret)
        {
            APIKey = apikey;
            APISecret = apisecret;

            if (!IsAuthHeaderPresent)
            {
              
            }
        }



        public async Task<InitialResponse> InitializeSnapshotAsync(string symbol)
        {
            InitialResponse resp = new InitialResponse();
            var response = await geminiClient.GetAsync("book/"+symbol);
            if (response != null)
            {
                string json = await response.Content.ReadAsStringAsync();
                resp = JsonConvert.DeserializeObject<InitialResponse>(json);
                
            }

            return resp;
        }


    }
}
