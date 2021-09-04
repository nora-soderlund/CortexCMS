using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.Pages.User {
    class Hotel : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Hotel";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/hotel.html", new Dictionary<string, string>() {
                { "key", client.User.Key }
            });
        }

        public bool GetAccess(PageRequestClient client) {
            return !client.User.Guest && client.User.Verified;
        }

        public bool GetPage(PageRequestClient client) {
            return false;
        }
    }
}
