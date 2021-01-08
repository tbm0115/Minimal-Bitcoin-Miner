using System;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace MiniMiner.Model
{
    public class Pool
    {
        public Uri Url;
        public string User;
        public string Password;

        public Pool(string login) {
            int urlStart = login.IndexOf('@');
            int passwordStart = login.IndexOf(':');
            string user = login.Substring(0, passwordStart);
            string password = login.Substring(passwordStart + 1, urlStart - passwordStart - 1);
            string url = "http://" + login.Substring(urlStart + 1);
            Url = new Uri(url);
            User = user;
            Password = password;
        }

        private string InvokeMethod(string method, string paramString = null) {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Url);
            webRequest.Credentials = new NetworkCredential(User, Password);
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";

            string jsonParam = (paramString != null) ? "\"" + paramString + "\"" : "";
            string request = "{\"id\": 0, \"method\": \"" + method + "\", \"params\": [" + jsonParam + "]}";

            // serialize json for the request
            byte[] byteArray = Encoding.UTF8.GetBytes(request);
            webRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = webRequest.GetRequestStream())
                dataStream.Write(byteArray, 0, byteArray.Length);

            string reply = "";
            using (WebResponse webResponse = webRequest.GetResponse())
            using (Stream str = webResponse.GetResponseStream())
            using (StreamReader reader = new StreamReader(str))
                reply = reader.ReadToEnd();

            return reply;
        }

        public Work GetWork(bool silent = false) {
            return new Work(ParseData(InvokeMethod("getwork")));
        }

        private byte[] ParseData(string json) {
            Match match = Regex.Match(json, "\"data\": \"([A-Fa-f0-9]+)");
            if (match.Success) {
                string data = Utils.RemovePadding(match.Groups[1].Value);
                data = Utils.EndianFlip32BitChunks(data);
                return Utils.ToBytes(data);
            }
            throw new Exception("Didn't find valid 'data' in Server Response");
        }

        public bool SendShare(byte[] share) {
            string data = Utils.EndianFlip32BitChunks(Utils.ToString(share));
            string paddedData = Utils.AddPadding(data);
            string jsonReply = InvokeMethod("getwork", paddedData);
            Match match = Regex.Match(jsonReply, "\"result\": true");
            return match.Success;
        }
    }
}
