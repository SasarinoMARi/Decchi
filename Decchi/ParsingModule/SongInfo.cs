using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Decchi.ParsingModule
{
    [DebuggerDisplay("{Client}")]
    public sealed class SongInfo
    {
        /// <summary>
        /// Dll 에 아래 함수를 public 으로 넣어두면 자동으로 인식합니다
        /// </summary>
        private delegate bool DllParse(out string title, out string artist, out string album, out string thumbnail);

        public static SongInfo[] SongInfos { get; private set; }

        private static List<Assembly> m_assemblies = new List<Assembly>();
        public  static List<Assembly>   Assemblies { get { return m_assemblies; } }

        public const string BaseURL = "https://raw.githubusercontent.com/Usagination/Decchi/songinfo/";
        /// <summary>
        /// Github 에서 Songinfo 데이터를 가져옵니다
        /// 
        /// [ClientName]
        /// clienticon=아이콘주소
        /// wndclass=Regex 이용할때 FindWindow 할 주소
        /// regex=regex
        /// dllext=어셈블리에 로드해야하는 추가 라이브러리 주소
        /// dll=DllParse 함수가 있는 dll 파일 주소
        /// </summary>
        public static bool InitSonginfo()
        {
            try
            {
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
                                        cur.ClientIcon = BaseURL + val.ToString();
                                        break;

                                    case "wndclass":
                                        cur.m_wndClass = val.ToString();
                                        break;

                                    case "regex":
                                        cur.m_regex = new Regex(val.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
                                        break;

                                    case "dll":
                                        {
                                            //cur.m_assmbly = AppDomain.CurrentDomain.Load(wc.DownloadData(BaseURL + val));
                                            cur.m_assmbly = Assembly.Load(wc.DownloadData(val));
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

                                                cur.m_parse = (DllParse)Delegate.CreateDelegate(typeof(DllParse), null, dllParse);
                                            }
                                        }
                                        break;

                                    case "dllext":
                                        Assemblies.Add(Assembly.Load(wc.DownloadData(val)));
                                        break;
                                }
                            }
                        }
                    }
                }

                SongInfo.SongInfos = lst.ToArray();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsMethodCompatibleWithDelegate(MethodInfo method, Type delegateType)
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
        public  string      ClientIcon  { get; private set; }
        private string      m_wndClass;
        private Regex       m_regex;
        private Assembly    m_assmbly;
        private DllParse    m_parse;
        
        public bool         Loaded      { get; private set; }
        
        private string      m_title;
        private string      m_album;
        private string      m_artist;
        private string      m_thumbnail;
        public  string        Thumbnail { get { return this.m_thumbnail; } }

        public const string Via = "#뎃찌NP";
        public const string defaultFormat = "{/Artist/의 }{/Title/{ (/Album/)}을/를 }듣고 있어요! {/Via/} - {/Client/} #NowPlaying";

        public bool GetCurrentPlayingSong( )
        {
            this.Loaded = false;

            if (m_parse != null)
            {
                bool succ = this.m_parse.Invoke(out this.m_title, out this.m_artist, out this.m_album, out this.m_thumbnail);

                if (!succ)
                    return false;

                this.Loaded = true;
                return true;
            }
            else if (!string.IsNullOrEmpty(this.m_wndClass))
            {
                var str = NativeMethods.GetWindowTitle(this.m_wndClass, null);
                if (string.IsNullOrEmpty(str)) return false;

                var match = this.m_regex.Match(str);

                if (!match.Success)
                    return false;

                Group g;

                g = match.Groups["aritis"];
                this.m_artist   = g != null ? g.Value : null;

                g = match.Groups["title"];
                this.m_title    = g != null ? g.Value : null;

                g = match.Groups["album"];
                this.m_album    = g != null ? g.Value : null;

                this.Loaded = true;
                return true;
            }
            else
            {
                var procs = Process.GetProcesses();

                bool find = false;

                Match m;
                Group g;

                for (int i = 0; i < procs.Length; i++)
                {
                    using (procs[i])
                    {
                        if (!find)
                        {
                            m = this.m_regex.Match(procs[i].MainWindowTitle);

                            if (!m.Success)
                                return false;

                            g = m.Groups["aritis"];
                            this.m_artist	= g != null ? g.Value : null;

                            g = m.Groups["title"];
                            this.m_title    = g != null ? g.Value : null;

                            g = m.Groups["album"];
                            this.m_album    = g != null ? g.Value : null;

                            this.Loaded = true;
                            find = true;
                        }
                    }
                }

                return find;
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

        public override string ToString( )
        {
            try
            {
                return ToFormat(Globals.Instance.PublishFormat, this);
            }
            catch
            {
                return null;
            }
        }

        private static string ToFormat(string format, SongInfo info, bool checkFormat = false)
        {
            StringBuilder			total	= !checkFormat ? new StringBuilder() : null;
            
            StringBuilder			sb	= null;
            Queue<StringBuilder>	queue = new Queue<StringBuilder>();
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
                                str = Replace(str, "/Title/",   info.m_title,   ref b);
                                str = Replace(str, "/Artist/",  info.m_artist,  ref b);
                                str = Replace(str, "/Album/",   info.m_album,   ref b);
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
