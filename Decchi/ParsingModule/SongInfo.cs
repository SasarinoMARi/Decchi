using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Decchi.Core.Windows;
using Decchi.ParsingModule.WebBrowser;
using Decchi.Utilities;

namespace Decchi.ParsingModule
{
    [DebuggerDisplay("{Rule.Client} : {Title}")]
    public sealed class SongInfo
    {
        public const string Via = "#뎃찌NP";
        public const string defaultFormat = "{/Artist/의 }{/Title/{ (/Album/)}을/를 }듣고 있어요! {/Via/} - {/Client/} #NowPlaying";

        [DebuggerDisplay("{Client}")]
        public sealed class SongInfoRule
        {
            public string       Client      { get; set; }
            public BitmapImage  ClientIcon  { get; set; }
            public string       WndClass    { get; set; }
            public bool         WndClassTop { get; set; }
            public string       UrlPart     { get; set; }
            public Regex        Regex       { get; set; }
            public Assembly     Assmbly     { get; set; }
            public DllParse     Parse       { get; set; }
            public DllParse2    Parse2      { get; set; }
            public string       PluginUrl   { get; set; }
            public string       PluginPipe  { get; set; }
        }

        // Dll 에 아래 함수를 public 으로 넣어두면 자동으로 인식합니다
        public delegate bool DllParse(out string title, out string album, out string artist, out Stream thumbnail, long option);
        public delegate IList<IDictionary<string, object>> DllParse2(long option);
        
        public static SongInfoRule[]    Rules      { get; private set; }
        public static SongInfoRule[]    RulesWithP { get; private set; }
        public static Assembly[]        Assemblies { get; private set; }

