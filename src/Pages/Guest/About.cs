using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.Pages.Guest {
    class AboutUs : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "About Us";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/about-us.html", new Dictionary<string, string>());
        }

        public bool GetAccess(PageRequestClient client) {
            return true;
        }
    }
}
