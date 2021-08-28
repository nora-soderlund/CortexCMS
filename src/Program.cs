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
        private static string prefix = "http://localhost:8080/";

        private static string directory = @"C:\Cortex\v2\CortexCMS\src\Web";

        private static List<IPage> pages = new List<IPage>() {
            { new Pages.Public.Index() }
        };

        public static void Main() {
            HttpListener listener = new HttpListener();

            listener.Prefixes.Add(prefix);

            listener.Start();
            
            Console.WriteLine($"Listening for connections on {prefix}");

            while(true) {
                HttpListenerContext context = listener.GetContext();
                
                ThreadPool.QueueUserWorkItem(async (e) => {
                    await HandleAsync(context);
                });
            }
        }

        public static async Task HandleAsync(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Handling new request");
            Console.ResetColor();

            Console.WriteLine(request.Url.ToString());
            Console.WriteLine(request.HttpMethod);
            Console.WriteLine(request.UserHostName);
            Console.WriteLine(request.UserAgent);
            Console.WriteLine();

            Console.WriteLine(request.RawUrl);
            Console.WriteLine(request.QueryString);

            Console.WriteLine();

            try {
                string file = request.RawUrl.ToLower();

                if(file == "/discord") {
                    response.Redirect("https://discord.gg/PScuBzeydM");

                    response.Close();

                    return;
                }

                string path = Path.Combine(new string[] { directory, "public", file.Trim('/').Replace('/', '\\') });

                if(File.Exists(path)) {
                    HandleResponse(response, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));

                    return;
                }

                if(path.LastIndexOf('.') != -1) {
                    response.StatusCode = 404;

                    HandleResponse(response, Encoding.UTF8.GetBytes("<!DOCTYPE html><html><head><title>Oops - Project Cortex</title></head><body>File Not Found</body></html>"));

                    return;
                }
                
                /*if(pages.Exists(x => x.Path == file)) {
                    IPage page = pages.Find(x => x.Path == file);

                    HandleResponse(response, Encoding.UTF8.GetBytes($"<!DOCTYPE><html><head>{page.GetHead()}</head><body>{page.GetBody()}</body></html>"));

                    return;
                }*/

                path = Path.Combine(new string[] { directory, "index.html" });
                
                HandleResponse(response, File.ReadAllBytes(path), MimeMapping.MimeUtility.GetMimeMapping(path));
                
                return;
            }
            catch(Exception exception) {
                byte[] data = Encoding.UTF8.GetBytes("<!DOCTYPE>" +
                    "<html>" +
                    "  <head>" +
                    "    <title>Oops!</title>" +
                    "  </head>" +
                    "  <body>" +
                    "    Something went wrong!" +
                    "  </body>" +
                    "</html>");

                response.ContentType = "text/html";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await response.OutputStream.WriteAsync(data, 0, data.Length);
            }

            // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
            /*if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
            {
                Console.WriteLine("Shutdown requested");
                runServer = false;
            }*/

            // Make sure we don't increment the page views counter if `favicon.ico` is requested
            //if (req.Url.AbsolutePath != "/favicon.ico")
            //    pageViews += 1;

            
            response.Close();
        }

        public static void HandleResponse(HttpListenerResponse response, byte[] data, string contentType = "text/html") {
            response.ContentType = contentType;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;

            response.OutputStream.Write(data, 0, data.Length);

            response.Close();
        }
    }
}