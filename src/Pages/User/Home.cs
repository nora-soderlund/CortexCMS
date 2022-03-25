using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace Cortex.CMS.Pages.User {
    class Home : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Home";
        }
        
        public string GetBody(PageRequestClient client) {
            string news = "";

            using MySqlConnection connection = new MySqlConnection(Program.Database);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("SELECT * FROM news ORDER BY id DESC LIMIT 3", connection);
            using MySqlDataReader reader = command.ExecuteReader();

            while(reader.Read()) {
                news += PageManager.Get(client, "Pages/index/news-article.html", new Dictionary<string, string>() {
                    { "title", reader.GetString("title") },
                    { "description", reader.GetString("description") },

                    { "link", "/community/news/" + reader.GetString("link") },
                    
                    { "image", reader.GetString("image") }
                });
            }

            return PageManager.Get(client, "Pages/home.html", new Dictionary<string, string>() {
                { "news", (news.Length != 0)?(PageManager.Get(client, "Pages/index/news.html", new Dictionary<string, string>() {
                    { "articles", news }
                })):("") }
            });
        }

        public bool GetAccess(PageRequestClient client) {
            return !client.User.Guest;
        }
    }
}
