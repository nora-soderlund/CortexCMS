using System.Net;
using System.Linq;
using System.Globalization;
using System;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace Cortex.CMS.Pages.User {
    class Hotel : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Hotel";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/hotel.html", new Dictionary<string, string>() {
                { "key", client.User.Key },
                { "time", DateTime.Now.Ticks.ToString() }
            });
        }

        public bool GetAccess(PageRequestClient client) {
            return !client.User.Guest/*  && client.User.Verified&& client.User.BETA*/;
        }

        public bool GetPage(PageRequestClient client) {
            return false;
        }

        public class Lockdown : IPageRequest {
            public string GetTitle(PageRequestClient client) {
                return "Hotel Lockdown";
            }
            
            public string GetBody(PageRequestClient client) {
                return PageManager.Get(client, "Pages/Hotel/lockdown.html", new Dictionary<string, string>() {
                });
            }

            public bool GetPage(PageRequestClient client) {
                return false;
            }

            public bool GetAccess(PageRequestClient client) {
                return !client.User.Guest;
            }
        }
    }
}
