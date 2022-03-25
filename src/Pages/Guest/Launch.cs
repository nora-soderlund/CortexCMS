using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace Cortex.CMS.Pages.Guest {
    class Launch : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Launch";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/launch.html", new Dictionary<string, string>());
        }

        public bool GetAccess(PageRequestClient client) {
            return client.User.Guest;
        }

        public bool GetPage(PageRequestClient client) {
            return false;
        }

        public class Login : IPageRequest {
            public string GetTitle(PageRequestClient client) {
                return "Staff Login";
            }
            
            public string GetBody(PageRequestClient client) {
                return PageManager.Get(client, "Pages/launch/login.html", new Dictionary<string, string>());
            }

            public bool GetAccess(PageRequestClient client) {
                return client.User.Guest;
            }

            public bool GetPage(PageRequestClient client) {
                return false;
            }
        }
    }
}
