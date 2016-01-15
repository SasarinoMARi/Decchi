using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Decchi.Core;
using Decchi.ParsingModule;
using Microsoft.Win32;

namespace Decchi
{
    /// <summary>
    /// 이 클래스는 싱글턴으로 설계되었습니다
    /// </summary>
    public sealed class Globals : DependencyObject
    {
        /// <summary>
        /// 기본 브라우저에서 url을 엽니다.
        /// </summary>
        /// <param name="url">열고자 하는 주소</param>
        public static void OpenWebSite(string url)
        {
            try
            {
                using (Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url }))
                { }
            }
            catch
            { }
        }

        //////////////////////////////////////////////////

        /// <summary>
        /// Property 에 Attribute 를 설정 하면 저장과 불러오기에 사용됩니다
        /// </summary>
        private class PropAttr : Attribute
        { }

        private static readonly string SettingFilePath; // 설정 파일 주소
        static Globals()
        {
            SettingFilePath = Path.Combine(App.ExeDir, "Decchi.ini");
        }

        private static object m_sync = new object();
        private static Globals m_instance;
        public  static Globals   Instance
        {
            get
            {
                return m_instance ?? (m_instance = new Globals());
            }
        }

        private PropertyInfo[] m_props;

        private Globals()
        {
            if (!File.Exists(SettingFilePath) && File.Exists(Path.Combine(App.ExeDir, "publish.ini")))
                File.Move(Path.Combine(App.ExeDir, "publish.ini"), SettingFilePath);

            this.m_props = typeof(Globals)
                           .GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance)
                           .Where(e => e.CustomAttributes.Any(ee => ee.AttributeType == typeof(PropAttr)))
                           .OrderBy(e => e.Name)
                           .ToArray();

            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(SettingFilePath))
            {
                int i = 0;

                using (var reader = new StreamReader(SettingFilePath))
                {
                    string      line;
                    string[]    splitedLine;
                    object      obj;

                    while ((line = reader.ReadLine()) != null)
                    {
                        splitedLine = line.Split('=');

                        if (splitedLine.Length != 2) continue;

                        for (i = 0; i < this.m_props.Length; ++i)
                        {
                            if (this.m_props[i].Name == splitedLine[0])
                            {
                                obj = String2Object(splitedLine[1], this.m_props[i].PropertyType);
                                if (obj != null)
                                    this.m_props[i].SetValue(this, obj);

                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 프로퍼티 뭉텅이를 저장합니다. 설정이 바뀌거나 프로그램이 종료되기 직전에 호출해주세요.
        /// </summary>
        public void SaveSettings()
        {
            using (var writer = new StreamWriter(SettingFilePath))
                for (int i = 0; i < this.m_props.Length; ++i)
                    writer.WriteLine("{0}={1}", this.m_props[i].Name, Object2String(this.m_props[i].GetValue(this)));
        }

        private static string Object2String(object value)
        {
            if (value is string)        return (string)value;
            if (value is bool)          return (bool)value ? "1" : "0";
            if (value is ShortcutInfo)  return ((ShortcutInfo)value).ToString();
            if (value is DateTime)      return ((DateTime)value).ToString("yyyy-MM-dd hh:mm:ss");

            return null;
        }
        private static object String2Object(string value, Type type)
        {
            if (type == typeof(string))             return value;
            if (type == typeof(bool))               return value == "1" ? true : false;

            try
            {
                if (type == typeof(ShortcutInfo))   return ShortcutInfo.Parse(value);
                if (type == typeof(DateTime))       return DateTime.Parse(value);	
            }
            catch
            {}

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// WPF Binding 에서 프로퍼티 수정했을때 콜백되는 함수
        /// </summary>
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var globals = (Globals)d;

            if (e.Property == UseShortcutProp ||
                e.Property == ShortcutProp)
            {
                DecchiCore.HookSetting();
            }
            else if (e.Property == WinStartupProp)
            {
                if ((bool)e.NewValue)
                {
                    using (var reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                        reg.SetValue("Decchi", App.ExePath);
                }
                else
                {
                    using (var reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                        reg.DeleteValue("Decchi");
                }
            }
            else if (e.Property == DetectLocalFileProp)
                globals.m_detectLocalFile = (bool)e.NewValue;

            else if (e.Property == DetectWebUrlProp)
                globals.m_detectWebUrl = (bool)e.NewValue;

            else if (e.Property == SkipFullscreenProp)
                globals.m_skipFullscreen = (bool)e.NewValue;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [PropAttr]
        public string TwitterToken { get; set; }

        [PropAttr]
        public string TwitterSecret { get; set; }

        private string m_publishFormat = SongInfo.defaultFormat;
        [PropAttr]
        public  string PublishFormat
        {
            get { return string.IsNullOrWhiteSpace(this.m_publishFormat) ? SongInfo.defaultFormat : this.m_publishFormat; }
            set { this.m_publishFormat = value; }
        }

        private static readonly DependencyProperty UseShortcutProp = DependencyProperty.Register("UseShortcut", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(true, Globals.PropertyChangedCallback));
        [PropAttr]
        public  bool UseShortcut
        {
            get { return (bool)this.GetValue(UseShortcutProp); }
            set { this.SetValue(UseShortcutProp, value); }
        }
        
        private static readonly DependencyProperty ShortcutProp = DependencyProperty.Register("Shortcut", typeof(ShortcutInfo), typeof(Globals), new FrameworkPropertyMetadata(ShortcutInfo.Default, Globals.PropertyChangedCallback));
        [PropAttr]
        public  ShortcutInfo Shortcut
        {
            get { return (ShortcutInfo)this.GetValue(ShortcutProp); }
            set { this.SetValue(ShortcutProp, value); }
        }

        private static readonly DependencyProperty SkipFullscreenProp = DependencyProperty.Register("SkipFullscreen", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(true, Globals.PropertyChangedCallback));
        private bool m_skipFullscreen;
        [PropAttr]
        public bool SkipFullscreen
        {
            get { return m_skipFullscreen; }
            set { this.SetValue(SkipFullscreenProp, value); this.m_skipFullscreen = value; }
        }

        private static readonly DependencyProperty TrayStartProp = DependencyProperty.Register("TrayStart", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false));
        [PropAttr]
        public bool TrayStart
        {
            get { return (bool)this.GetValue(TrayStartProp); }
            set { this.SetValue(TrayStartProp, value); }
        }

        private static readonly DependencyProperty WinStartupProp = DependencyProperty.Register("WinStartup", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false, Globals.PropertyChangedCallback));
        [PropAttr]
        public bool WinStartup
        {
            get { return (bool)this.GetValue(WinStartupProp); }
            set { this.SetValue(WinStartupProp, value); }
        }

        private static readonly DependencyProperty DetectLocalFileProp = DependencyProperty.Register("DetectLocalFile", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false, Globals.PropertyChangedCallback));
        private bool m_detectLocalFile;
        [PropAttr]
        public bool DetectLocalFile
        {
            get { return this.m_detectLocalFile; }
            set { this.SetValue(DetectLocalFileProp, value); this.m_detectLocalFile = value; }
        }

        private static readonly DependencyProperty DetectWebUrlProp = DependencyProperty.Register("DetectWebUrl", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false, Globals.PropertyChangedCallback));
        private bool m_detectWebUrl;
        [PropAttr]
        public bool DetectWebUrl
        {
            get { return this.m_detectWebUrl; }
            set { this.SetValue(DetectWebUrlProp, value); this.m_detectWebUrl = value; }
        }

        private static readonly DependencyProperty TopMostProp = DependencyProperty.Register("TopMost", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false));
        [PropAttr]
        public bool TopMost
        {
            get { return (bool)this.GetValue(TopMostProp); }
            set { this.SetValue(TopMostProp, value); }
        }

        private static readonly DependencyProperty TrayWhenMinimizeProp = DependencyProperty.Register("TrayWhenMinimize", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false));
        [PropAttr]
        public bool TrayWhenMinimize
        {
            get { return (bool)this.GetValue(TrayWhenMinimizeProp); }
            set { this.SetValue(TrayWhenMinimizeProp, value); }
        }

        private static readonly DependencyProperty TrayWhenCloseProp = DependencyProperty.Register("TrayWhenClose", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(true));
        [PropAttr]
        public bool TrayWhenClose
        {
            get { return (bool)this.GetValue(TrayWhenCloseProp); }
            set { this.SetValue(TrayWhenCloseProp, value); }
        }

        private static readonly DependencyProperty TrayVisibleProp = DependencyProperty.Register("TrayVisible", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(true));
        [PropAttr]
        public bool TrayVisible
        {
            get { return (bool)this.GetValue(TrayVisibleProp); }
            set { this.SetValue(TrayVisibleProp, value); }
        }

        private static readonly DependencyProperty MiniModeProp = DependencyProperty.Register("MiniMode", typeof(bool), typeof(Globals), new FrameworkPropertyMetadata(false));
        [PropAttr]
        public bool MiniMode
        {
            get { return (bool)this.GetValue(MiniModeProp); }
            set { this.SetValue(MiniModeProp, value); }
        }

        public struct ShortcutInfo
        {
            public static readonly ShortcutInfo Default = new ShortcutInfo(ModifierKeys.Control, Key.Q);

            public static ShortcutInfo Parse(string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    ModifierKeys    modifier = ModifierKeys.None;
                    Key             key;

                    if (value.IndexOf("Ctrl+")  >= 0) modifier |= ModifierKeys.Control;
                    if (value.IndexOf("Alt+")   >= 0) modifier |= ModifierKeys.Alt;
                    if (value.IndexOf("Shift+") >= 0) modifier |= ModifierKeys.Shift;

                    if (value.Contains('+')) value = value.Substring(value.LastIndexOf('+') + 1);
                    
                    if (Enum.TryParse<Key>(value, out key) && Enum.IsDefined(typeof(Key), key))
                        return new ShortcutInfo(modifier, key);
                }

                return ShortcutInfo.Default;
            }
            public ShortcutInfo(ModifierKeys modifier, Key key)
            {
                this.Modifier   = modifier;
                this.Key        = key;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                if ((this.Modifier & ModifierKeys.Control)  == ModifierKeys.Control)    sb.Append("Ctrl+");
                if ((this.Modifier & ModifierKeys.Alt)      == ModifierKeys.Alt)        sb.Append("Alt+");
                if ((this.Modifier & ModifierKeys.Shift)    == ModifierKeys.Shift)      sb.Append("Shift+");
                sb.Append(this.Key.ToString());
                return sb.ToString();
            }

            public ModifierKeys Modifier;
            public Key          Key;
        }
    }
}
