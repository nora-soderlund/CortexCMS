using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Cortex.CMS.API.User;

namespace Cortex.CMS.API {
    class APIManager {
        public static Dictionary<string, IAPIRequest> Requests = new Dictionary<string, IAPIRequest>() {
            { "/api/user", new UserAPI() },
            { "/api/user/register", new UserRegisterAPI() }
        };

        public static T Evaluate<T>(HttpListenerContext context, string request, string method, string body) {
            return (T)Requests[request].Evaluate(context, method, body);
        }

        public static void Handle(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if(!Requests.ContainsKey(request.RawUrl)) {
                Respond(context, JsonConvert.SerializeObject(new {
                    success = false,
                    message = $"The request {request.RawUrl} does not exist on the server!"
                }, Program.JSON));

                return;
            }

            IAPIRequest apiRequest = Requests[request.RawUrl];
            
            using StreamReader streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string body = streamReader.ReadToEnd();
            
            Respond(context, JsonConvert.SerializeObject(apiRequest.Evaluate(context, context.Request.HttpMethod, body), Program.JSON));
        }

        public static void Respond(HttpListenerContext context, string response) {
            byte[] data = Encoding.UTF8.GetBytes(response);

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = data.LongLength;

            context.Response.OutputStream.Write(data, 0, data.Length);

            context.Response.Close();
        }
    }
}
