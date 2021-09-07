using System;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using MySql.Data.MySqlClient;

namespace Cortex.CMS.API.User {

    class UserRegisterAPI : IAPIRequest {
        public class Response {
            public bool Error;

            public string Field = null;

            public string Message = null;
        }

        public object Evaluate(HttpListenerContext context, string method, string body) {
            if(method == "POST") { 
                var jObject = JObject.Parse(body);

                JToken name = jObject["name"];
                JToken email = jObject["email"];
                JToken password = jObject["password"];

                if(name == null || name.ToString().Length < 3) {
                    return new Response() {   
                        Error = true,
                        Field = "name",
                        Message = "Your user name must be 3 letters or more!"
                    };
                }

                if(!Regex.IsMatch(name.ToString(), @"^[a-zA-Z0-9:.-_]+$")) {
                    return new Response() {   
                        Error = true,
                        Field = "name",
                        Message = "Your user name can't contain those characters!"
                    };
                }

                using MySqlConnection connection = new MySqlConnection(Program.Database);
                connection.Open();

                using(MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE name = @name", connection)) {
                    command.Parameters.AddWithValue("@name", name.ToString());

                    using MySqlDataReader reader = command.ExecuteReader();

                    if(reader.Read()) {
                        return new Response() {   
                            Error = true,
                            Field = "name",
                            Message = "This user name is already taken!"
                        };
                    }
                }

                try {
                    MailAddress address = new MailAddress(email.ToString());
                    
                    if(address.Address != email.ToString()) {
                        return new Response() {   
                            Error = true,
                            Field = "email",
                            Message = "You must enter a valid e-mail!"
                        };
                    }
                }
                catch {
                    return new Response() {   
                        Error = true,
                        Field = "email",
                        Message = "You must enter a valid e-mail!"
                    };
                }

                using(MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE email = @email", connection)) {
                    command.Parameters.AddWithValue("@email", email.ToString());

                    using MySqlDataReader reader = command.ExecuteReader();

                    if(reader.Read()) {
                        return new Response() {   
                            Error = true,
                            Field = "email",
                            Message = "This e-mail is already registered to a user!"
                        };
                    }
                }

                if(password == null || password.ToString().Length < 6) {
                    return new Response() {   
                        Error = true,
                        Field = "password",
                        Message = "Your password must be 6 letters or more!"
                    };
                }

                string key = Guid.NewGuid().ToString();

                while(true) {
                    using(MySqlCommand command = new MySqlCommand("SELECT * FROM links WHERE `key` = @key", connection)) {
                        command.Parameters.AddWithValue("@key", key);

                        using MySqlDataReader reader = command.ExecuteReader();

                        if(reader.Read()) {
                            key = Guid.NewGuid().ToString();

                            continue;      
                        }

                        break;
                    }
                }

                using(MySqlCommand command = new MySqlCommand("INSERT INTO links (`key`, redirect) VALUES (@key, @redirect)", connection)) {
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@redirect", "/registration/verification?key=" + key);

                    command.ExecuteNonQuery();

                    Program.Links.Add(key, "/registration/verification?key=" + key);
                }

                Program.Smtp.Send(new MailMessage(new MailAddress("noreply@cortex5.io", "Project Cortex"), new MailAddress(email.ToString(), name.ToString())) {
                    Subject = "Registration",

                    Sender = new MailAddress("noreply@cortex5.io", "Project Cortex"),
                    
                    IsBodyHtml = true,

                    Body = Pages.PageManager.Get(null, "Pages/email.html", new Dictionary<string, string>() {
                        { "title", $"Hey there {name.ToString()}!" },
                        { "body", Pages.PageManager.Get(null, "Pages/email/verification.html", new Dictionary<string, string>() {
                            { "key", key }
                            })
                        }
                    })
                });

                int user = -1;

                using(MySqlCommand command = new MySqlCommand("INSERT INTO users (name, email, password) VALUES (@name, @email, @password)", connection)) {
                    command.Parameters.AddWithValue("@name", name.ToString());
                    command.Parameters.AddWithValue("@email", email.ToString());
                    command.Parameters.AddWithValue("@password", Security.Hashing.HashPassword(password.ToString()));
                    
                    command.ExecuteNonQuery();

                    user = (int)command.LastInsertedId;
                }

                using(MySqlCommand command = new MySqlCommand("INSERT INTO user_keys (user, `key`, address, type) VALUES (@user, @key, @address, @type)", connection)) {
                    command.Parameters.AddWithValue("@user", user);
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@address", context.Request.RemoteEndPoint.Address.ToString());
                    command.Parameters.AddWithValue("@type", "email");

                    command.ExecuteNonQuery();
                }

                key = Guid.NewGuid().ToString();

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

                return new Response() {   
                    Error = false
                };
            }

            return null;
        }
    }
}
