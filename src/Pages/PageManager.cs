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

            { "/404", new Errors.Error404() }
        };

        public static void Handle(HttpListenerContext context) {
            if(!Requests.ContainsKey(context.Request.RawUrl)) {
                context.Response.Redirect("/404");

                return;
            }

            Dictionary<string, string> replacements = new Dictionary<string, string>();

            IPageRequest request = Requests[context.Request.RawUrl];

            string title = request.GetTitle(context);
            replacements.Add("title", title == null ? "Project Cortex" : title + " - Project Cortex");

            if(context.Request.HttpMethod != "HEAD") {
                string body = request.GetBody(context);

                if(body != null)
                    replacements.Add("body", body);
            }
            
            Respond(context, Get(context, "index.html", replacements));
        }

        public static string Get(HttpListenerContext context, string component, Dictionary<string, string> replacements) {
            string path = Path.Combine(new string[] { Program.Directory, "Components", component });

            string document = File.ReadAllText(path);

            replacements.Add("guest", ((bool)API.APIManager.Evaluate(context, "/api/user/authorize", "GET")["guest"])?("guest"):("user"));

            foreach(KeyValuePair<string, string> replacement in replacements) {
                document = document.Replace("${" + replacement.Key + "}", replacement.Value);
            }

            document = Regex.Replace(document, @"\$\{.*?\}", "");

            MatchCollection matches = Regex.Matches(document, @"(\%{)(.*?)(\})");

            foreach(Match match in matches) {
                document = document.Replace(match.Value, Get(context, match.Groups[2].Value, new Dictionary<string, string>()));
            }
        
            return document;
        }

        public static void Respond(HttpListenerContext context, string response, int status = 200) {
            byte[] data = Encoding.UTF8.GetBytes(response);

            context.Response.StatusCode = status;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = data.LongLength;

            context.Response.OutputStream.Write(data, 0, data.Length);

            context.Response.Close();
        }
    }
}
