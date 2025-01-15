using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.Net.Models
{
    public class CoinbaseSubscription
    {
        public string type { get; set; } = "subscribe";
        public List<string> product_ids { get; set; }
        public List<object> channels { get; set; }
        public string signature { get; set; }
        public string key{ get; set; }
        public string passphrase { get; set; }
        public string timestamp { get; set; }
    }


    public class CoinbaseCredentials
    {
          public string AccessKey { get; set; }

          public string Passphrase { get; set; }

          public string SigningKey { get; set; }

        public CoinbaseCredentials(string accessKey,string passphrase,string signingKey)
        {
            if (string.IsNullOrWhiteSpace(accessKey.Trim()))
            {
                throw new Exception("Access key is required");
            }

            this.AccessKey = accessKey;

            if (string.IsNullOrWhiteSpace(passphrase.Trim()))
            {
                throw new Exception("Passphrase is required");
            }

            this.Passphrase = passphrase;

            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new Exception("Signing key is required");
            }

            this.SigningKey = signingKey;
        } 
        public CoinbaseCredentials()
        {
        }

       

        public string Sign(string timestamp, string method, string path, string body)
        {
            try
            {
                string message = $"{timestamp}{method}{path}{body}";

                byte[] hmacKey;
                try
                {
                    hmacKey = Convert.FromBase64String(this.SigningKey);
                }
                catch (FormatException)
                {
                    hmacKey = Encoding.UTF8.GetBytes(this.SigningKey);
                }

                using var hmac = new HMACSHA256(hmacKey);
                byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(signature);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to generate signature", e);
            }
        }
    }
}
