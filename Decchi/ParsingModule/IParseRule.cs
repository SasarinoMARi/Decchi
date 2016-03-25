using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;
using Decchi.Core;
using Decchi.ParsingModule.WebBrowser;
using Decchi.Utilities;

namespace Decchi.ParsingModule
{
    [Flags]
    public enum ParseFlags : ushort
    {
        /// <summary>윈도우 타이틀을 이용해 재생 정보를 가져옵니다</summary>
        Title = 0x0001,
        /// <summary>로컬 파일 인식 기능을 사용하여 추가 정보를 가져옵니다</summary>
        Local = 0x0002,
        /// <summary>뎃찌EXT 와 파이프 통신을 하여 재생 정보를 가져옵니다</summary>
        Pipe = 0x0004,
        /// <summary>웹 브라우저 정보입니다. 이 플래그는 Title Local Pipe 플래그를 무시합니다</summary>
        WebBrowser = 0x0008,
        /// <summary>별도의 작업을 거쳐 재생정보를 하나 가져옵니다 (Get)</summary>
        ManualParse = 0x0010,
        /// <summary>정규식을 사용하지 않고 별도로 파싱작업을 진행합니다 (ParseTitle)</summary>
        ManualTitle = 0x0040,
        /// <summary>트윗하기 전 별도의 수정작업을 진행합니다</summary>
        Edit = 0x0080,

        /// <summary>Title + Local</summary>
        Default = 0x0003,
    }

    public class IParseRuleOption
    {
        /// <summary>외부에 노출되는 클라이언트 이름입니다.</summary>
        public string Client { get; set; }
        /// <summary>재생정보를 가져올 동작을 지정합니다</summary>
        public ParseFlags ParseFlag { get; set; }
        /// <summary>윈도우 타이틀을 인식할 정규식입니다. (웹 브라우저 포함)</summary>
        public string Regex { get; set; }
        /// <summary>
        /// ParseFlags.WebBrowser 전용
        /// 인식된 Url 과 정규식을 비교하여 일치하였을 때 재생 정보에 주소를 포함합니다
        /// </summary>
        public string UrlRegex { get; set; }
        /// <summary>
        /// 윈도우를 검색할 때 사용할 창의 ClassName 입니다
        /// 윈도우를 찾지 못할 경우에는 해당 플레이어의 검색을 중지합니다
        /// (ParseFlags.WebBrowser 가 설정되었을 경우 무시)
        /// </summary>
        public string WndClass { get; set; }
        /// <summary>검색된 윈도우가 최상위 핸들일 경우에만 다음으로 넘어갑니다</summary>
        public bool WndClassTop { get; set; }
        /// <summary>뎃찌EXT 안내 홈페이지 주소입니다</summary>
        public string PluginUrl { get; set; }
        /// <summary>파이프 통신할 파이프 이름입니다.</summary>
        public string PipeName { get; set; }
        /// <summary>Decchi/ParsingModule/Rules/Icons/ 에 위치한 클라이언트 이름입니다. 이 png 파일은 반드시 컴파일 옵션을 Resource 으로 지정하영 압니다</summary>
        public string ClientIcon { get; set; }
    }

    [DebuggerDisplay("{Client}")]
    public class IParseRule : IDisposable, INotifyPropertyChanged
    {
        protected const int RefreshTimeSpan = 30 * 1000;

        public readonly static IParseRule[]  Rules;
        public readonly static IParseRule[]  RulesPipe;
        public readonly static IParseRule[]  RulesPlayer;

        static IParseRule()
        {
            var type = typeof(IParseRule);
            Rules = type.Assembly
                        .GetTypes()
                        .Where(e => e.IsClass && e.IsSealed && e.IsSubclassOf(type))
                        .Select(e => (IParseRule)Activator.CreateInstance(e))
                        .OrderBy(e => e.Client)
                        .ToArray();

            RulesPipe = Rules.Where(e => !string.IsNullOrWhiteSpace(e.PipeName))
                             .ToArray();

            RulesPlayer = Rules.Where(e => (e.ParseFlag & ParseFlags.WebBrowser) != ParseFlags.WebBrowser)
                               .ToArray();
        }
        
        private static int m_checkPipe;
        public static void CheckPipe()
        {
            if (Interlocked.CompareExchange(ref m_checkPipe, 1, 0) == 1) return;

            foreach (var pipeRule in IParseRule.RulesPipe)
            {
                if (!pipeRule.IsInstalled)
                    if (pipeRule.GetDataFromPipe() != null)
                        pipeRule.IsInstalled = true;
            }

            Interlocked.CompareExchange(ref m_checkPipe, 0, 1);
        }


