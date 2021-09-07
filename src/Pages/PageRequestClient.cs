using System;
using System.Net;
using System.Collections.Generic;

using Cortex.CMS.API.User;

namespace Cortex.CMS {
    class PageRequestClient {
        public HttpListenerRequest Request;
        public HttpListenerResponse Response;

        public UserAPI.Response User;

        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public PageRequestClient(HttpListenerContext context) {
            Request = context.Request;
            Response = context.Response;

            User = API.APIManager.Evaluate<UserAPI.Response>(context, "/api/user", "GET", "");

            int get = context.Request.RawUrl.IndexOf('?');

            if(get != -1) {
                string[] parameters = context.Request.RawUrl.Substring(get + 1, context.Request.RawUrl.Length - (get + 1)).Split('&');

                foreach(string parameter in parameters) {
                    string[] properties = parameter.Split('=');

                    Parameters.Add(properties[0], (properties.Length == 1)?(null):(properties[1]));
                }
            }
        }
    }
}