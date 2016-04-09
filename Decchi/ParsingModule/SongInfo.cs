using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decchi.Core;
using Decchi.ParsingModule.WebBrowser;
using Decchi.Utilities;

namespace Decchi.ParsingModule
{
    [DebuggerDisplay("{Rule.Client} : {Title}")]
    public sealed class SongInfo : IComparable, IComparable<SongInfo>
    {
        public const string Via = "#뎃찌NP";
        public const string defaultFormat = "{/Artist/의 }{/Title/{ (/Album/{ CD/Disc/{//TDisc/}}{ #/Track/{//TTrack/}})}을(를) }듣고 있어요! {/Via/} - {/Client/} #NowPlaying";

        private const int    ShortenUrlLength = 24;
        private const string ShortenUrlStr    = @"@!^@@#$%^#$^@%&(%^&#^&*$";
        //                                        123456789012345678901234

        public IParseRule   Rule    { get; private set; }
        public IntPtr       Handle  { get; set; }
        public bool         MainTab { get; set; }

        public string   Title   { get; set; }
        public string   Album   { get; set; }
        public string   Artist  { get; set; }
        // 추가 태그들
        public uint     Track   { get; set; }
        public uint     TTrack  { get; set; }
        public uint     Disc    { get; set; }
        public uint     TDisc   { get; set; }
        public uint     Year    { get; set; }
        public string   Genre   { get; set; }

        public string   Url     { get; set; }
        public string   Local   { get; set; }
        public Stream   Cover   { get; set; }
        
        public SongInfo(IParseRule rule)
        {
            this.Rule = rule;
        }
        
        public int CompareTo(object other)
        {
            return this.CompareTo(other as SongInfo);
        }
        public int CompareTo(SongInfo other)
        {
            if (other == null) return 0;

            int i;

            i = this.Rule.Client.CompareTo(other.Rule.Client);
            if (i != 0) return i;
            
            var thisHandle  = this .Handle.ToInt64();
            var otherHandle = other.Handle.ToInt64();

            if (thisHandle > otherHandle) return  1;
            if (thisHandle < otherHandle) return -1;

            return this.Title.CompareTo(other.Title);
        }

        private readonly static List<SongInfo> m_lastResult = new List<SongInfo>();
        public static IList<SongInfo> LastResult { get { return SongInfo.m_lastResult; } } 
        public static IList<SongInfo> GetCurrentPlayingSong()
        {
            if (App.DebugMode)
            {
                App.Debug("===== Songinfo =====");
                App.Debug("===== Windows");
                App.Debug(OpenedWindow.GetOpenedWindows().Select(e => e.ToString()));

                App.Debug("===== Web Pages");
                var webPages = WBParser.Parse(Globals.Instance.WBDetailSearch);
                App.Debug(webPages.Select(e => e.ToString()));

                for (int i = 0; i < IParseRule.Rules.Length; ++i)
                {
                    App.Debug("===== " + IParseRule.Rules[i].Client);
                    IParseRule.Rules[i].GetCurrentPlayingSong(SongInfo.m_lastResult, webPages);
                }
                
                App.Debug("===== Result =====");
                SongInfo.m_lastResult.ForEach(e =>
                {
                    App.Debug("===== " + e.Rule.Client);
                    App.Debug("Title  : " + e.Title);
                    App.Debug("Artist : " + e.Artist);
                    App.Debug("Album  : " + e.Album);
                    App.Debug("Url    : " + e.Url);
                    App.Debug("Cover  : {0} Bytes", e.Cover != null ? e.Cover.Length : 0);
                    App.Debug("Local  : " + e.Local);
                    App.Debug("Handle : 0x" + e.Handle.ToString("X8"));
                    App.Debug("MainTab: " + (e.MainTab ? "1" : "0"));
                });
                App.Debug("===== END =====");
            }
            else
            {
                var webPages = WBParser.Parse(Globals.Instance.WBDetailSearch);
                Parallel.ForEach(IParseRule.Rules, e => e.GetCurrentPlayingSong(SongInfo.m_lastResult, webPages));
            }

            m_lastResult.Sort();

            return SongInfo.m_lastResult;   
        }
        public static void AllClear()
        {
            for (int i = 0; i < SongInfo.m_lastResult.Count; ++i)
                SongInfo.m_lastResult[i].Clear();

            SongInfo.m_lastResult.Clear();
        }
        public void Clear()
        {
            if (this.Cover != null)
                this.Cover.Dispose();
        }

