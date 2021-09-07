using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using MySql.Data.MySqlClient;

using Cortex;

namespace Cortex.CMS.Pages.Guest {
    class Registration : IPageRequest {
        public string GetTitle(PageRequestClient client) {
            return "Registration";
        }
        
        public string GetBody(PageRequestClient client) {
            return PageManager.Get(client, "Pages/registration.html", new Dictionary<string, string>() {
                { "discord.oauth2.authorize", (string)Program.Config["discord"]["api"]["oauth2"]["authorize"] }
            });
        }

        public bool GetAccess(PageRequestClient client) {
            return client.User.Guest;
        }
        
        public class Verification : IPageRequest {
            public string GetTitle(PageRequestClient client) {
                return "Registration Verification";
            }
            
            public string GetBody(PageRequestClient client) {
                Dictionary<string, string> properties = new Dictionary<string, string>();

                if(!client.User.Guest && client.User.Verified) {
                    properties.Add("body", "You have already verified your e-mail!");
                }
                else if(client.Parameters.ContainsKey("key")) {
                    string key = client.Parameters["key"];

                    using MySqlConnection connection = new MySqlConnection(Program.Database);
                    connection.Open();

                    int user = 0;

                    using(MySqlCommand command = new MySqlCommand("SELECT * FROM user_keys WHERE `key` = @key AND type = @type", connection)) {
                        command.Parameters.AddWithValue("@key", key);
                        command.Parameters.AddWithValue("@type", "email");

                        using MySqlDataReader reader = command.ExecuteReader();

                        if(!reader.Read()) {
                            properties.Add("body", "We couldn't verify any users with that verification key!");
                            
                            return PageManager.Get(client, "Pages/registration/verification.html", properties);
                        }

                        user = reader.GetInt32("user");
                    }

                    using(MySqlCommand command = new MySqlCommand("UPDATE users SET verified = true WHERE id = @id", connection)) {
                        command.Parameters.AddWithValue("@id", user);
                        
                        command.ExecuteNonQuery();
                    }
                    
                    properties.Add("body", "You have verified your e-mail, thank you! You're now put on the BETA waiting list. You will receive an e-mail when you're invited for testing!");
                }
                else
                    properties.Add("body", "You have been sent a verification e-mail, follow the instructions there to enter the BETA waiting list!");

                return PageManager.Get(client, "Pages/registration/verification.html", properties);
            }

            public bool GetAccess(PageRequestClient client) {
                return true;
            }

            public void Evaluate(PageRequestClient client) {
                
            }
        }
        
        public class Discord : IPageRequest {
            public string GetTitle(PageRequestClient client) {
                return "Discord Registration";
            }
            
            public string GetBody(PageRequestClient client) {
                if(client.Parameters.ContainsKey("code")) {
                    JObject user = API.Discord.OAuth2.GetUser(API.Discord.OAuth2.GetToken(client.Parameters["code"]));

                    Logs.WriteConsole(user.ToString());

                    using MySqlConnection connection = new MySqlConnection(Program.Database);
                    connection.Open();

                    int id;

                    using(MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE discord = @discord", connection)) {
                        command.Parameters.AddWithValue("@discord", (ulong)user["id"]);

                        using MySqlDataReader reader = command.ExecuteReader();

                        if(!reader.Read()) {
                            return PageManager.Get(client, "Pages/registration/discord.html", new Dictionary<string, string>() {
                                { "body", "Your Discord account is not associated with any users!" }
                            });
                        }

                        id = reader.GetInt32("id");
                    }

                    using(MySqlCommand command = new MySqlCommand("UPDATE users SET email = @email WHERE id = @id AND email = ''", connection)) {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@email", user["email"]);
                        
                        command.ExecuteNonQuery();
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

                            client.Response.Headers.Add("Set-Cookie", $"key={key}; expires={DateTime.UtcNow.AddMonths(3).ToString("dddd, dd-MM-yyyy hh:mm:ss GMT")}; path=/");

                            break;
                        }
                    }

                    using(MySqlCommand command = new MySqlCommand("INSERT INTO user_keys (user, `key`, address) VALUES (@user, @key, @address)", connection)) {
                        command.Parameters.AddWithValue("@user", id);
                        command.Parameters.AddWithValue("@key", key);
                        command.Parameters.AddWithValue("@address", client.Request.RemoteEndPoint.Address.ToString());

                        command.ExecuteNonQuery();
                    }

                    client.Response.Redirect("/");

                    return null;
                }

                return PageManager.Get(client, "Pages/registration/discord.html", new Dictionary<string, string>() {
                    { "body", client.Parameters["code"] }
                });
            }

            public bool GetAccess(PageRequestClient client) {
                return client.User.Guest;
            }

            public void Evaluate(PageRequestClient client) {
                if(!client.Parameters.ContainsKey("code")) {
                    client.Response.Redirect((string)Program.Config["discord"]["api"]["oauth2"]["authorize"]);
                }
            }
        }
    }
}
