using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Decchi.Core;
using Decchi.ParsingModule;
using System.Linq;
using System.Text;

namespace Decchi
{
	/// <summary>
	/// 이 클래스는 싱글턴으로 설계되었습니다
	/// </summary>
    public class Globals : DependencyObject
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

        private class PropAttr : Attribute
        { }

        private const string SettingFilePath = "publish.ini"; // 설정 파일 주소

        private static Globals m_instance;
        public  static Globals   Instance { get { return m_instance ?? (m_instance = new Globals()); } }

        private const BindingFlags PropFlags = BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;
        private Globals()
        {
            if (File.Exists(SettingFilePath))
            {
                var props = typeof(Globals).GetProperties(PropFlags);
                int i = 0;

                using (var reader = new StreamReader(SettingFilePath))
                {
                    string line;
                    string[] splitedLine;

                    while ((line = reader.ReadLine()) != null)
                    {
                        splitedLine = line.Split('=');

                        for (i = 0; i < props.Length; ++i)
                        {
                            if (props[i].Name == splitedLine[0])
                            {
                                props[i].SetValue(this, String2Object(splitedLine[1], props[i].PropertyType));
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
                var props = typeof(Globals).GetProperties(PropFlags);

                for (int i = 0; i < props.Length; ++i)
                    if (props[i].CustomAttributes.Any(e => e.AttributeType == typeof(PropAttr)))
					    writer.WriteLine("{0}={1}", props[i].Name, Object2String(props[i].GetValue(this)));
			}
		}

        private static string Object2String(object value)
        {
            if (value is string)        return (string)value;
            if (value is bool)          return (bool)value ? "1" : "0";
            if (value is ShortcutInfo)  return ((ShortcutInfo)value).ToString();

            return null;
        }
        private static object String2Object(string value, Type type)
        {
            if (type == typeof(string))         return value;
            if (type == typeof(bool))           return value == "1" ? true : false;
            if (type == typeof(ShortcutInfo))   return ShortcutInfo.Parse(value);

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == UseShortcutProp ||
                e.Property == ShortcutProp)
            {
                DecchiCore.HookSetting();
            }
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