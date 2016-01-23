using System;
using System.Text.RegularExpressions;

namespace Decchi.ParsingModule.Rules
{
    internal sealed class Nicodong : IParseRule
    {
        public Nicodong() : base(            
            new IParseRuleOption
            {
                Client      = "Nicodong",
                ParseFlag   = ParseFlags.WebBrowser | ParseFlags.Edit,
                Regex       = "^(?<title>.+)( - Niconico Live)? - Niconico",
                UrlRegex    = @"^https?:\/\/(?:(?:www\.|live\.)?nicovideo\.jp\/watch|nico\.ms)\/([a-zA-Z0-9]+)(?:\?.*)?$",
                ClientIcon  = "nicodong"
            })
        { }
        
        public override void Edit(SongInfo si)
        {
            if (!string.IsNullOrWhiteSpace(si.Url))
            {
                var m = this.UrlRegex.Match(si.Url);
                if (!m.Success) return;

                if (si.Url.IndexOf("live") >= 0)
                    si.Url = "http://live.nicovideo.jp/watch/" + m.Groups[1].Value;
                else
                    si.Url = "http://nico.ms/" + m.Groups[1].Value;
            }
        }
    }
}
