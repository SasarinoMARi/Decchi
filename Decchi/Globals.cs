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
            SettingFilePath = Path.Combine(Program.ExeDir, "publish.ini");
        }

        private static Globals m_instance;
        public  static Globals   Instance { get { return m_instance ?? (m_instance = new Globals()); } }

        private static PropertyInfo[] GetProperties()
        {
            return
                typeof(Globals)
                .GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Where(e => e.CustomAttributes.Any(ee => ee.AttributeType == typeof(PropAttr)))
                .ToArray();
        }

        private Globals()
        {
            if (File.Exists(SettingFilePath))
            {
                int i = 0;

                using (var reader = new StreamReader(SettingFilePath))
                {
                    var props = GetProperties();

                    string      line;
                    string[]    splitedLine;
                    object      obj;

                    while ((line = reader.ReadLine()) != null)
                    {
                        splitedLine = line.Split('=');

                        if (splitedLine.Length != 2) continue;

                        for (i = 0; i < props.Length; ++i)
                        {
                            if (props[i].Name == splitedLine[0])
                            {
                                obj = String2Object(splitedLine[1], props[i].PropertyType);
                                if (obj != null)
                                    props[i].SetValue(this, obj);
                                    
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
            {
                var props = GetProperties();

                for (int i = 0; i < props.Length; ++i)
                    writer.WriteLine("{0}={1}", props[i].Name, Object2String(props[i].GetValue(this)));
            }
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
                        reg.SetValue("Decchi", Program.ExePath);
                }
                else
                {
                    using (var reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                        reg.DeleteValue("Decchi");
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [PropAttr]
        public DateTime LastUpdateCheck { get; set; }

        [PropAttr]
        public string TwitterToken { get; set; }
        
        [PropAttr]
        public string TwitterSecret { get; set; }

        private string m_publishFormat = SongInfo.defaultFormat;
        [PropAttr]
        public  string PublishFormat
        {
            get { return string.IsNullOrEmpty(this.m_publishFormat) ? (this.m_publishFormat = SongInfo.defaultFormat) : this.m_publishFormat; }
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
        [PropAttr]
        public bool SkipFullscreen
        {
            get { return (bool)this.GetValue(SkipFullscreenProp); }
            set { this.SetValue(SkipFullscreenProp, value); }
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