        public static bool CheckFormat(string format)
        {
            try
            {
                ToFormat(format, null, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool m_firstToString = true;
        private static readonly string[] exts = { ".mp3", ".ogg", ".wav", ".aiff", ".asf", ".flac", ".mpc", ".wav", ".tak" };
        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool withLink)
        {
            // 이것저것 가공처리
            if (this.m_firstToString)
            {
                this.m_firstToString = false;

                // 끝에 확장자 제거
                if (SongInfo.exts.Any(e => this.Title.EndsWith(e)))
                    this.Title = this.Title.Substring(0, this.Title.LastIndexOf('.'));

                if ((this.Rule.ParseFlag & ParseFlags.Edit) == ParseFlags.Edit)
                    this.Rule.Edit(this);

                // Url 치환
                if (this.Url != null)
                    this.Title = string.Format("{0} {1} ", this.Title, ShortenUrlStr);
            }

            if (string.IsNullOrWhiteSpace(Globals.Instance.PublishFormat)) return null;

            var len = 140;
            if (withLink) len -= ShortenUrlLength;
                
            // 전부 파싱
            var str = ToFormat(Globals.Instance.PublishFormat, this, false, KeywordRule.All);
            if (str.Length > len)
            {
                // 추가 정보를 빼본다
                str = ToFormat(Globals.Instance.PublishFormat, this, false, KeywordRule.Title | KeywordRule.Artist | KeywordRule.Album);

                if (str.Length > len)
                {
                    // 앨범도 빼본다
                    str = ToFormat(Globals.Instance.PublishFormat, this, false, KeywordRule.Title | KeywordRule.Artist);
                    var tialLength = str.Length;

                    // 앨범의 문제가 아니라면
                    if (str.Length > len)
                    {
                        // 타이틀이 긴건지 확인
                        str = ToFormat(Globals.Instance.PublishFormat, this, false, KeywordRule.Title);

                        if (str.Length > len)
                            str = ToFormat(Globals.Instance.PublishFormat, this, false, KeywordRule.TitleFit, this.Title.Length - (str.Length - len));

                        // 타이틀이 긴게 아니면 아티스트가 길다
                        else
                            str = ToFormat(Globals.Instance.PublishFormat, this, false, KeywordRule.Title | KeywordRule.ArtistFit, this.Artist.Length - (tialLength - len));
                    }
                }
            }

            if (this.Url != null)
                str = str.Replace(ShortenUrlStr, this.Url);

            return str;
        }

        [Flags]
        private enum KeywordRule : ushort
        {
            Title     = 0x0001,
            Artist    = 0x0002,
            Album     = 0x0004,
            Extension = 0x0008,
            All       = 0x000F,
            TitleFit  = 0x0100,
            ArtistFit = 0x0200
        }

        // 트윗수 길이 맞추는 작업은 나중에하기
        private static string ToFormat(string format, SongInfo info, bool checkFormat = false, KeywordRule rule = KeywordRule.All, int fitSize = 0)
        {
            var current = new StringBuilder();
            var stack   = new Stack<StringBuilder>();

            string str;

            char c;
            bool b;

            int i = 0;
            while (i < format.Length)
            {
                // { 부터 } 까지다
                c = format[i++];

                switch (c)
                {
                    case '{':
                        {
                            stack.Push(current);
                            current = new StringBuilder();
                        }
                        break;

                    case '}':
                        {
                            str = current.ToString();

                            if (checkFormat)
                            {
                                b = str.IndexOf("/Title/",  StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/Artist/", StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/Album/",  StringComparison.CurrentCultureIgnoreCase)	>= 0 || //
                                    str.IndexOf("/Client/", StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/Via/",    StringComparison.CurrentCultureIgnoreCase)	>= 0 || //
                                    str.IndexOf("/Track/",  StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/TTrack/", StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/Disc/",   StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/TDisc/",  StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/Year/",   StringComparison.CurrentCultureIgnoreCase)	>= 0 ||
                                    str.IndexOf("/Genre/",  StringComparison.CurrentCultureIgnoreCase)	>= 0;

                                if (b)
                                {
                                    current = stack.Pop();
                                    current.Append(str);
                                }
                                else
                                {
                                    // { } 안에는 최소한 하나가 있어야함
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                b = false;

                                // b -> { } 안에 포멧 변환된게 있음
                                str = Replace(str, "/Client/", info.Rule.Client, ref b);
                                str = Replace(str, "/Via/", SongInfo.Via, ref b);

                                if ((rule & KeywordRule.TitleFit)  == KeywordRule.TitleFit)  str = Replace(str, "/Title/",  info.Title,  fitSize, true,  ref b);
                                if ((rule & KeywordRule.ArtistFit) == KeywordRule.ArtistFit) str = Replace(str, "/Artist/", info.Artist, fitSize, false, ref b);

                                str = Replace(str, "/Title/",  ((rule & KeywordRule.Title)  == KeywordRule.Title)  ? info.Title  : null, ref b);
                                str = Replace(str, "/Artist/", ((rule & KeywordRule.Artist) == KeywordRule.Artist) ? info.Artist : null, ref b);
                                str = Replace(str, "/Album/",  ((rule & KeywordRule.Album)  == KeywordRule.Album)  ? info.Album  : null, ref b);

                                // 추가 태그
                                if ((rule & KeywordRule.Extension) == KeywordRule.Extension)
                                {
                                    str = Replace(str, "/Track/",  info.Track  > 0 ? info.Track .ToString() : null, ref b);
                                    str = Replace(str, "/TTrack/", info.TTrack > 0 ? info.TTrack.ToString() : null, ref b);
                                    str = Replace(str, "/Disc/",   info.Disc   > 0 ? info.Disc  .ToString() : null, ref b);
                                    str = Replace(str, "/TDisc/",  info.TDisc  > 0 ? info.TDisc .ToString() : null, ref b);
                                    str = Replace(str, "/Year/",   info.Year   > 0 ? info.Year  .ToString() : null, ref b);
                                    str = Replace(str, "/Genre/",  info.Genre,                                      ref b);
                                }
                                else
                                {
                                    str = ReplaceWithComparison(str, "/Track",  null);
                                    str = ReplaceWithComparison(str, "/TTrack", null);
                                    str = ReplaceWithComparison(str, "/Disc",   null);
                                    str = ReplaceWithComparison(str, "/TDisc",  null);
                                    str = ReplaceWithComparison(str, "/Year",   null);
                                    str = ReplaceWithComparison(str, "/Genre",  null);
                                }

                                current = stack.Pop();
                                if (b) current.Append(str);
                            }
                        }
                        break;

                    case '\\':
                        {
                            if (i < format.Length)
                            {
                                switch (format[i])
                                {
                                    case '{':
                                    case '}':
                                        c = format[i];
                                        i++;
                                        break;
                                    case 'n':
                                        c = '\n';
                                        i++;
                                        break;
                                    case '\\':
                                        c = '\\';
                                        i++;
                                        break;
                                }
                            }

                            current.Append(c);
                        }
                        break;

                    default:
                        current.Append(c);
                        break;
                }
            }

            if (checkFormat && stack.Count > 0)
                throw new Exception();
            
            return !checkFormat ? current.ToString() : null;
        }

        private static string Replace(string str, string find, string replace, ref bool b)
        {
            if (str.IndexOf(find, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                b |= !string.IsNullOrWhiteSpace(replace);
                return ReplaceWithComparison(str, find, replace);
            }
            return str;
        }
        private static string Replace(string str, string find, string replace, int length, bool isTitle, ref bool b)
        {
            string newReplace = null;
            if (str.IndexOf(find, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                if (isTitle)
                {
                    var ind = replace.IndexOf(ShortenUrlStr, StringComparison.CurrentCultureIgnoreCase);
                    if (ind >= 0)
                    {
                        // "{etcPart} {ShortenUrlStr} "
                        var etcPart = replace.Substring(0, ind);

                        if (!string.IsNullOrWhiteSpace(etcPart))
                        {
                            // url 부분하고 양옆 스페이스바를 제외한 부분
                            var fitLength = length - ShortenUrlLength - 2;

                            if (etcPart.Length > fitLength)
                                etcPart = etcPart.Substring(0, fitLength - 3) + "...";

                            newReplace = etcPart + ShortenUrlStr;
                        }
                    }
                }

                if (newReplace == null)
                    newReplace = replace.Substring(0, length - 3) + "...";

                b |= !string.IsNullOrWhiteSpace(newReplace);
                return ReplaceWithComparison(str, find, newReplace);
            }
            return str;
        }
        public static string ReplaceWithComparison(string str, string oldValue, string newValue, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            var replace = !string.IsNullOrEmpty(newValue);

            int find;
            while ((find = str.IndexOf(oldValue, 0, StringComparison.CurrentCultureIgnoreCase)) != -1)
            {
                str = str.Remove(find, oldValue.Length);
                
                if (replace)
                    str = str.Insert(find, newValue);
            }
            return str;
        }
    }
}
