using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Threading.Tasks;

using MimeMapping;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using MySql.Data.MySqlClient;

using CortexCMS.Pages;

namespace CortexCMS {
    class Program {
        public static JObject Config;
        public static Dictionary<string, string> Settings = new Dictionary<string, string>();

        public static string Directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string Database;

        public static JsonSerializerSettings JSON = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static SmtpClient Smtp;

        public static Dictionary<string, string> Links = new Dictionary<string, string>();

        public static void Main() {
            try {
                Console.WriteLine("Reading the configuration manifest...");

                Config = JObject.Parse(File.ReadAllText(Program.Directory + "/CortexCMS.json"));

                Console.WriteLine($"Connecting to the SMTP server at {Config["smtp"]["host"]}:{Config["smtp"]["port"]}...");

                Smtp = new SmtpClient((string)Config["smtp"]["host"]) {
                    Port = (int)Config["smtp"]["port"],
                    EnableSsl = (bool)Config["smtp"]["ssl"],
                    
                    Credentials = new NetworkCredential((string)Config["smtp"]["credentials"]["name"], (string)Config["smtp"]["credentials"]["password"])
                };

                Console.WriteLine($"Connecting to the MySQL server at {Config["mysql"]["credentials"]["name"]}@{Config["smtp"]["host"]}...");

                Database = $"server={Config["mysql"]["host"]};uid={Config["mysql"]["credentials"]["name"]};pwd={Config["mysql"]["credentials"]["password"]};database={Config["mysql"]["database"]};SslMode={Config["mysql"]["ssl"]}";

                using (MySqlConnection connection = new MySqlConnection(Database)) {
                    connection.Open();

                    using(MySqlCommand command = new MySqlCommand("SELECT * FROM settings", connection)) {
                        using MySqlDataReader reader = command.ExecuteReader();

                        while(reader.Read()) {
                            Settings.Add(reader.GetString("key"), reader.GetString("value"));
                        }
                    }

                    using(MySqlCommand command = new MySqlCommand("SELECT * FROM links", connection)) {
                        using MySqlDataReader reader = command.ExecuteReader();

                        while(reader.Read()) {
                            Links.Add(reader.GetString("key"), reader.GetString("redirect"));
                        }
                    }
                }

                Console.WriteLine("Starting to listen to connections to:");

                using HttpListener listener = new HttpListener();

                foreach(JToken prefix in Config["prefixes"]) {
                    Console.WriteLine("\t" + (string)prefix);

                    listener.Prefixes.Add((string)prefix);
                }

                listener.Start();

                while(true) {
                    HttpListenerContext context = listener.GetContext();
                    
                    ThreadPool.QueueUserWorkItem((e) => {
                        HandleRequest(context);
                    });
                }
            }
            catch(Exception exception) {
                Console.ForegroundColor = ConsoleColor.DarkRed;

                Console.WriteLine("An error occured in the main thread, application must exit:" + Environment.NewLine + "\t" + exception.Message);
                Console.WriteLine(exception.StackTrace);

                Console.Read();
            }
        }

        public static void HandleRequest(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try {
                Console.WriteLine($"Receiving request from {request.RemoteEndPoint.Address} for {request.RawUrl}");
                
                Console.WriteLine();

                string file = request.RawUrl.ToLower();

                int questionMark = file.LastIndexOf('?');

                if(questionMark != -1)
                    file = file.Substring(0, questionMark);

                Console.WriteLine(file);

                string path = Path.Combine(new string[] { (string)Program.Config["directories"]["cms"], "public", file.Trim('/').Replace('/', '\\') });

                if(File.Exists(path)) {
                    Respond(context, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));
                }
                else if(file.LastIndexOf('.') != -1) {
                    if(file.StartsWith("/cdn/")) {
                        PageRequestClient client = new PageRequestClient(context);

                        if(!client.User.Guest && client.User.Verified) {
                            path = Path.Combine(new string[] { (string)Program.Config["directories"]["client"], file.Trim('/').Replace("cdn/", "").Replace('/', '\\') });

                            Respond(context, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));
                        }
                        else
                            context.Response.Redirect("/403");
                    }
                    else
                        Respond(context, Encoding.UTF8.GetBytes("File Not Found"), "text/html", 404);
                }
                else if(file.StartsWith("/api")) {
                    API.APIManager.Handle(context);
                }
                else if(request.HttpMethod == "GET") {
                    PageRequestClient client = new PageRequestClient(context);

                    if(Links.ContainsKey(file.Substring(1))) {
                        context.Response.Redirect(Links[file.Substring(1)]);
                    }
                    else if(file == "/logout") {
                        context.Response.Headers.Add("Set-Cookie", $"key=null; expires={DateTime.UtcNow.ToString("dddd, dd-MM-yyyy hh:mm:ss GMT")}; path=/");
                        
                        context.Response.Redirect("/index");
                    }
                    else if(Program.Settings["maintenance"] == "true" && file != "/maintenance") {
                        context.Response.Redirect("/maintenance");
                    }
                    else if(file.Length == 1)
                        context.Response.Redirect((client.User.Guest)?("/index"):("/home"));
                    else
                        Pages.PageManager.Handle(client);
                }
                else {
                    Respond(context, Encoding.UTF8.GetBytes("Not Implemented"), "text/html", 501);
                }
            }
            catch(Exception exception) {
                Respond(context, Encoding.UTF8.GetBytes("Internal Server Error"), "text/html", 500);

                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }

            response.Close();
        }

        public static void Respond(HttpListenerContext context, byte[] data, string contentType = "text/html", int status = 200) {
            context.Response.StatusCode = status;
            context.Response.ContentType = contentType;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = data.LongLength;

            context.Response.OutputStream.Write(data, 0, data.Length);

            context.Response.Close();
        }
    }
}