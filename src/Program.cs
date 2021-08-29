using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Threading.Tasks;

using MimeMapping;

using CortexCMS.Pages;

namespace CortexCMS {
    class Program {
        public static string Directory = @"C:\Cortex\v2\CortexCMS\src\Web";

        public static string Database = "server=127.0.0.1;uid=root;database=cortex;SslMode=none";

        public static void Main() {
            HttpListener listener = new HttpListener();

            listener.Prefixes.Add("http://localhost:8080/");
            //listener.Prefixes.Add("http://cortex5.io:80/");

            listener.Start();
            
            Console.WriteLine($"Listening for connections!");

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

                if(file.Length == 1) {
                    context.Response.Redirect("/index");
                }
                else if(File.Exists(path)) {
                    Respond(context, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));
                }
                else if(file.LastIndexOf('.') != -1) {
                    Respond(context, Encoding.UTF8.GetBytes("File Not Found"), "text/html", 404);
                }
                else if(file == "/discord") {
                    response.Redirect("https://discord.gg/PScuBzeydM");
                }
                else if(file.StartsWith("/api/")) {
                    API.APIManager.Handle(context);
                }
                else if(request.HttpMethod == "GET") {
                    Pages.PageManager.Handle(context);
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