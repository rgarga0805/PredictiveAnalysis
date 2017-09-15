using PredictiveAnalysis.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PredictiveAnalysis.Api
{
    public enum HttpMethod
    {
        POST,
        GET
    };

    public class ServiceClient<T>
    {
        public string ServiceAddress { get; set; }

        public string Domain { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        public byte[] Data { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public ServiceClient()
        {
            Parameters = new Dictionary<string, string>();
        }

        public ServiceClient(byte[] data, HttpMethod method)
        {
            Parameters = new Dictionary<string, string>();
            this.Server = "";
            this.Data = data;
            this.HttpMethod = method;
        }

        public Response<T> Post()
        {
            Response<T> response = new Response<T>();

            try
            {
                HttpWebRequest request = WebRequest.Create(this.ServiceAddress) as HttpWebRequest;
                request.Method = this.HttpMethod.ConvertTo<string>();
                request.ContentType = ServiceParameters.ContentTypeJson;
                request.ContentLength = this.Data.Length;
                request.Expect = ServiceParameters.ResponseTypeJson;

                request.Credentials = new System.Net.NetworkCredential(this.Username, this.Password, this.Domain);

                request.GetRequestStream().Write(this.Data, 0, this.Data.Length);

                HttpWebResponse serviceResponse = request.GetResponse() as HttpWebResponse;
                response.Data = (T)Convert.ChangeType(new StreamReader(serviceResponse.GetResponseStream()).ReadToEnd(), typeof(T));
            }
            catch (Exception ex)
            {
                File.WriteAllText(@"C:\LogFiles\GS_Log.txt", ex.Message)
;
            }
            return response;
        }

        public Response<T> Get()
        {
            Response<T> response = new Response<T>();

            this.ServiceAddress = String.Format("{0}{1}{2}", this.ServiceAddress, Literals.Question, BuildQueryString());
            HttpWebRequest request = WebRequest.Create(this.ServiceAddress) as HttpWebRequest;
            request.Method = HttpMethod.GET.ConvertTo<string>();

            request.Credentials = new System.Net.NetworkCredential(this.Username, this.Password, this.Domain);

            //request.GetRequestStream();

            HttpWebResponse serviceResponse = request.GetResponse() as HttpWebResponse;
            response.Data = new StreamReader(serviceResponse.GetResponseStream()).ReadToEnd().ConvertTo<T>();

            return response;
        }

        private string BuildQueryString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var parameter in this.Parameters)
            {
                result.Append(parameter.Key);
                result.Append(Literals.Equal);
                result.Append(parameter.Value);
                result.Append(Literals.Ampersand);
            }

            return result.ToString();
        }
    }
}