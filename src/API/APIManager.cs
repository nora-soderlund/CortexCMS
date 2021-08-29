using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CortexCMS.API {
    class APIManager {
        public static Dictionary<string, IAPIRequest> Requests = new Dictionary<string, IAPIRequest>() {
            { "/api/user/authorize", new User.Authorize() }
        };

        public static Dictionary<string, object> Evaluate(HttpListenerContext context, string request, string method) {
            return Requests[request].Handle(context, method);
        }

        public static void Handle(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if(!Requests.ContainsKey(request.RawUrl)) {
                Respond(context, new {
                    success = false,
                    message = $"The request {request.RawUrl} does not exist on the server!"
                });

                return;
            }

            IAPIRequest apiRequest = Requests[request.RawUrl];

            Respond(context, apiRequest.Handle(context, context.Request.HttpMethod));
        }

        public static void Respond(HttpListenerContext context, object response) {
            byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = data.LongLength;

            context.Response.OutputStream.Write(data, 0, data.Length);

            context.Response.Close();
        }
    }
}
