using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace Cortex.CMS.Pages.Guest {
    class Maintenance : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Maintenance Break";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/maintenance.html", new Dictionary<string, string>() {});
        }

        public bool GetAccess(PageRequestClient client) {
            return Program.Settings["maintenance"] == "true";
        }
    }
}
