using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace CortexCMS.Pages.Errors {
    class Error404 : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "404";
        }

        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/Errors/404.html", new Dictionary<string, string>());
        }
    }
}
