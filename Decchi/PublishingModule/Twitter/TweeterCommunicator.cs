using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Decchi.ParsingModule;
using Decchi.Utilities;

namespace Decchi.PublishingModule.Twitter
{
    /// <summary>
    /// 트위터와 통신을 담당하는 TwitterCommunicator를 정의합니다.
    /// 이 클래스는 싱글턴 패턴으로 설계되었습니다. DecchiCore.Instance를 통해 인스턴스에 접근할 수 있습니다.
    /// IDecchiPublisher 인터페이스를 상속받은 퍼블리셔 클래스입니다.
    /// </summary>
    public partial class TwitterCommunicator : IDecchiPublisher
    {
        //const string Consumer_Key		= "";
        //const string Consumer_Secret	= "";
        
        private static TwitterCommunicator _instance = null;
        
        public static TwitterCommunicator Instance
        {
            get
            {
                return _instance ?? (_instance = new TwitterCommunicator());
            }
        }

        public OAuth OAuth { get; private set; }

        public bool Login()
        {
            var token  = Globals.Instance.TwitterToken;
            var secret = Globals.Instance.TwitterSecret;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(secret))
            {
                this.OAuth = new OAuth(Consumer_Key, Consumer_Secret);
                return false;
            }
            else
            {
                this.OAuth = new OAuth(Consumer_Key, Consumer_Secret, token, secret);
                return true;
            }
        }

        /// <summary>
        /// 트윗을 게시합니다.
        /// </summary>
        /// <param name="songinfo">트윗할 SongInfo 객체</param>
        /// <returns>트윗 성공 여부</returns>
        public bool Publish(SongInfo songinfo)
        {
            if (songinfo == null || !songinfo.Loaded) return false;
            
            var text = songinfo.ToString();
            if (string.IsNullOrEmpty(text)) return false;

            string mediaId = null;

            var stream = ImageResize.LoadImageResized(songinfo.Cover);
            if (stream != null)
            {
                try
                {
                    var req = OAuth.MakeRequest("POST", "https://upload.twitter.com/1.1/media/upload.json");

                    using (stream)
                    {
                        var buff = new byte[4096];
                        var read = 0;

                        var boundary = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                        var bef = Encoding.UTF8.GetBytes(string.Format("--{0}\r\nContent-Type: application/octet-stream\r\nContent-Disposition: form-data; name=\"media\";\r\n\r\n", boundary));
                        var aft = Encoding.UTF8.GetBytes(string.Format("\r\n\r\n--{0}--\r\n", boundary));

                        req.ContentType = "multipart/form-data; boundary=" + boundary;

                        using (var writer = new BinaryWriter(req.GetRequestStream()))
                        {
                            writer.Write(bef);
                            while ((read = stream.Read(buff, 0, 4096)) > 0)
                                writer.Write(buff, 0, read);
                            writer.Write(aft);
                        }
                    }

                    using (var res = req.GetResponse())
                    using (var reader = new StreamReader(res.GetResponseStream()))
                        mediaId = Regex.Match(reader.ReadToEnd(), "\"media_id_string\"[ \t]*:[ \t]*\"([^\"]+)\"").Groups[1].Value;
                }
                catch
                {
                    mediaId = null;
                }
            }

            //////////////////////////////////////////////////

            object obj;
            if (mediaId != null)
                obj = new { status = text, media_ids = mediaId };
            else
                obj = new { status = text };

            try
            {
                var buff = Encoding.UTF8.GetBytes(OAuth.ToString(obj));

                var req = OAuth.MakeRequest("POST", "https://api.twitter.com/1.1/statuses/update.json", obj);
                req.GetRequestStream().Write(buff, 0, buff.Length);

                using (var res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                    return reader.ReadToEnd() != null;
            }
            catch
            {
                return false;
            }
        }

        private TwitterUser me;

        /// <summary>
        /// 로그인된 유저 객체를 얻어옵니다.
        /// 유저 정보 갱신이 필요할 경우 RefrashMe() 메서드를 호출합니다.
        /// </summary>
        public TwitterUser Me
        {
            get
            {
                // ?? 쓰려고 했는데 API 에러나면 뻗음 ㅡㅡ
                if (me == null)
                {
                    try
                    {
                        var req = OAuth.MakeRequest("GET", "https://api.twitter.com/1.1/account/verify_credentials.json");

                        using (var res = req.GetResponse())
                        using (var reader = new StreamReader(res.GetResponseStream()))
                            me = TwitterUser.Parse(reader.ReadToEnd());
                    }
                    catch
                    { }
                }

                return me;
            }
        }

        /// <summary>
        /// 로그인된 유저 정보를 갱신하고, Me 객체를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public TwitterUser RefrashMe()
        {
            me = null;
            return this.Me;
        }
    }
}
