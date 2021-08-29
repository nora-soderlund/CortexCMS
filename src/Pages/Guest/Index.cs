using System.Net;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.Pages.Guest {
    class Index : IPageRequest {
        public string GetTitle(HttpListenerContext context) {
            return null;
        }
        
        public string GetBody(HttpListenerContext context) {
            string news = "";

            using MySqlConnection connection = new MySqlConnection(Program.Database);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("SELECT * FROM news ORDER BY id DESC LIMIT 3", connection);
            using MySqlDataReader reader = command.ExecuteReader();

            while(reader.Read()) {
                news += PageManager.Get(context, "Pages/index/news.html", new Dictionary<string, string>() {
                    { "title", reader.GetString("title") },
                    { "description", reader.GetString("description") },
                    
                    { "image", reader.GetString("image") }
                });
            }

            return PageManager.Get(context, "Pages/index.html", new Dictionary<string, string>() {
                { "news", news }
            });
        }
    }
}
