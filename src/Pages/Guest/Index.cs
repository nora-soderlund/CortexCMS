using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.Pages.Guest {
    class Index : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return null;
        }
        
        public string GetBody(PageRequestClient client) {
            string news = "";

            using MySqlConnection connection = new MySqlConnection(Program.Database);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("SELECT * FROM news ORDER BY id DESC LIMIT 3", connection);
            using MySqlDataReader reader = command.ExecuteReader();

            while(reader.Read()) {
                news += PageManager.Get(client, "Pages/index/news.html", new Dictionary<string, string>() {
                    { "title", reader.GetString("title") },
                    { "description", reader.GetString("description") },
                    
                    { "image", reader.GetString("image") }
                });
            }

            return PageManager.Get(client, "Pages/index.html", new Dictionary<string, string>() {
                { "news", news }
            });
        }

        public bool GetAccess(PageRequestClient client) {
            return client.User.Guest;
        }
    }
}
