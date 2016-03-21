using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        
        public readonly static IParseRule[]  Rules;
        public readonly static IParseRule[]  RulesPipe;
        public readonly static IParseRule[]  RulesPlayer;

        static SongInfo()
        {
            var type = typeof(IParseRule);
            Rules = type.Assembly
                        .GetTypes()
                        .Where(e => e.IsClass && e.IsSealed && e.IsSubclassOf(type))
                        .Select(e => (IParseRule)Activator.CreateInstance(e))
                        .OrderBy(e => e.Client)
                        .ToArray();

            RulesPipe = Rules.Where(e => !string.IsNullOrEmpty(e.PipeName))
                             .ToArray();

            RulesPlayer = Rules.Where(e => (e.ParseFlag & ParseFlags.WebBrowser) != ParseFlags.WebBrowser)
                               .ToArray();
        }

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

        private static int m_checkPipe;
        public static void CheckPipe()
        {
            if (Interlocked.CompareExchange(ref m_checkPipe, 1, 0) == 1) return;

            foreach (var pipeRule in SongInfo.RulesPipe)
            {
                if (!pipeRule.IsInstalled)
                    if (SongInfo.GetDataFromPipe(pipeRule) != null)
                        pipeRule.IsInstalled = true;
            }

            Interlocked.CompareExchange(ref m_checkPipe, 0, 1);
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

                for (int i = 0; i < SongInfo.Rules.Length; ++i)
                {
                    App.Debug("===== " + SongInfo.Rules[i].Client);
                    GetCurrentPlayingSong(SongInfo.Rules[i], webPages);
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
                Parallel.ForEach(SongInfo.Rules, e => GetCurrentPlayingSong(e, webPages));
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
                
        private static void GetCurrentPlayingSong(IParseRule rule, IList<WBResult> webPages)
        {
            if ((rule.ParseFlag & ParseFlags.WebBrowser) != ParseFlags.WebBrowser)
                SongInfo.GetFromPlayer(rule);
            else
                SongInfo.GetFromWebBrowser(rule, webPages);
        }

        private static bool AddToList(SongInfo info)
        {
            SongInfo.GetTagsFromFile(info);

            if (!string.IsNullOrWhiteSpace(info.Title))
            {
                lock (SongInfo.m_lastResult)
                    SongInfo.m_lastResult.Add(info);
                return true;
            }
            return false;
        }

        public static IntPtr GetWindowHandle(IParseRule rule)
        {
            var hwnd = NativeMethods.FindWindow(rule.WndClass, null);
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;

			if ( rule.WndClassTop )
				//return NativeMethods.GetParent(hwnd) == IntPtr.Zero ? hwnd : IntPtr.Zero;
				return hwnd;
			else
				return hwnd;
        }

        private static void GetFromPlayer(IParseRule rule)
        {
            SongInfo si = null;
            IntPtr hwnd = IntPtr.Zero;

            // 핸들 체크
            hwnd = GetWindowHandle(rule);
            if (hwnd == IntPtr.Zero) return;

            App.Debug("Handle : {0} : 0x{1:X8}", rule.WndClass, hwnd);

            // 파이프 : 성공시 return
            if ((rule.ParseFlag & ParseFlags.Pipe) == ParseFlags.Pipe)
                if (GetTagsFromPipe(rule, hwnd))
                    return;

            // 로컬 파일 인식
            if ((rule.ParseFlag & ParseFlags.Local) == ParseFlags.Local && Globals.Instance.DetectLocalFile)
                GetLocalFile(rule, hwnd, si);

            // 추가 파싱 : 성공하면 Return
            if ((rule.ParseFlag & ParseFlags.ManualOne) == ParseFlags.ManualOne)
            {
                if (rule.Get(si ?? (si = new SongInfo(rule)), hwnd))
                {
                    App.Debug("Manual Parse");

                    SongInfo.AddToList(si);

                    return;
                }
            }
            if ((rule.ParseFlag & ParseFlags.ManualMulti) == ParseFlags.ManualMulti)
            {
                var manual = rule.GetList(hwnd);
                if (manual != null)
                {
                    App.Debug("Manual Parse Multi");

                    foreach (var info in manual)
                        SongInfo.AddToList(info);

                    return;
                }
            }

            // 타이틀 파싱 : 성공하면 Return
            if ((rule.ParseFlag & ParseFlags.Title) == ParseFlags.Title)
                if (GetTagsFromTitle(rule, hwnd, si))
                    return;
        }
        
        private static void GetFromWebBrowser(IParseRule rule, IList<WBResult> webPages)
        {
            App.Debug("Web");

            string title;
            Match match;

            for (int i = 0; i < webPages.Count; ++i)
            {
                match = rule.Regex.Match(webPages[i].Title);
                if (!match.Success) continue;

                App.Debug("Match : " + webPages[i].ToString());

                title = match.Groups["title"].Value.Trim();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var si = new SongInfo(rule);
                    si.Title = title;

                    if (webPages[i].Url != null && rule.UrlRegex.IsMatch(webPages[i].Url))
                        si.Url = webPages[i].Url;

                    si.Handle = webPages[i].Handle;
                    si.MainTab = webPages[i].MainTab;

                    SongInfo.AddToList(si);
                }
            }
        }

        private static string GetDataFromPipe(IParseRule rule)
        {
            using (var stream = new NamedPipeClientStream(".", rule.PipeName, PipeDirection.In))
            {
                try
                {
                    stream.Connect(1000);

                    if (stream.IsConnected)
                        using (var reader = new StreamReader(stream))
                            return reader.ReadToEnd();
                }
                catch
                { }
            }

            return null;
        }

        private static bool GetTagsFromPipe(IParseRule rule, IntPtr hwnd, string data = null)
        {
            App.Debug("Pipe");
            if (data == null)
                data = GetDataFromPipe(rule);

            if (data != null)
            {
                App.Debug(data);

                var si = new SongInfo(rule);
                si.Handle = hwnd;

                var split = data.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                int sep;
                uint num;
                string key, val;
                for (int read = 0; read < split.Length; ++read)
                {
                    if ((sep = split[read].IndexOf('=')) > 0)
                    {
                        key = split[read].Substring(0, sep).Trim();
                        val = split[read].Substring(sep + 1).Trim();

                        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(val))
                            continue;

                        switch (key.ToLower())
                        {
                            case "title":  si.Title  = val; break;
                            case "album":  si.Album  = val; break;
                            case "artist": si.Artist = val; break;

                            // 확장 태그들
                            case "track":  if (uint.TryParse(val, out num)) si.Track  = num; break;
                            case "ttrack": if (uint.TryParse(val, out num)) si.TTrack = num; break;
                            case "disc":   if (uint.TryParse(val, out num)) si.Disc   = num; break;
                            case "tdisc":  if (uint.TryParse(val, out num)) si.TDisc  = num; break;
                            case "year":   if (uint.TryParse(val, out num)) si.Year   = num; break;
                            case "genre":  si.Genre  = val; break;

                            case "path":   si.Local  = val; break;
                            case "cover":  si.Cover  = new MemoryStream(Convert.FromBase64String(val)); si.Cover.Position = 0; break;
                            case "handle": si.Handle = new IntPtr(long.Parse(val)); break;
                        }
                    }
                }

                SongInfo.AddToList(si);
                return true;
            }

            return false;
        }

        private static void GetLocalFile(IParseRule rule, IntPtr hwnd, SongInfo si)
        {
            var local = DetectOpenedFile.GetOpenedFile(hwnd);
            if (!string.IsNullOrEmpty(local) && File.Exists(local))
            {
                App.Debug("Local");
                App.Debug(local);
                si = new SongInfo(rule);
                si.Local = local;
            }
        }

        private static bool GetTagsFromTitle(IParseRule rule, IntPtr hwnd, SongInfo si, string title = null)
        {
            if (title == null)
                title = NativeMethods.GetWindowTitle(hwnd);
            if (string.IsNullOrEmpty(title)) return false;
            App.Debug("Title");
            App.Debug(title);
                        
            if ((rule.ParseFlag & ParseFlags.ManualParse) == ParseFlags.ManualParse)
            {
                if (rule.ParseTitle(ref si, title))
                {
                    SongInfo.AddToList(si);
                    return true;
                }
                
                return false;
            }

            var match = rule.Regex.Match(title);
            if (!match.Success) return false;

            if (si == null) si = new SongInfo(rule);

            Group g;
            if (string.IsNullOrWhiteSpace(si.Title)  && (g = match.Groups["title"])  != null) si.Title  = g.Value.Trim();
            if (string.IsNullOrWhiteSpace(si.Artist) && (g = match.Groups["artist"]) != null) si.Artist = g.Value.Trim();
            if (string.IsNullOrWhiteSpace(si.Album)  && (g = match.Groups["album"])  != null) si.Album  = g.Value.Trim();

            si.Handle = hwnd;
            return SongInfo.AddToList(si);
        }

        private static void GetTagsFromFile(SongInfo si)
        {
            // front.jpg
            // cover.jpg
            // (filename).jpg
            // (album).jpg

            if (!string.IsNullOrWhiteSpace(si.Local) && File.Exists(si.Local))
            {
                // From MP3Tag
                using (var abs = new Abstraction(si.Local))
                {
                    try
                    {
                        using (var file = TagLib.File.Create(abs))
                        {
                            var tag = file.Tag;

                            if (string.IsNullOrWhiteSpace(si.Album)  && !string.IsNullOrWhiteSpace(tag.Album)) si.Album  = tag.Album.Trim();
                            if (string.IsNullOrWhiteSpace(si.Title)  && !string.IsNullOrWhiteSpace(tag.Title)) si.Title  = tag.Title.Trim();
                            if (string.IsNullOrWhiteSpace(si.Artist))
                            {
                                // 아티스트 순서 변경
                                     if (!string.IsNullOrWhiteSpace(tag.JoinedPerformers))   si.Artist = tag.JoinedPerformers  .Trim(); // Track Artist
                                else if (!string.IsNullOrWhiteSpace(tag.JoinedAlbumArtists)) si.Artist = tag.JoinedAlbumArtists.Trim(); // Album Artist
                                else if (!string.IsNullOrWhiteSpace(tag.JoinedComposers))    si.Artist = tag.JoinedComposers   .Trim(); // Composers
                            }

                            // 추가 태그들
                            if (si.Track  == 0 && tag.Track      > 0) si.Track  = tag.Track;
                            if (si.TTrack == 0 && tag.TrackCount > 0) si.TTrack = tag.TrackCount;
                            if (si.Disc   == 0 && tag.Disc       > 0) si.Disc   = tag.Disc;
                            if (si.TDisc  == 0 && tag.DiscCount  > 0) si.TDisc  = tag.DiscCount;
                            if (si.Year   == 0 && tag.Year       > 0) si.Year   = tag.Year;
                            if (string.IsNullOrWhiteSpace(si.Genre) && !string.IsNullOrWhiteSpace(tag.JoinedGenres)) si.Genre = tag.JoinedGenres.Trim();

                            if (file.Tag.Pictures.Length > 0 && si.Cover == null)
                            {
                                si.Cover = new MemoryStream(file.Tag.Pictures[0].Data.Data, false);
                                si.Cover.Position = 0;
                            }
                        }
                    }
                    catch
                    { }
                }

                if (si.Cover == null)
                {
                    // From Directory
                    var path = Path.GetDirectoryName(si.Local);
                    var dirName = Path.GetFileName(path).ToLower();
                    var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                    string filename;

                    for (var i = 0; i < files.Length; ++i)
                    {
                        switch (Path.GetExtension(files[i]).ToLower())
                        {
                            case ".jpg":
                            case ".png":
                                break;
                            default:
                                continue;
                        }

                        filename = Path.GetFileNameWithoutExtension(files[i]).ToLower();
                        if (filename.IndexOf("cover") >= 0  ||
                            filename.IndexOf("front") >= 0  ||
                            filename.IndexOf(dirName) >= 0  ||
                            (!string.IsNullOrEmpty(si.Title) && filename.IndexOf(si.Title) >= 0) ||
                            (!string.IsNullOrEmpty(si.Album) && filename.IndexOf(si.Album) >= 0))
                            si.Cover = new FileStream(files[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
            }
        }
        
        private static string m_befTitle;
        public static void DefaultADCallback(object args)
        {
            var rule = (IParseRule)args;

            var hwnd = SongInfo.GetWindowHandle(rule);
            if (hwnd == IntPtr.Zero) return;

            var pipe = (rule.ParseFlag & ParseFlags.Pipe) == ParseFlags.Pipe;
            
            var title = pipe ? SongInfo.GetDataFromPipe(rule) : NativeMethods.GetWindowTitle(hwnd);
            if (string.IsNullOrWhiteSpace(title)) return;

            if (SongInfo.m_befTitle != null)
            {
                // 같은 곡이 아님
                if (SongInfo.m_befTitle != title)
                {

                    var si = new SongInfo(rule);

                    if (pipe ? SongInfo.GetTagsFromPipe(rule, hwnd, title) : SongInfo.GetTagsFromTitle(rule, hwnd, si, title))
                        Task.Run(new Action(() => DecchiCore.Run(si)));
                }
            }

            SongInfo.m_befTitle = title;
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
                                b = str.IndexOf("/Title/")	>= 0 ||
                                    str.IndexOf("/Artist/")	>= 0 ||
                                    str.IndexOf("/Album/")	>= 0 || //
                                    str.IndexOf("/Client/")	>= 0 ||
                                    str.IndexOf("/Via/")	>= 0 || //
                                    str.IndexOf("/Track/")	>= 0 ||
                                    str.IndexOf("/TTrack/")	>= 0 ||
                                    str.IndexOf("/Disc/")	>= 0 ||
                                    str.IndexOf("/TDisc/")	>= 0 ||
                                    str.IndexOf("/Year/")	>= 0 ||
                                    str.IndexOf("/Genre/")	>= 0;

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
                                    str = str.Replace("/Track",  null);
                                    str = str.Replace("/TTrack", null);
                                    str = str.Replace("/Disc",   null);
                                    str = str.Replace("/TDisc",  null);
                                    str = str.Replace("/Year",   null);
                                    str = str.Replace("/Genre",  null);
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
            if (str.IndexOf(find) >= 0)
            {
                b |= !string.IsNullOrEmpty(replace);
                return str.Replace(find, replace);
            }
            return str;
        }
        private static string Replace(string str, string find, string replace, int length, bool isTitle, ref bool b)
        {
            string newReplace = null;
            if (str.IndexOf(find) >= 0)
            {
                if (isTitle)
                {
                    var ind = replace.IndexOf(ShortenUrlStr);
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

                b |= !string.IsNullOrEmpty(newReplace);
                return str.Replace(find, newReplace);
            }
            return str;
        }
    }
}