        private const string UriBase = "pack://application:,,,/Decchi;component/ParsingModule/Rules/Icons/{0}.png";
        protected IParseRule(IParseRuleOption option)
        {
            this.Client         = option.Client;
            this.ParseFlag      = option.ParseFlag;
            this.Regex          = option.Regex    == null ? null : new Regex(option.Regex,    RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            this.UrlRegex       = option.UrlRegex == null ? null : new Regex(option.UrlRegex, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            this.WndClass       = option.WndClass;
            this.WndClassTop    = option.WndClassTop;
            this.PluginUrl      = option.PluginUrl;
            this.PipeName       = option.PipeName;
            this.ClientIcon     = App.Current.Dispatcher.Invoke(new Func<BitmapImage>(() => new BitmapImage(new Uri(string.Format(UriBase, option.ClientIcon), UriKind.RelativeOrAbsolute))));
        }
        ~IParseRule()
        {
            this.Dispose(false);
        }

        private bool m_disposed = false;
        public void Dispose()
        {
            if (this.m_disposed) return;
            this.m_disposed = true;

            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public string       Client      { get; private set; }
        public ParseFlags   ParseFlag   { get; private set; }
        public Regex        Regex       { get; private set; }
        public Regex        UrlRegex    { get; private set; }
        public string       WndClass    { get; private set; }
        public bool         WndClassTop { get; private set; }
        public string       PluginUrl   { get; private set; }
        public string       PipeName    { get; private set; }
        public BitmapImage ClientIcon { get; private set; }

        public IntPtr GetWindowHandle()
        {
            return GetWindowHandle(IntPtr.Zero);
        }
        public IntPtr GetWindowHandle(IntPtr childAfter)
        {
            var hwnd = NativeMethods.FindWindowEx(IntPtr.Zero, childAfter, this.WndClass, null);
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;

            if (this.WndClassTop)
                return NativeMethods.GetParent(hwnd) == IntPtr.Zero ? hwnd : IntPtr.Zero;
            else
                return hwnd;
        }

        /// <summary>
        /// SongInfo 포메팅 전에 호출됩니다.
        /// </summary>
        public virtual void Edit(SongInfo si)
        { }

        /// <summary>
        /// 정규식을 사용하지 않고 별도로 파싱작업을 진행합니다
        /// </summary>
        /// <param name="title">창 타이틀</param>
        public virtual bool ParseTitle(SongInfo si, string title)
        {
            return false;
        }

        /// <summary>
        /// FindWindow 를 사용하지 않고 별도의 작업을 통해 재생정보를 가져옵니다
        /// 주로 SDK나 API를 이용할 때 사용합니다
        /// ParseFlag 에 ParseFlags.Get 플래그가 지정되야 합니다
        /// </summary>
        /// <param name="hwnd">찾은 윈도우 핸들 (선택)</param>
        public virtual bool ParseManual(SongInfo si, IntPtr hwnd)
        {
            return false;
        }

        /*
        public virtual IList<SongInfo> GetList(IntPtr hwnd)
        {
            return null;
        }
        */

        private Timer m_timer;
        /// <summary>
        /// 자동 인식 기능을 활성화합니다.
        /// </summary>
        public virtual void EnableAD()
        {
            if (this.m_timer != null) return;

            this.m_timer = new Timer(IParseRule.DefaultADCallback, this, 0, IParseRule.RefreshTimeSpan);
        }
        /// <summary>
        /// 자동 인식 기능을 끕니다.
        /// </summary>
        public virtual void DisableAD()
        {
            if (this.m_timer == null) return;
            
            this.m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.m_timer.Dispose();
            this.m_timer = null;
        }
        
        private static string m_befTitle;
        public static void DefaultADCallback(object args)
        {
            var rule = (IParseRule)args;

            var si = rule.GetFromPlayer(null);
            if (si == null) return;

            if (IParseRule.m_befTitle != null)
            {
                // 같은 곡이 아님
                if (IParseRule.m_befTitle != si.Title)
                    DecchiCore.Run(si);
            }

            IParseRule.m_befTitle = si.Title;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 플러그인이 설치되었는지 반환합니다.
        /// </summary>
        private bool m_isInstalled;
        public bool IsInstalled
        {
            get { return this.m_isInstalled; }
            set
            {
                if (value == true)
                {
                    this.m_isInstalled = value;

                    if (this.PropertyChanged != null)
                        App.Current.Dispatcher.Invoke(new Action<object, PropertyChangedEventArgs>(this.PropertyChanged.Invoke), new object[] { this, new PropertyChangedEventArgs("IsInstalled") });
                }
            }
        }

        //////////////////////////////////////////////////
        
        public void GetCurrentPlayingSong(IList<SongInfo> result, IList<WBResult> webPages)
        {
            if ((this.ParseFlag & ParseFlags.WebBrowser) != ParseFlags.WebBrowser)
                this.GetFromPlayer(result);
            else
                this.GetFromWebBrowser(result, webPages);
        }

        private static bool AddToList(IList<SongInfo> result, SongInfo si)
        {
            IParseRule.GetTagsFromFile(si);
            
            if (!string.IsNullOrWhiteSpace(si.Title))
            {
                if (result != null)
                    lock (result)
                        result.Add(si);
                return true;
            }
            return false;
        }

        private SongInfo GetFromPlayer(IList<SongInfo> result)
        {
            SongInfo si = new SongInfo(this);
            IntPtr hwnd = IntPtr.Zero;
            hwnd = this.GetWindowHandle(hwnd);
            if (hwnd == IntPtr.Zero) return null;
            
            // 파이프 : 성공시 return
            if ((this.ParseFlag & ParseFlags.Pipe) == ParseFlags.Pipe)
            {
                App.Debug("Pipe Parse");

                if (this.GetTagsFromPipe(si, hwnd))
                    if (IParseRule.AddToList(result, si))
                        return si;
                
                return null;
            }

            // 추가 파싱 : 성공하면 Return
            if ((this.ParseFlag & ParseFlags.ManualParse) == ParseFlags.ManualParse)
            {
                App.Debug("Manual Parse");
                if (this.ParseManual(si, hwnd))
                    if (IParseRule.AddToList(result, si))
                        return si;

                return null;
            }

            // 핸들 체크 (윈도우가 여러개 감지되면 각각 따로 작업합니다)
            if ((this.ParseFlag & ParseFlags.Title) == ParseFlags.Title)
            {
                do
                {
                
                    App.Debug("Handle : {0} : 0x{1:X8}", this.WndClass, hwnd);

                    // 타이틀 파싱 : 성공하면 Return
                    if (this.GetTagsFromTitle(si, hwnd))
                        IParseRule.AddToList(result, si);

                    si = new SongInfo(this);
                }
                while ((hwnd = this.GetWindowHandle(hwnd)) != IntPtr.Zero);
            }

            return result == null ? si : (result.Count > 0 ? result[0] : null);
        }
        
        private void GetFromWebBrowser(IList<SongInfo> result, IList<WBResult> webPages)
        {
            App.Debug("Web");

            string title;
            Match match;

            for (int i = 0; i < webPages.Count; ++i)
            {
                match = this.Regex.Match(webPages[i].Title);
                if (!match.Success) continue;

                App.Debug("Match : " + webPages[i].ToString());

                title = match.Groups["title"].Value.Trim();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var si = new SongInfo(this);
                    si.Title = title;

                    if (webPages[i].Url != null && this.UrlRegex.IsMatch(webPages[i].Url))
                        si.Url = webPages[i].Url;

                    si.Handle = webPages[i].Handle;
                    si.MainTab = webPages[i].MainTab;

                    IParseRule.AddToList(result, si);
                }
            }
        }
        
        private bool GetTagsFromPipe(SongInfo si, IntPtr hwnd, string data = null)
        {
            App.Debug("Pipe");
            if (data == null)
                data = this.GetDataFromPipe();

            if (data != null)
            {
                App.Debug(data);

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
                return true;
            }

            return false;
        }
        private string GetDataFromPipe()
        {
            using (var stream = new NamedPipeClientStream(".", this.PipeName, PipeDirection.In))
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

        private bool GetTagsFromTitle(SongInfo si, IntPtr hwnd, string title = null)
        {
            if (title == null)
                title = NativeMethods.GetWindowTitle(hwnd);
            if (string.IsNullOrWhiteSpace(title)) return false;
            App.Debug("Title");
            App.Debug(title);
                        
            if ((this.ParseFlag & ParseFlags.ManualTitle) == ParseFlags.ManualTitle)
            {
                if (this.ParseTitle(si, title))
                    return true;
                
                return false;
            }

            var match = this.Regex.Match(title);
            if (!match.Success) return false;

            Group g;
            if (string.IsNullOrWhiteSpace(si.Title)  && (g = match.Groups["title"])  != null) si.Title  = g.Value.Trim();
            if (string.IsNullOrWhiteSpace(si.Artist) && (g = match.Groups["artist"]) != null) si.Artist = g.Value.Trim();
            if (string.IsNullOrWhiteSpace(si.Album)  && (g = match.Groups["album"])  != null) si.Album  = g.Value.Trim();

            si.Handle = hwnd;
            return true;
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
                            (!string.IsNullOrWhiteSpace(si.Title) && filename.IndexOf(si.Title) >= 0) ||
                            (!string.IsNullOrWhiteSpace(si.Album) && filename.IndexOf(si.Album) >= 0))
                            si.Cover = new FileStream(files[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
            }
        }
    }
}
