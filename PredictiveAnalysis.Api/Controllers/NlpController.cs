using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using PredictiveAnalysis.Api;
using System;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace PredictiveAnalysis.Controllers
{
    public class NlpController : ApiController
    {
        public async void GetScore()
        {
            //var baseurl = "https://gateway.watsonplatform.net/conversation/api";
            var baseurl = "https://gateway.watsonplatform.net/natural-language-understanding/api/v1/analyze?version=2017-02-27";
            //var workspace = "RGargaCoversation";
            //analyze?version=2017-02-27
            var workspace = "pizza_app - e0f3";

            var username = "5a0eb094-fde8-4d0f-89cf-cb5d35f2719b";
            var password = "OJ8pW2FgDhql";
            var context = null as object;
            //var input = Console.ReadLine();
            //input = new { text = input },

            var message = new
            {
                text = "I still have a dream, a dream deeply rooted in the American dream – one day this nation will rise up and live up to its creed, We hold these truths to be self evident: that all men are created equal",
                features = "sentiment,keywords"
            };
            var test1 = new
            {
                keywords = new { sentiment = true }
            };
            var test = JsonConvert.SerializeObject(message);

            var resp = await baseurl
                //  .AppendPathSegments("v1", "workspaces", workspace, "message")
                //.SetQueryParam("text", "I still have a dream, a dream deeply rooted in the American dream – one day this nation will rise up and live up to its creed, We hold these truths to be self evident: that all men are created equal")
                .SetQueryParams(message)
                .WithBasicAuth(username, password)
                .AllowAnyHttpStatus()
                .PostJsonAsync(test1);

            //var client = new RestClient("https://api.ambiverse.com/v1/entitylinking/analyze");
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("authorization", "Bearer ACCESS_TOKEN");
            //request.AddHeader("accept", "application/json");
            //request.AddHeader("content-type", "application/json");
            //request.AddParameter("application/json", "{\"text\" : \"Ma founded Alibaba in Hangzhou with investments from SoftBank and Goldman.\"}", ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

            var json = await resp.Content.ReadAsStringAsync();

            var answer = new
            {
                intents = default(object),
                entities = default(object),
                input = default(object),
                output = new
                {
                    text = default(string[])
                },
                context = default(object)
            };

            answer = JsonConvert.DeserializeAnonymousType(json, answer);

            var output = answer?.output?.text?.Aggregate(
                new StringBuilder(),
                (sb, l) => sb.AppendLine(l),
                sb => sb.ToString());

            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine($"{resp.StatusCode}: {output}");

            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine(json);
            //Console.ResetColor();
            //Console.ReadLine();
        }

        public void GetScoreEx()
        {
            //var baseurl = "https://gateway.watsonplatform.net/conversation/api";
            var baseurl = "https://gateway.watsonplatform.net/natural-language-understanding/api/v1/analyze?version=2017-02-27";
            //var workspace = "RGargaCoversation";
            //analyze?version=2017-02-27
            var workspace = "pizza_app - e0f3";

            ServiceClient<string> client = new ServiceClient<string>();

            //ServiceClient serviceClient = new
            client.ServiceAddress = baseurl;
            client.Username = "5a0eb094-fde8-4d0f-89cf-cb5d35f2719b";
            client.Password = "OJ8pW2FgDhql";
            var context = null as object;
            //var input = Console.ReadLine();
            ////input = new { text = input },

            //var message = new
            //{
            //    text = "I still have a dream, a dream deeply rooted in the American dream – one day this nation will rise up and live up to its creed, We hold these truths to be self evident: that all men are created equal",
            //    features = "sentiment,keywords"
            //};
            //var test1 = new
            //{
            //    keywords = new { sentiment = true }
            //};
            //var test = JsonConvert.SerializeObject(message);

            client.Parameters.Add("Text", "I still have a dream, a dream deeply rooted in the American dream – one day this nation will rise up and live up to its creed, We hold these truths to be self evident: that all men are created equal");
            client.Parameters.Add("features", "sentiment,keywords");

            var resp = client.Get();

            //var client = new RestClient("https://api.ambiverse.com/v1/entitylinking/analyze");
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("authorization", "Bearer ACCESS_TOKEN");
            //request.AddHeader("accept", "application/json");
            //request.AddHeader("content-type", "application/json");
            //request.AddParameter("application/json", "{\"text\" : \"Ma founded Alibaba in Hangzhou with investments from SoftBank and Goldman.\"}", ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

            var json = resp.Data;

            var answer = new
            {
                intents = default(object),
                entities = default(object),
                input = default(object),
                output = new
                {
                    text = default(string[])
                },
                context = default(object)
            };

            answer = JsonConvert.DeserializeAnonymousType(json, answer);

            var output = answer?.output?.text?.Aggregate(
                new StringBuilder(),
                (sb, l) => sb.AppendLine(l),
                sb => sb.ToString());

            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine($"{resp.StatusCode}: {output}");

            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine(json);
            //Console.ResetColor();
            //Console.ReadLine();
        }
    }
}