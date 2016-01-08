using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Decchi.PublishingModule.Twitter
{
    public class OAuth
    {
        public class TokenPair
        {
            public TokenPair()
            {
            }
            public TokenPair(string token, string secret)
            {
                this.Token  = token;
                this.Secret = secret;
            }
            public string Token { get; set; }
            public string Secret { get; set; }
        }

        static OAuth()
        {
            ServicePointManager.Expect100Continue = false;
        }

        public OAuth(string appToken, string appSecret)
            : this(appToken, appSecret, null, null)
        {
        }
        public OAuth(string appToken, string appSecret, string userToken, string userSecret)
        {
            this.App  = new TokenPair(appToken,  appSecret);
            this.User = new TokenPair(userToken, userSecret);
        }

        public TokenPair App    { get; private set; }
        public TokenPair User   { get; private set; }

        private static string[] oauth_array = { "oauth_consumer_key", "oauth_version", "oauth_nonce", "oauth_signature", "oauth_signature_method", "oauth_timestamp", "oauth_token", "oauth_callback" };

        public WebRequest MakeRequest(string method, string url, object data = null)
        {
            method = method.ToUpper();
            var uri = new Uri(url);
            var dic = new SortedDictionary<string, object>();

            if (!string.IsNullOrEmpty(uri.Query))
                OAuth.AddDictionary(dic, uri.Query);

            if (data != null)
                OAuth.AddDictionary(dic, data);

            if (!string.IsNullOrWhiteSpace(this.User.Token))
                dic.Add("oauth_token", Uri.EscapeDataString(this.User.Token));

            dic.Add("oauth_consumer_key", Uri.EscapeDataString(this.App.Token));
            dic.Add("oauth_nonce", OAuth.GetNonce());
            dic.Add("oauth_timestamp", OAuth.GetTimeStamp());
            dic.Add("oauth_signature_method", "HMAC-SHA1");
            dic.Add("oauth_version", "1.0");

            var hashKey = string.Format(
                "{0}&{1}",
                Uri.EscapeDataString(this.App.Secret),
                this.User.Secret == null ? null : Uri.EscapeDataString(this.User.Secret));
            var hashData = string.Format(
                    "{0}&{1}&{2}",
                    method.ToUpper(),
                    Uri.EscapeDataString(string.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Host, uri.AbsolutePath)),
                    Uri.EscapeDataString(OAuth.ToString(dic)));

            using (var hash = new HMACSHA1(Encoding.UTF8.GetBytes(hashKey)))
                dic.Add("oauth_signature", Uri.EscapeDataString(Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(hashData)))));

            var sbData = new StringBuilder();
            sbData.Append("OAuth ");
            foreach (var st in dic)
                if (Array.IndexOf<string>(oauth_array, st.Key) >= 0)
                    sbData.AppendFormat("{0}=\"{1}\",", st.Key, Convert.ToString(st.Value));
            sbData.Remove(sbData.Length - 1, 1);

            var str = sbData.ToString();

            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = method;
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.UserAgent = "Decchi";
            req.Headers.Add("Authorization", sbData.ToString());
           
            if (method == "POST")
                req.ContentType = "application/x-www-form-urlencoded";

            return req;
        }

        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private static string GetNonce()
        {
            return rnd.Next(int.MinValue, int.MaxValue).ToString("X");
        }

        private static DateTime GenerateTimeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private static string GetTimeStamp()
        {
            return Convert.ToInt64((DateTime.UtcNow - GenerateTimeStampDateTime).TotalSeconds).ToString();
        }

        private static string ToString(IDictionary<string, object> dic)
        {
            var sb = new StringBuilder();

            if (dic.Count > 0)
            {
                foreach (var st in dic)
                    if (st.Value is bool)
                        sb.AppendFormat("{0}={1}&", st.Key, (bool)st.Value ? "true" : "false");
                    else
                        sb.AppendFormat("{0}={1}&", st.Key, Convert.ToString(st.Value));

                if (sb.Length > 0)
                    sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public static string ToString(object values)
        {
            var sb = new StringBuilder();

            string name;
            object value;

            foreach (var p in values.GetType().GetProperties())
            {
                if (!p.CanRead) continue;

                name  = p.Name;
                value = p.GetValue(values, null);

                if (value is bool)
                    sb.AppendFormat("{0}={1}&", name, (bool)value ? "true" : "false");
                else
                    sb.AppendFormat("{0}={1}&", name, Uri.EscapeDataString(Convert.ToString(value)));
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private static void AddDictionary(IDictionary<string, object> dic, string query)
        {
            if (!string.IsNullOrEmpty(query) || (query.Length > 1))
            {
                int read = 0;
                int find = 0;

                if (query[0] == '?')
                    read = 1;

                string key, val;

                while (read < query.Length)
                {
                    find = query.IndexOf('=', read);
                    key = query.Substring(read, find - read);
                    read = find + 1;

                    find = query.IndexOf('&', read);
                    if (find > 0)
                    {
                        if (find - read == 1)
                            val = null;
                        else
                            val = query.Substring(read, find - read);

                        read = find + 1;
                    }
                    else
                    {
                        val = query.Substring(read);

                        read = query.Length;
                    }

                    dic[key] = val;
                }
            }
        }

        private static void AddDictionary(IDictionary<string, object> dic, object values)
        {
            object value;

            foreach (var p in values.GetType().GetProperties())
            {
                if (!p.CanRead) continue;
                value = p.GetValue(values, null);

                if (value is bool)
                    dic[p.Name] = (bool)value ? "true" : "false";
                else
                    dic[p.Name] = Uri.EscapeDataString(Convert.ToString(value));


            }
        }

        public TokenPair RequestToken()
        {
            try
            {
                var req = MakeRequest("POST", "https://api.twitter.com/oauth/request_token");
                using (var res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    var str = reader.ReadToEnd();

                    var token = new TokenPair();
                    token.Token  = Regex.Match(str, @"oauth_token=([^&]+)").Groups[1].Value;
                    token.Secret = Regex.Match(str, @"oauth_token_secret=([^&]+)").Groups[1].Value;

                    return token;
                }
            }
            catch
            {
                return null;
            }
        }

        public TokenPair AccessToken(string verifier)
        {
            try
            {
                var obj = new { oauth_verifier = verifier };
                var buff = Encoding.UTF8.GetBytes(OAuth.ToString(obj));

                var req = MakeRequest("POST", "https://api.twitter.com/oauth/access_token", obj);
                req.GetRequestStream().Write(buff, 0, buff.Length);

                using (var res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    var str = reader.ReadToEnd();

                    var token = new TokenPair();
                    token.Token  = Regex.Match(str, @"oauth_token=([^&]+)").Groups[1].Value;
                    token.Secret = Regex.Match(str, @"oauth_token_secret=([^&]+)").Groups[1].Value;

                    return token;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
