using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Decchi.Core.Windows;
using Decchi.Utilities;

namespace Decchi.ParsingModule
{
    [DebuggerDisplay("{Client}")]
    public sealed class SongInfo
    {
        /// <summary>
        /// Dll 에 아래 함수를 public 으로 넣어두면 자동으로 인식합니다
        /// </summary>
        private delegate bool DllParse(out string title, out string album, out string artist, out Stream cover);

        public static SongInfo[]    SongInfos   { get; private set; }
        public static Assembly[]    Assemblies  { get; private set; } 

        public const string BaseURL = "https://raw.githubusercontent.com/Usagination/Decchi/songinfo/";
        /// <summary>
        /// Github 에서 Songinfo 데이터를 가져옵니다
        /// 
        /// [ClientName]
        /// clienticon=아이콘주소
        /// 
        /// wndclass=Regex 이용할때 FindWindow 할 주소
        /// wndClassTop=wndclass 가 최상위 핸들이여야만 사용합니다
        /// regex=regex
        /// 
        /// 동적 라이브러리
        /// dllext=추가 라이브러리
        /// dll=DllParse 함수가 있는 dll 파일
        /// 
        /// (사용안함)
        /// 플레이어 플러그인
        /// plugin=dll
        /// pipename=파이프 이름
        /// </summary>
        public static bool InitSonginfo()
        {
            try
            {
                var asm = new List<Assembly>();
                var lst = new List<SongInfo>();

                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.BaseAddress = BaseURL;

                    SongInfo cur = null;
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
                                cur = new SongInfo(line.Substring(1, line.Length - 2).Trim());
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
                                        cur.m_wndClass = val;
                                        break;

                                    case "wndclasstop":
                                        cur.m_wndClassTop = val == "1";
                                        break;

                                    case "regex":
                                        cur.m_regex = new Regex(val, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
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

                                    /*
                                    case "plugin":
                                        cur.Plugin = BaseURL + val;
                                        break;

                                    case "pipename":
                                        cur.m_pipeName = val;
                                        break;
                                    */
                                }
                            }
                        }
                    }
                }

                SongInfo.SongInfos  = lst.ToArray();
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
        private static void GetMethod(SongInfo cur, byte[] assemblyData)
        {
            try
            {
                cur.m_assmbly = Assembly.Load(assemblyData);
                if (cur.m_assmbly != null)
                {
                    MethodInfo dllParse = null;

                    foreach (var type in cur.m_assmbly.GetExportedTypes())
                    {
                        bool find = false;

                        foreach (var method in type.GetMethods())
                        {
                            if (IsMethodCompatibleWithDelegate(method, typeof(DllParse)))
                            {
                                dllParse = method;
                                find = true;
                                break;
                            }
                        }

                        if (find)
                            break;
                    }

                    if (dllParse != null)
                        cur.m_parse = (DllParse)Delegate.CreateDelegate(typeof(DllParse), null, dllParse);
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

        private SongInfo(string client)
        {
            this.Client = client;
        }

        public  string      Client      { get; private set; }
        public  BitmapImage ClientIcon  { get; private set; }
        private string      m_wndClass;
        private bool        m_wndClassTop;
        private Regex       m_regex;
        private Assembly    m_assmbly;
        private DllParse    m_parse;
        /*
        public  string      Plugin      { get; private set; } 
        private string      m_pipeName;
        */
        
        public  bool        Loaded      { get; private set; }

        public  string      Title       { get; private set; }
        public  string      Album       { get; private set; }
        public  string      Artist      { get; private set; }
        public  string      LocalPath   { get; private set; }
        public  Stream      Cover       { get; private set; }

        public const string Via = "#뎃찌NP";
        public const string defaultFormat = "{/Artist/의 }{/Title/{ (/Album/)}을/를 }듣고 있어요! {/Via/} - {/Client/} #NowPlaying";

        public static void Clear()
        {
            SongInfo info;

            for (int i = 0; i < SongInfos.Length; ++i)
            {
                info = SongInfos[i];
                info.Loaded     = false;

                info.Album      = null;
                info.Title      = null;
                info.Artist     = null;
                info.LocalPath  = null;
                if (info.Cover != null)
                {
                    try
                    {
                        info.Cover.Dispose();	
                    }
                    catch
                    { }
                    info.Cover = null;
                }
            }
        }

        private IntPtr GetWindowHandle()
        {
            var hwnd = NativeMethods.FindWindow(this.m_wndClass, null);
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;

            if (this.m_wndClassTop)
                return NativeMethods.GetParent(hwnd) == IntPtr.Zero ? hwnd : IntPtr.Zero;
            else
                return hwnd;
        }

        public bool GetCurrentPlayingSong( )
        {
            this.Loaded = false;

            var hwnd = !string.IsNullOrEmpty(this.m_wndClass) ? GetWindowHandle() : IntPtr.Zero;

            //////////////////////////////////////////////////
            // 열린 파일 감지
            if (hwnd != IntPtr.Zero && Globals.Instance.DetectLocalFile)
                this.LocalPath = DetectOpenedFile.GetOpenedFile(hwnd);

            //////////////////////////////////////////////////
            // 파이프
            /*
            if (m_pipeName != null)
            {
                using (var stream = new NamedPipeClientStream(".", this.m_pipeName, PipeDirection.In))
                {
                    try
                    {
                        stream.Connect(1000);
                    }
                    catch
                    { }

                    if (stream.IsConnected)
                    {
                        var buff = new byte[4096];
                        var read = 0;
                        read = stream.Read(buff, 0, 4096);

                        if (read > 0)
                        {
                            var split = Encoding.UTF8.GetString(buff, 0, read).Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                            int sep;
                            string key, val;
                            for (read = 0; read < split.Length; ++read)
                            {
                                if ((sep = split[read].IndexOf('=')) > 0)
                                {
                                    key = split[read].Substring(0, sep).Trim();
                                    val = split[read].Substring(sep + 1).Trim();

                                    switch (key.ToLower())
                                    {
                                        case "title":   this.Title      = val; break;
                                        case "album":   this.Album      = val; break;
                                        case "artist":  this.Artist     = val; break;
                                        case "path":    this.LocalPath  = val; break;
                                    }
                                }
                            }

                            this.Loaded = true;
                            return true;
                        }
                    }
                }
            }
            */

            //////////////////////////////////////////////////
            // 파일 경로에서 정보 얻어옴
            if (!string.IsNullOrEmpty(this.LocalPath) && File.Exists(this.LocalPath))
                GetTagsFromFile();

            //////////////////////////////////////////////////
            // 추가 DLL
            if (m_parse != null)
            {
                string title, album, artist;
                Stream stream;

                bool succ = m_parse.Invoke(out title, out album, out artist, out stream);

                if (succ)
                {
                    this.Title  = this.Title    ?? title;
                    this.Album  = this.Album    ?? album;
                    this.Artist = this.Artist   ?? artist;

                    if (stream != null)
                    {
                        if (this.Cover == null)
                            this.Cover = stream;
                        else
                            stream.Dispose();
                    }

                    this.Loaded = true;
                    return true;
                }
            }

            //////////////////////////////////////////////////
            // 타이틀 파싱
            if (hwnd != IntPtr.Zero)
            {
                var str = NativeMethods.GetWindowTitle(hwnd);
                if (string.IsNullOrEmpty(str)) return false;

                var match = this.m_regex.Match(str);

                if (!match.Success)
                    return false;

                Group g;

                this.Artist     = this.Artist       ?? ((g = match.Groups["aritis"])  != null ? g.Value : null);
                this.Title      = this.Title        ?? ((g = match.Groups["title"])   != null ? g.Value : null);
                this.Album      = this.Album        ?? ((g = match.Groups["album"])   != null ? g.Value : null);
                this.LocalPath  = this.LocalPath    ?? ((g = match.Groups["path"])    != null ? g.Value : null);

                this.Loaded = true;
                return true;
            }

            // Title
            if (this.m_regex != null)
            {
                var procs = Process.GetProcesses();

                bool find = false;

                Match match;
                Group g;

                for (int i = 0; i < procs.Length; i++)
                {
                    using (procs[i])
                    {
                        if (!find)
                        {
                            match = this.m_regex.Match(procs[i].MainWindowTitle);

                            if (!match.Success)
                                return false;

                            this.Artist     = this.Artist       ?? ((g = match.Groups["aritis"])  != null ? g.Value : null);
                            this.Title      = this.Title        ?? ((g = match.Groups["title"])   != null ? g.Value : null);
                            this.Album      = this.Album        ?? ((g = match.Groups["album"])   != null ? g.Value : null);
                            this.LocalPath  = this.LocalPath    ?? ((g = match.Groups["path"])    != null ? g.Value : null);

                            this.Loaded = true;
                            find = true;
                        }
                    }
                }

                return find;
            }

            return false;
        }

        private void GetTagsFromFile()
        {
            // front.jpg
            // cover.jpg
            // (filename).jpg
            // (album).jpg

            if (this.LocalPath != null && File.Exists(this.LocalPath))
            {
                // From MP3Tag
                using (var abs = new Abstraction(this.LocalPath))
                {
                    try
                    {
                        using (var file = TagLib.File.Create(abs))
                        {
                            var tag = file.Tag;

                            if (string.IsNullOrEmpty(this.Album)  && !string.IsNullOrEmpty(tag.Album))              this.Album  = tag.Album;
                            if (string.IsNullOrEmpty(this.Artist) && !string.IsNullOrEmpty(tag.JoinedPerformers))   this.Artist = tag.JoinedPerformers;
                            if (string.IsNullOrEmpty(this.Title)  && !string.IsNullOrEmpty(tag.Title))              this.Title  = tag.Title;

                            if (file.Tag.Pictures.Length > 0)
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
                    var path = Path.GetDirectoryName(this.LocalPath);
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
                            filename.IndexOf("folder") >= 0 ||
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
                                str = Replace(str, "/Title/",   info.Title,     ref b);
                                str = Replace(str, "/Artist/",  info.Artist,    ref b);
                                str = Replace(str, "/Album/",   info.Album,     ref b);
                                str = Replace(str, "/Client/",  info.Client,    ref b);
                                str = Replace(str, "/Via/",     SongInfo.Via,   ref b);

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
