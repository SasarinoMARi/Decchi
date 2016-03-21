using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;
using Decchi.Core;

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
        ManualOne = 0x0010,
        /// <summary>별도의 작업을 거쳐 재생정보를 하나 이상 가져옵니다 (GetItems)</summary>
        ManualMulti = 0x0020,
        /// <summary>정규식을 사용하지 않고 별도로 파싱작업을 진행합니다 (ParseTitle)</summary>
        ManualParse = 0x0040,
        /// <summary>트윗하기 전 별도의 수정작업을 진행합니다</summary>
        Edit = 0x0080,

        /// <summary>Title + Local</summary>
        Default = 0x0003,
    }

    [DebuggerDisplay("{Client}")]
    public class IParseRule : IDisposable, INotifyPropertyChanged
    {
        protected class IParseRuleOption
        {
            /// <summary>외부에 노출되는 클라이언트 이름입니다.</summary>
            public string       Client      { get; set; }
            /// <summary>재생정보를 가져올 동작을 지정합니다</summary>
            public ParseFlags   ParseFlag   { get; set; }
            /// <summary>윈도우 타이틀을 인식할 정규식입니다. (웹 브라우저 포함)</summary>
            public string       Regex       { get; set; }
            /// <summary>
            /// ParseFlags.WebBrowser 전용
            /// 인식된 Url 과 정규식을 비교하여 일치하였을 때 재생 정보에 주소를 포함합니다
            /// </summary>
            public string       UrlRegex    { get; set; }
            /// <summary>
            /// 윈도우를 검색할 때 사용할 창의 ClassName 입니다
            /// 윈도우를 찾지 못할 경우에는 해당 플레이어의 검색을 중지합니다
            /// (ParseFlags.WebBrowser 가 설정되었을 경우 무시)
            /// </summary>
            public string       WndClass    { get; set; }
            /// <summary>검색된 윈도우가 최상위 핸들일 경우에만 다음으로 넘어갑니다</summary>
            public bool         WndClassTop { get; set; }
            /// <summary>뎃찌EXT 안내 홈페이지 주소입니다</summary>
            public string       PluginUrl   { get; set; }
            /// <summary>파이프 통신할 파이프 이름입니다.</summary>
            public string       PipeName    { get; set; }
            /// <summary>Decchi/ParsingModule/Rules/Icons/ 에 위치한 클라이언트 이름입니다. 이 png 파일은 반드시 컴파일 옵션을 Resource 으로 지정하영 압니다</summary>
            public string       ClientIcon  { get; set; }
        }

        protected const int RefreshTimeSpan = 30 * 1000;

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
        public BitmapImage  ClientIcon  { get; private set; }

        public virtual void Edit(SongInfo si)
        { }

        public virtual bool ParseTitle(ref SongInfo si, string title)
        {
            return false;
        }

        public virtual bool Get(SongInfo si, IntPtr hwnd)
        {
            return false;
        }
        public virtual IList<SongInfo> GetList(IntPtr hwnd)
        {
            return null;
        }

        private Timer m_timer;
        public virtual void EnableAD()
        {
            if (this.m_timer != null) return;

            this.m_timer = new Timer(SongInfo.DefaultADCallback, this, 0, IParseRule.RefreshTimeSpan);
        }
        public virtual void DisableAD()
        {
            if (this.m_timer == null) return;
            
            this.m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.m_timer.Dispose();
            this.m_timer = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
