using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace CortexCMS.Pages.Errors {
    class Error404 : IPageRequest {
        public string GetTitle(HttpListenerContext context) {
            return "404";
        }

        public string GetBody(HttpListenerContext context) {
            return PageManager.Get(context, "Pages/Errors/404.html", new Dictionary<string, string>());
        }
    }
}
