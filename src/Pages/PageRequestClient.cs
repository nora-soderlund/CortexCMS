using System.Net;
using System.Collections.Generic;

using CortexCMS.API.User;

namespace CortexCMS {
    class PageRequestClient {
        public HttpListenerRequest Request;
        public HttpListenerResponse Response;

        public UserAPI.Response User;

        public PageRequestClient(HttpListenerContext context) {
            Request = context.Request;
            Response = context.Response;

            User = API.APIManager.Evaluate<UserAPI.Response>(context, "/api/user", "GET", "");
        }
    }
}