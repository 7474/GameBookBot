using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace GameBookBot
{
    public class BingImagesConnector
    {
        private const string SUBSCRIPTION_KEY = "{set your key}";

        public async Task<string> SearchImage(string word)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);

            // Request parameters
            queryString["q"] = word;
            queryString["count"] = "10";
            queryString["offset"] = "0";
            queryString["mkt"] = "ja-JP";
            queryString["safeSearch"] = "Moderate";
            var uri = "https://api.cognitive.microsoft.com/bing/v5.0/images/search?" + queryString;

            string imageUrl = "";
            var response = await client.GetStringAsync(uri);
            try
            {
                dynamic result = JObject.Parse(response);
                imageUrl = result.value[0].contentUrl;
            }
            catch
            {
                imageUrl = null;
            }
            return imageUrl;
        }

    }
}