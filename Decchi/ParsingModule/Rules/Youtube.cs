using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Decchi.ParsingModule.Rules
{
    internal sealed class Youtube : IParseRule
    {
        public Youtube() : base(            
            new IParseRuleOption
            {
                Client      = "Youtube",
                ParseFlag   = ParseFlags.WebBrowser | ParseFlags.Edit,
                Regex       = "^(?<title>.+) - YouTube",
                // 신난다!!!!
                // https://www.regex101.com/r/mT0cT7/3
                UrlRegex    = @"^(?:https?:\/\/)?(?:(?:www\.)?youtube\.com\/(?:(?:v\/)|(?:embed\/|watch(?:\/|\?)){1,2}(?:.*v=)?|.*v=)?|(?:www\.)?youtu\.be\/)([A-Za-z0-9_\-]+)&?.*$",
                ClientIcon  = "youtube"
            })
        { }

        private Regex m_playlist    = new Regex(@"list=([a-zA-Z0-9\-_]+)&?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex m_checkPublic = new Regex("<a [^>]*class=\"[^\"]*pl-header-play-all-overlay[^\"]*\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void Edit(SongInfo si)
        {
            if (!string.IsNullOrWhiteSpace(si.Url))
            {
                try
                {
                    // 플레이 리스트인지 확인한다
                    var ml = this.m_playlist.Match(new Uri(si.Url).Query);
                    var v  = this.UrlRegex.Match(si.Url).Groups[1].Value;
 
                    if (ml.Success)
                    {
                        var list = ml.Groups[1].Value;

                        // 공개 플레이리스트 확인
                        var req = WebRequest.Create("https://www.youtube.com/playlist?list=" + list) as HttpWebRequest;
                        req.Timeout = req.ReadWriteTimeout = 2000;

                        using (var res = req.GetResponse())
                        using (var stm = new StreamReader(res.GetResponseStream()))
                        {
                            var body = stm.ReadToEnd();
                            if (!this.m_checkPublic.IsMatch(body))
                            {
                                // 비공개 플레이 리스트
                                // 링크를 단일 링크로 재작성
                                si.Url = "https://youtu.be/" + v;
                            }
                            else
                            {
                                // 공개 플레이 리스트
                                // youtu.be 링크로 재작성
                                si.Url = string.Format("https://youtu.be/{0}?list={1}", v, list);
                            }
                        }
                    }
                    else
                    {
                        si.Url = "https://youtu.be/" + v;
                    }

                    SetThumbnail(si, v);
                }
                catch
                { }
            }
        }

        private void SetThumbnail(SongInfo si, string videoId)
        {
            var ms = new MemoryStream(64 * 1024);

            try
            {
                var req = WebRequest.Create(string.Format("http://img.youtube.com/vi/{0}/sddefault.jpg", videoId));
                var res = req.GetResponse();

                var resStream = res.GetResponseStream();

                var buff = new byte[4096];
                int read;
                while ((read = resStream.Read(buff, 0, 4096)) >= 0)
                    ms.Write(buff, 0, read);

                si.Cover = ms;
            }
            catch
            {
                ms.Dispose();
            }
        }
    }
}