        public const string BaseURL = "https://raw.githubusercontent.com/Usagination/Decchi/songinfo/";
        public const string PluginBaseURI = "https://github.com/Usagination/Decchi/blob/songinfo/";
        /// <summary>
        /// Github 에서 Songinfo 데이터를 가져옵니다
        /// 
        /// [ClientName]
        /// ClientIcon      아이콘주소
        /// 
        /// wndClass        윈도우 핸들을 찾을때 사용하는 ClassName
        /// wndClassTop     wndclass 가 최상위 핸들이여야만 사용합니다
        /// urlpart         웹브라우저 인식할때 url 에 포함되어 있어야 하는거
        /// 
        /// 동적 라이브러리
        /// DLLExt          추가 라이브러리
        /// DLL             DllParse 함수가 있는 dll 파일
        /// 
        /// 플레이어 플러그인
        /// Plugin          뎃찌EXT 설치 안내 주소
        /// PluginPipe      뎃찌EXT 파이프 이름
        /// </summary>
        public static bool InitSonginfo()
        {
            try
            {
                var asm = new List<Assembly>();
                var lst = new List<SongInfoRule>();
                var lstPlugin = new List<SongInfoRule>();

                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.BaseAddress = BaseURL;
                    wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                    SongInfoRule cur = null;
                    string line, key, val;
                    int sep;

                    using (var stream = new StringReader(wc.DownloadString("songinfo.txt")))
                    {
                        while ((line = stream.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;

                            if (line.StartsWith("["))
                            {
                                cur = new SongInfoRule { Client = line.Substring(1, line.Length - 2).Trim() };
                                lst.Add(cur);
                            }
                            else if ((sep = line.IndexOf('=')) > 0)
                            {
                                key = line.Substring(0, sep).Trim().ToLower();
                                val = line.Substring(sep + 1).Trim();

                                switch (key)
                                {
                                    case "clienticon":
                                        cur.ClientIcon = (BitmapImage)MainWindow.Instance.Dispatcher.Invoke(new Func<string, BitmapImage>(GetBitmapImage), BaseURL + val);
                                        break;

                                    case "wndclass":
                                        cur.WndClass = val;
                                        break;

                                    case "wndclasstop":
                                        cur.WndClassTop = val == "1";
                                        break;

                                    case "UrlPart":
                                        cur.UrlPart = val;
                                        break;

                                    case "regex":
                                        cur.Regex = new Regex(val, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
                                        break;

                                    case "dll":
                                        GetMethod(cur, wc.DownloadData(val));
                                        break;

                                    case "dllext":
                                        try
                                        {
                                            asm.Add(Assembly.Load(wc.DownloadData(val)));
                                        }
                                        catch
                                        { }
                                        break;

                                    case "plugin":
                                        cur.PluginUrl = new Uri(new Uri(PluginBaseURI), val).ToString();
                                        lstPlugin.Add(cur);
                                        break;

                                    case "pluginpipe":
                                        cur.PluginPipe = val;
                                        break;
                                }
                            }
                        }
                    }
                }

                SongInfo.Rules      = lst.ToArray();
                SongInfo.RulesWithP = lstPlugin.ToArray();
                SongInfo.Assemblies = asm.ToArray();

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static BitmapImage GetBitmapImage(string url)
        {
            try
            {
                var image = new BitmapImage();
                image.CacheOption = BitmapCacheOption.OnDemand;
                image.CreateOptions = BitmapCreateOptions.DelayCreation;
                image.BeginInit();
                image.UriSource = new Uri(url);
                image.EndInit();

                return image;
            }
            catch
            {
                return null;
            }
        }
        private static void GetMethod(SongInfoRule cur, byte[] assemblyData)
        {
            try
            {
                cur.Assmbly = Assembly.Load(assemblyData);
                if (cur.Assmbly != null)
                {
                    MethodInfo dllParse  = null;
                    MethodInfo dllParse2 = null;

                    foreach (var type in cur.Assmbly.GetExportedTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (dllParse == null && IsMethodCompatibleWithDelegate(method, typeof(DllParse)))
                                dllParse = method;

                            if (dllParse2 == null && IsMethodCompatibleWithDelegate(method, typeof(DllParse2)))
                                dllParse2 = method;

                            if (dllParse != null && dllParse2 != null)
                                break;
                        }

                        if (dllParse != null && dllParse2 != null)
                            break;
                    }

                    if (dllParse2 != null)
                        cur.Parse2 = (DllParse2)Delegate.CreateDelegate(typeof(DllParse2), null, dllParse2);
                    else if (dllParse != null)
                        cur.Parse  = (DllParse )Delegate.CreateDelegate(typeof(DllParse ), null, dllParse);
                }
            }
            catch
            { }
        }
        private static bool IsMethodCompatibleWithDelegate(MethodInfo method, Type delegateType)
        {
            MethodInfo invoke = delegateType.GetMethod("Invoke");

            return invoke.ReturnType == method.ReturnType &&
                               invoke.GetParameters().Select(x => x.ParameterType)
                .SequenceEqual(method.GetParameters().Select(x => x.ParameterType));
        }

        public SongInfoRule Rule    { get; private set; } 
        public IntPtr       Handle  { get; private set; }
        public bool         MainTab { get; private set; }
        public string       Title   { get; private set; }
        public string       Album   { get; private set; }
        public string       Artist  { get; private set; }
        public string       Local   { get; private set; }
        public Stream       Cover   { get; private set; }

        private SongInfo(SongInfoRule rule)
        {
            this.Rule = rule;
        }

        public static IntPtr GetWindowHandle(SongInfoRule rule)
        {
            var hwnd = NativeMethods.FindWindow(rule.WndClass, null);
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;

            if (rule.WndClassTop)
                return NativeMethods.GetParent(hwnd) == IntPtr.Zero ? hwnd : IntPtr.Zero;
            else
                return hwnd;
        }

        public static SongInfo[] LastResult { get; private set; }
        public static SongInfo[] GetCurrentPlayingSong()
        {
            var lst = new List<SongInfo>();

            var webPages = WBParser.Parse(Globals.Instance.WBDetailSearch);

            Parallel.ForEach(SongInfo.Rules, e => GetCurrentPlayingSong(lst, e, webPages));

            return (LastResult = lst.ToArray());
        }
        public static void Clear()
        {
            for (int i = 0; i < LastResult.Length; ++i)
                if (LastResult[i].Cover != null)
                    LastResult[i].Cover.Dispose();
        }

        private static void GetCurrentPlayingSong(List<SongInfo> lst, SongInfoRule rule, WBResult[] WebPages)
        {
            SongInfo si = null;
            
            #region 파이프
            if (rule.PluginPipe != null)
            {
                string data = null;

                using (var stream = new NamedPipeClientStream(".", rule.PluginPipe, PipeDirection.In))
                {
                    try
                    {
                        stream.Connect(1000);

                        if (stream.IsConnected)
                            using (var reader = new StreamReader(stream))
                                data = reader.ReadToEnd();
                    }
                    catch
                    { }
                }

                if (data != null)
                {
                    lock (lst)
                        lst.Add((si = new SongInfo(rule)));
                    si.GetFromPipe(data);
                    if (si.Handle == IntPtr.Zero)
                        si.Handle = NativeMethods.FindWindow(rule.WndClass, null);
                    return;
                }
            }
            #endregion

            IntPtr hwnd = IntPtr.Zero;

            if (!string.IsNullOrWhiteSpace(rule.WndClass))
            {
                hwnd = GetWindowHandle(rule);
                if (hwnd == IntPtr.Zero) return;
            }

            #region 로컬 파일 인식
            if (hwnd != IntPtr.Zero && Globals.Instance.DetectLocalFile)
            {
                var local = DetectOpenedFile.GetOpenedFile(hwnd);
                if (!string.IsNullOrEmpty(local) && File.Exists(local))
                {
                    si = new SongInfo(rule);
                    si.Local = local;
                }
            }
            #endregion

            #region 추가 DLL
            if (rule.Parse != null)
            {
                string title, album, artist;
                Stream stream;

                bool succ = rule.Parse.Invoke(out title, out album, out artist, out stream, 0);

                if (succ)
                {
                    lock (lst)
                        lst.Add(si ?? (si = new SongInfo(rule)));
                    si.Title    = title;
                    si.Album    = album;
                    si.Artist   = artist;
                    si.Cover    = stream;
                    si.Handle   = hwnd;

                    si.GetTagsFromFile();

                    return;
                }
            }
            else if (rule.Parse2 != null)
            {
                var lstResult = rule.Parse2.Invoke(0);

                if (lstResult != null && lstResult.Count > 0)
                {
                    for (int i = 0; i < lstResult.Count; ++i)
                    {
                        lock (lst)
                            lst.Add((si = new SongInfo(rule)));
                        si.GetFromParseResult(lstResult[i]);
                    }
                    return;
                }
            }
            #endregion

            #region 타이틀 파싱
            if (hwnd != IntPtr.Zero && rule.Regex != null)
            {
                var str = NativeMethods.GetWindowTitle(hwnd);
                if (string.IsNullOrEmpty(str)) return;

                var match = rule.Regex.Match(str);
                if (!match.Success) return;

                if (si == null) si = new SongInfo(rule);

                Group g;
                si.Title    = si.Title  ?? ((g = match.Groups["title"])     != null ? g.Value : null);
                si.Album    = si.Album  ?? ((g = match.Groups["aritis"])    != null ? g.Value : null);
                si.Artist   = si.Artist ?? ((g = match.Groups["aritis"])    != null ? g.Value : null);
                si.Local    = si.Local  ?? ((g = match.Groups["aritis"])    != null ? g.Value : null);

                si.Handle = hwnd;
                si.GetTagsFromFile();

                if (!string.IsNullOrWhiteSpace(si.Title) ||
                    !string.IsNullOrWhiteSpace(si.Album) ||
                    !string.IsNullOrWhiteSpace(si.Artist))
                    lock (lst)
                        lst.Add(si);

                return;
            }
            #endregion

            #region 웹브라우저 파싱
            if (hwnd == IntPtr.Zero && rule.Regex != null)
            {
                Match match;

                for (int i = 0; i < WebPages.Length; ++i)
                {
                    match = rule.Regex.Match(WebPages[i].Title);
                    if (!match.Success) continue;

                    AddWebBrowser(lst, rule, match, WebPages[i].Url, WebPages[i].Handle, WebPages[i].MainTab);
                }
            }
            #endregion
        }

        private static void AddWebBrowser(List<SongInfo> lst, SongInfoRule rule, Match match, string url, IntPtr handle, bool mainTab)
        {
            var si = new SongInfo(rule);

            Group g;
            si.Title    = (g = match.Groups["title"])   != null ? g.Value : null;
            si.Album    = (g = match.Groups["album"])   != null ? g.Value : null;
            si.Artist   = (g = match.Groups["aritis"])  != null ? g.Value : null;

            if (!string.IsNullOrWhiteSpace(si.Title) ||
                !string.IsNullOrWhiteSpace(si.Album) ||
                !string.IsNullOrWhiteSpace(si.Artist))
            {
                if (url != null && url.IndexOf(rule.UrlPart, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    si.Title = string.Format("{0} {1} ", si.Title, url);

                si.Handle = handle;

                si.MainTab = mainTab;

                lock (lst)
                    lst.Add(si);
            }
        }

        private void GetFromPipe(string str)
        {
            var split = str.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            int sep;
            string key, val;
            for (int read = 0; read < split.Length; ++read)
            {
                if ((sep = split[read].IndexOf('=')) > 0)
                {
                    key = split[read].Substring(0, sep).Trim();
                    val = split[read].Substring(sep + 1).Trim();

                    switch (key.ToLower())
                    {
                        case "title":   this.Title  = val; break;
                        case "album":   this.Album  = val; break;
                        case "artist":  this.Artist = val; break;
                        case "path":    this.Local  = val; break;
                        case "cover":   this.Cover  = new MemoryStream(Convert.FromBase64String(val)); Cover.Position = 0; break;
                        case "handle":  this.Handle = new IntPtr(long.Parse(val)); break;
                    }
                }
            }

            this.GetTagsFromFile();
        }
        private void GetFromParseResult(IDictionary<string, object> dic)
        {
            if (dic.ContainsKey("title"))   this.Title  = (string)dic["title"];
            if (dic.ContainsKey("album"))   this.Album  = (string)dic["album"];
            if (dic.ContainsKey("artist"))  this.Artist = (string)dic["artist"];
            if (dic.ContainsKey("path"))    this.Local  = (string)dic["path"];
            if (dic.ContainsKey("cover"))   this.Cover  = (Stream)dic["cover"];
            if (dic.ContainsKey("handle"))  this.Handle = (IntPtr)dic["handle"];

            this.GetTagsFromFile();
        }

        private void GetTagsFromFile()
        {
            // front.jpg
            // cover.jpg
            // (filename).jpg
            // (album).jpg

            if (!string.IsNullOrWhiteSpace(this.Local) && File.Exists(this.Local))
            {
                // From MP3Tag
                using (var abs = new Abstraction(this.Local))
                {
                    try
                    {
                        using (var file = TagLib.File.Create(abs))
                        {
                            var tag = file.Tag;

                            if (string.IsNullOrWhiteSpace(this.Album)  && !string.IsNullOrWhiteSpace(tag.Album))              this.Album  = tag.Album;
                            if (string.IsNullOrWhiteSpace(this.Artist) && !string.IsNullOrWhiteSpace(tag.JoinedPerformers))   this.Artist = tag.JoinedPerformers;
                            if (string.IsNullOrWhiteSpace(this.Title)  && !string.IsNullOrWhiteSpace(tag.Title))              this.Title  = tag.Title;

                            if (file.Tag.Pictures.Length > 0 && this.Cover == null)
                            {
                                this.Cover = new MemoryStream(file.Tag.Pictures[0].Data.Data, false);
                                this.Cover.Position = 0;
                            }
                        }
                    }
                    catch
                    { }
                }

                if (this.Cover == null)
                {
                    // From Directory
                    var path = Path.GetDirectoryName(this.Local);
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
                            (string.IsNullOrEmpty(this.Title) && filename.IndexOf(this.Title) >= 0) ||
                            (string.IsNullOrEmpty(this.Album) && filename.IndexOf(this.Album) >= 0))
                            this.Cover = new FileStream(files[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
            }
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

        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool withLink)
        {
            try
            {
                return ToFormat(Globals.Instance.PublishFormat, this, false, withLink ? 125 : 140);
            }
            catch
            {
                return null;
            }
        }

        // 트윗수 길이 맞추는 작업은 나중에하기
        private static string ToFormat(string format, SongInfo info, bool checkFormat = false, int length = 140)
        {
            StringBuilder			total	= !checkFormat ? new StringBuilder() : null;
            
            StringBuilder			sb      = null;
            Queue<StringBuilder>	queue   = new Queue<StringBuilder>();
            string					str;

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
                            if (sb != null) queue.Enqueue(sb);
                            sb = new StringBuilder();
                        }
                        break;

                    case '}':
                        {
                            str = sb.ToString();

                            b = false;

                            if (checkFormat)
                            {
                                b = str.IndexOf("/Title/")	>= 0 ||
                                    str.IndexOf("/Artist/")	>= 0 ||
                                    str.IndexOf("/Album/")	>= 0 ||
                                    str.IndexOf("/Client/")	>= 0 ||
                                    str.IndexOf("/Via/")	>= 0;

                                if (b)
                                {
                                    if (queue.Count > 0)
                                    {
                                        sb = queue.Dequeue();
                                        sb.Append(str);
                                    }
                                    else
                                    {
                                        sb = null;
                                    }
                                }
                                else
                                {
                                    // { } 안에는 최소한 하나가 있어야함
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                // b -> { } 안에 포멧 변환된게 있음
                                str = Replace(str, "/Title/",   info.Title,         ref b);
                                str = Replace(str, "/Artist/",  info.Artist,        ref b);
                                str = Replace(str, "/Album/",   info.Album,         ref b);
                                str = Replace(str, "/Client/",  info.Rule.Client,   ref b);
                                str = Replace(str, "/Via/",     SongInfo.Via,       ref b);

                                if (b)
                                {
                                    if (queue.Count > 0)
                                    {
                                        sb = queue.Dequeue();
                                        sb.Append(str);
                                    }
                                    else
                                    {
                                        total.Append(str);
                                        sb = null;
                                    }
                                }
                                else
                                {
                                    if (queue.Count > 0)
                                        sb = queue.Dequeue();
                                    else
                                        sb = null;
                                }
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
                                }
                            }

                            if (sb != null)
                                sb.Append(c);
                            else if (!checkFormat)
                                total.Append(c);
                        }
                        break;

                    default:
                        if (sb != null)
                            sb.Append(c);
                        else if (!checkFormat)
                            total.Append(c);
                        break;
                }
            }

            if (checkFormat && (queue.Count > 0 || sb != null))
                throw new Exception();
            
            return !checkFormat ? total.ToString() : null;
        }

        private static string Replace(string str, string find, string replace, ref bool b)
        {
            if (str.IndexOf(find) >= 0 && !string.IsNullOrEmpty(replace))
            {
                b = true;
                return str.Replace(find, replace);
            }
            return str;
        }
    }
}
