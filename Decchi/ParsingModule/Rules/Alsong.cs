using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Decchi.ParsingModule.Rules
{
    internal sealed class Alsong : IParseRule
    {
        public Alsong() : base(            
            new IParseRuleOption
            {
                Client      = "알송",
                ParseFlag   = ParseFlags.Default | ParseFlags.ManualParse,
                WndClass    = "ALSongUniWnd",
                WndClassTop = false,
                ClientIcon  = "alsong"

            })
        { }

        public override bool ParseTitle(ref SongInfo si, string title)
        {
            try
            {
                var alsongFormat = Registry.GetValue(@"HKEY_CURRENT_USER\Software\ESTsoft\ALSong\Param\Option\Title", "DescFormat", null) as string;
                if (alsongFormat == null) return false;

                alsongFormat = alsongFormat.Replace("%가수%", "(?<가수>.+)");
                alsongFormat = alsongFormat.Replace("%제목%", "(?<제목>.+)");
                alsongFormat = alsongFormat.Replace("%앨범%", "(?<앨범>.+)");
                alsongFormat = alsongFormat.Replace("%년도%", ".+");
                alsongFormat = alsongFormat.Replace("%설명%", ".+");
                alsongFormat = alsongFormat.Replace("%장르%", ".+");
                alsongFormat = alsongFormat.Replace("%파일%", ".+");
                alsongFormat = alsongFormat.Replace("%경로%", ".+");
                alsongFormat = alsongFormat.Replace("%트랙%", ".+");

				var regex = new Regex(alsongFormat, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                var match = regex.Match(title);
                if (!match.Success) return false;
				
                Group g;
				si = new SongInfo(this);

				si.Artist   = (g = match.Groups["가수"]) != null ? g.Value : null;
                si.Title    = (g = match.Groups["제목"]) != null ? g.Value : null;
                si.Album    = (g = match.Groups["앨범"]) != null ? g.Value : null;

                return true;
            }
            catch
            { }

            return false;
        }
    }
}
