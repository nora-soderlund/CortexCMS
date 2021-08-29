using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace CortexCMS.API.User {
    class Authorize : IAPIRequest {
        public Dictionary<string, object> Handle(HttpListenerContext context, string method) {
            if(method == "GET") {
                Cookie cookie = context.Request.Cookies.ToList().Find(x => x.Name == "key");

                if(cookie == null) {
                    return new Dictionary<string, object>() {
                        { "guest", true }
                    };
                }

                using MySqlConnection connection = new MySqlConnection(Program.Database);
                connection.Open();

                using MySqlCommand command = new MySqlCommand("SELECT * FROM user_keys WHERE `key` = @key", connection);
                command.Parameters.AddWithValue("@key", cookie.Value);

                using MySqlDataReader reader = command.ExecuteReader();

                if(!reader.Read()) {
                    return new Dictionary<string, object>() {
                        { "guest", true }
                    };
                }
                
                return new Dictionary<string, object>() {
                    { "guest", false },
                    { "user", reader.GetInt32("user") }
                };
            }

            if(method == "POST") { 
                using StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();

                JsonDocument result = JsonDocument.Parse(body);
                
                Console.WriteLine(result.RootElement.EnumerateObject().First(x => x.Name == "name").Value);
                Console.WriteLine(result.RootElement.EnumerateObject().First(x => x.Name == "password").Value);
                
                return new Dictionary<string, object>() {   
                    { "error", true }
                };
            }

            return null;
        }
    }
}
