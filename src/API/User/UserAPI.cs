using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using MySql.Data.MySqlClient;

namespace CortexCMS.API.User {

    class UserAPI : IAPIRequest {
        public class Response {
            public bool Guest;

            public int? User = null;

            public bool Verified = false;
            
            public string Name = null;

            public string Key = null;
        }

        public class PostResponse {
            public bool Error;

            public string Field = null;
        }

        public object Evaluate(HttpListenerContext context, string method, string body) {
            if(method == "GET") {
                Cookie cookie = context.Request.Cookies.ToList().Find(x => x.Name == "key");

                if(cookie == null) {
                    return new Response() {
                        Guest = true
                    };
                }

                using MySqlConnection connection = new MySqlConnection(Program.Database);
                connection.Open();

                int user = -1;

                using(MySqlCommand command = new MySqlCommand("SELECT * FROM user_keys WHERE `key` = @key", connection)) {
                    command.Parameters.AddWithValue("@key", cookie.Value);

                    using MySqlDataReader reader = command.ExecuteReader();

                    if(!reader.Read()) {
                        return new Response() {
                            Guest = true
                        };
                    }

                    user = reader.GetInt32("user");
                }

                using(MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE id = @id", connection)) {
                    command.Parameters.AddWithValue("@id", user);

                    using MySqlDataReader reader = command.ExecuteReader();

                    if(!reader.Read()) {
                        return new Response() {
                            Guest = true
                        };
                    }
                    
                    return new Response() {
                        Guest = !reader.GetBoolean("verified"),

                        User = user,
                        Verified = reader.GetBoolean("verified"),
                        Name = reader.GetString("name"),
                        Key = cookie.Value
                    };
                }
            }

            if(method == "POST") { 
                var jObject = JObject.Parse(body);

                JToken name = jObject["name"];
                JToken password = jObject["password"];

                if(name == null) {
                    return new PostResponse() {   
                        Error = true,
                        Field = "name"
                    };
                }

                if(password == null) {
                    return new PostResponse() {   
                        Error = true,
                        Field = "password"
                    };
                }

                int user = 0;

                using MySqlConnection connection = new MySqlConnection(Program.Database);
                connection.Open();

                using(MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE name = @name", connection)) {
                    command.Parameters.AddWithValue("@name", name.ToString());

                    using MySqlDataReader reader = command.ExecuteReader();

                    if(!reader.Read()) {
                        return new PostResponse() {   
                            Error = true,
                            Field = "name"
                        };
                    }

                    if(!Security.Hashing.ValidatePassword(password.ToString(), reader.GetString("password"))) {
                        return new PostResponse() {   
                            Error = true,
                            Field = "password"
                        };
                    }

                    user = reader.GetInt32("id");
                }

                string key = Guid.NewGuid().ToString();

                while(true) {
                    using(MySqlCommand command = new MySqlCommand("SELECT * FROM user_keys WHERE `key` = @key", connection)) {
                        command.Parameters.AddWithValue("@key", key);

                        using MySqlDataReader reader = command.ExecuteReader();

                        if(reader.Read()) {
                            key = Guid.NewGuid().ToString();

                            continue;      
                        }          

                        context.Response.Headers.Add("Set-Cookie", $"key={key}; expires={DateTime.UtcNow.AddMonths(3).ToString("dddd, dd-MM-yyyy hh:mm:ss GMT")}; path=/");

                        break;
                    }
                }

                using(MySqlCommand command = new MySqlCommand("INSERT INTO user_keys (user, `key`, address) VALUES (@user, @key, @address)", connection)) {
                    command.Parameters.AddWithValue("@user", user);
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@address", context.Request.RemoteEndPoint.Address.ToString());

                    command.ExecuteNonQuery();
                }   

                return new PostResponse() {   
                    Error = false
                };
            }

            return null;
        }
    }
}
