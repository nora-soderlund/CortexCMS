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
using Newtonsoft.Json.Serialization;

using MySql.Data.MySqlClient;

using CortexCMS.Pages;

namespace CortexCMS {
    class Program {
        public static bool Maintenance = false;

        public static string Directory = @"C:\Cortex\v2\CortexCMS\src\Web";
        public static string DirectoryClient = @"C:\Cortex\Client";

        //public static string Database = "server=127.0.0.1;uid=root;database=cortex;SslMode=none";
        public static string Database = "server=127.0.0.1;uid=root;database=cortex;pwd=AfQ4P6!!;SslMode=none";

        public static JsonSerializerSettings JSON = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static SmtpClient Smpt = new SmtpClient("smtp.gmail.com") {
            Port = 587,
            Credentials = new NetworkCredential("prj.cortex@gmail.com", "AfQ4P6!!"),
            EnableSsl = true
        };

        public static void Main() {
            HttpListener listener = new HttpListener();

            //listener.Prefixes.Add("http://localhost:8080/");
            listener.Prefixes.Add("http://cortex5.io:80/");

            listener.Start();
            
            Console.WriteLine($"Listening for connections!");

            Console.WriteLine(Security.Hashing.HashPassword("123"));

            while(true) {
                HttpListenerContext context = listener.GetContext();
                
                ThreadPool.QueueUserWorkItem((e) => {
                    HandleRequest(context);
                });
            }
        }

        public static void HandleRequest(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try {
                Console.WriteLine($"Receiving request from {request.RemoteEndPoint.Address} for {request.RawUrl}");
                
                Console.WriteLine();

                string file = request.RawUrl.ToLower();

                string path = Path.Combine(new string[] { Directory, "public", file.Trim('/').Replace('/', '\\') });

                PageRequestClient client = new PageRequestClient(context);

                if(File.Exists(path)) {
                    Respond(context, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));
                }
                else if(file.LastIndexOf('.') != -1) {
                    if(file.StartsWith("/hotel/")) {
                        if(!client.User.Guest && client.User.Verified) {
                            path = Path.Combine(new string[] { DirectoryClient, file.Trim('/').Replace("hotel/", "").Replace('/', '\\') });

                            Respond(context, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));
                        }
                        else
                            context.Response.Redirect("/403");
                    }
                    else
                        Respond(context, Encoding.UTF8.GetBytes("File Not Found"), "text/html", 404);
                }
                else if(file == "/discord") {
                    response.Redirect("https://discord.gg/PScuBzeydM");
                }
                else if(file.StartsWith("/api")) {
                    API.APIManager.Handle(context);
                }
                else if(request.HttpMethod == "GET") {
                    if(file.Count(x => x == '-') == 4 && file.LastIndexOf('/') == 0) {
                        Guid guid = Guid.Parse(file.Substring(1));

                        using MySqlConnection connection = new MySqlConnection(Program.Database);
                        connection.Open();

                        using MySqlCommand command = new MySqlCommand("SELECT * FROM links WHERE `key` = @key", connection);
                        command.Parameters.AddWithValue("key", guid.ToString());

                        using MySqlDataReader reader = command.ExecuteReader();

                        if(reader.Read())
                            context.Response.Redirect(reader.GetString("redirect"));
                    }
                    else if(file == "/logout") {
                        context.Response.Headers.Add("Set-Cookie", $"key=null; expires={DateTime.UtcNow.ToString("dddd, dd-MM-yyyy hh:mm:ss GMT")}; path=/");
                        
                        context.Response.Redirect("/index");
                    }
                    else if(Program.Maintenance == true && file != "/maintenance") {
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