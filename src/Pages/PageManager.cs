using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CortexCMS.Pages {
    class PageManager {
        public static Dictionary<string, IPageRequest> Requests = new Dictionary<string, IPageRequest>() {
            { "/index", new Guest.Index() },

            { "/about-us", new Guest.AboutUs() },
            { "/maintenance", new Guest.Maintenance() },

            { "/registration", new Guest.Registration() },
            { "/registration/verification", new Guest.Registration.Verification() },

            { "/home", new User.Home() },
            { "/hotel", new User.Hotel() },

            { "/404", new Errors.Error404() }
        };

        public static void Handle(PageRequestClient client) {
            if(!Requests.ContainsKey(client.Request.RawUrl)) {
                client.Response.Redirect("/404");

                return;
            }

            Dictionary<string, string> replacements = new Dictionary<string, string>();

            IPageRequest request = Requests[client.Request.RawUrl];

            if(!request.GetAccess(client)) {
                client.Response.Redirect("/");

                return;
            }

            string title = request.GetTitle(client);
            replacements.Add("title", title == null ? "Project Cortex" : title + " - Project Cortex");

            if(client.Request.HttpMethod != "HEAD") {
                string body = request.GetBody(client);

                if(body != null)
                    replacements.Add("body", body);
            }
            
            Respond(client, Get(client, "index.html", replacements));
        }

        public static string Get(PageRequestClient client, string component, Dictionary<string, string> replacements) {
            string path = Path.Combine(new string[] { Program.Directory, "Components", component });

            string document = File.ReadAllText(path);

            replacements.Add("guest", (client == null || client.User.Guest)?("guest"):("user"));

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
