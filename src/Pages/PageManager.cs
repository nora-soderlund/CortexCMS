using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cortex.CMS.Pages {
    class PageManager {
        public static Dictionary<string, IPageRequest> Requests = new Dictionary<string, IPageRequest>() {
            { "/index", new Guest.Index() },

            { "/about-us", new Guest.AboutUs() },
            { "/maintenance", new Guest.Maintenance() },

            { "/registration", new Guest.Registration() },
            { "/registration/verification", new Guest.Registration.Verification() },
            { "/registration/discord", new Guest.Registration.Discord() },

            { "/home", new User.Home() },
            { "/hotel", new User.Hotel() },

            { "/404", new Errors.Error404() }
        };

        public static void Handle(PageRequestClient client, string file) {
            if(!Requests.ContainsKey(file)) {
                client.Response.Redirect("/404");

                return;
            }

            Dictionary<string, string> replacements = new Dictionary<string, string>();

            IPageRequest request = Requests[file];

            if(!request.GetAccess(client)) {
                client.Response.Redirect("/");

                return;
            }

            request.Evaluate(client);

            string title = request.GetTitle(client);
            replacements.Add("title", title == null ? "Project Cortex" : title + " - Project Cortex");

            if(client.Request.HttpMethod != "HEAD") {
                string body = request.GetBody(client);

                if(body != null)
                    replacements.Add("body", body);
            }

            if(request.GetPage(client)) {
                string page = Get(client, "page.html", replacements);

                replacements["body"] = page;

                Respond(client, Get(client, "index.html", replacements));
            }
            else
                Respond(client, Get(client, "index.html", replacements));
        }

        public static string Get(PageRequestClient client, string component, Dictionary<string, string> replacements) {
            string path = Path.Combine(new string[] { (string)Program.Config["cms"]["directories"]["cms"], "Components", component });

            string document = File.ReadAllText(path);

            replacements.TryAdd("guest", (client == null || client.User.Guest)?("guest"):("user"));

            if(client != null && !client.User.Guest) {
                replacements.TryAdd("name", client.User.Name);
            }

            foreach(KeyValuePair<string, string> replacement in replacements) {
                document = document.Replace("${" + replacement.Key + "}", replacement.Value);
            }

            document = Regex.Replace(document, @"\$\{.*?\}", "");

            MatchCollection matches = Regex.Matches(document, @"(\%{)(.*?)(\})");

            foreach(Match match in matches) {
                document = document.Replace(match.Value, Get(client, match.Groups[2].Value, new Dictionary<string, string>()));
            }
        
            return document;
        }

        public static void Respond(PageRequestClient client, string response, int status = 200) {
            if(client.Response.RedirectLocation != null)
                return;

            byte[] data = Encoding.UTF8.GetBytes(response);

            client.Response.StatusCode = status;
            client.Response.ContentType = "text/html";
            client.Response.ContentEncoding = Encoding.UTF8;
            client.Response.ContentLength64 = data.LongLength;

            client.Response.OutputStream.Write(data, 0, data.Length);

            client.Response.Close();
        }
    }
}
