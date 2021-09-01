using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.Pages.Guest {
    class Registration : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Registration";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/registration.html", new Dictionary<string, string>());
        }

        public bool GetAccess(PageRequestClient client) {
            return client.User.Guest;
        }
        
        public class Verification : IPageRequest {
            public string GetTitle(PageRequestClient client) {
                return "Registration Verification";
            }
            
            public string GetBody(PageRequestClient client) {
                return PageManager.Get(client, "Pages/registration/verification.html", new Dictionary<string, string>());
            }

            public bool GetAccess(PageRequestClient client) {
                return client.User.Guest && !client.User.Verified;
            }
        }
    }
}
