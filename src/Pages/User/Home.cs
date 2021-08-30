using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.Pages.User {
    class Home : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Home";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/home.html", new Dictionary<string, string>());
        }

        public bool GetAccess(PageRequestClient client) {
            return !client.User.Guest;
        }
    }
}
