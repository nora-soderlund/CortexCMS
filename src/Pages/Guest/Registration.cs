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
    }
}
