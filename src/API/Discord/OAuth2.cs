using System;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CortexCMS.API.Discord {
    class OAuth2 {
        public static string GetToken(string code) {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/oauth2/token");

            webRequest.Method = "POST";
            
            string parameters = $"client_id={Program.Config["discord"]["client"]}&client_secret={Program.Config["discord"]["secret"]}&grant_type=authorization_code&code={code}&redirect_uri={Program.Config["discord"]["oauth2"]["redirect"]}";

            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);

            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;

            using(Stream postStream = webRequest.GetRequestStream()) {
                postStream.Write(byteArray, 0, byteArray.Length);

                postStream.Close();
            }

            string text;

            using(WebResponse response = webRequest.GetResponse()) {
                using Stream postStream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(postStream);

                text = reader.ReadToEnd();
            }

            JObject result = JObject.Parse(text);

            return (string)result["access_token"];
        }

        public static JObject GetUser(string token) {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/@me");
            webRequest.Method = "Get";
            webRequest.ContentLength = 0;
            webRequest.Headers.Add("Authorization", $"Bearer {token}");
            webRequest.ContentType = "application/x-www-form-urlencoded";
            
            string text;

            using (HttpWebResponse response1 = webRequest.GetResponse() as HttpWebResponse) {
                StreamReader reader1 = new StreamReader(response1.GetResponseStream());
                
                text = reader1.ReadToEnd();
            }

            return JObject.Parse(text);
        }
    }
}
