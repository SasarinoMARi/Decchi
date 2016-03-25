using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Decchi.ParsingModule;
using Decchi.Utilities;
using Microsoft.Win32;

namespace Decchi.Core
{
	internal partial class App : Application
    {
        private static readonly string      Guid;
        public  static readonly string      ExePath;
        public  static readonly string      ExeDir;
        public  static readonly Version     Version;

        private static bool m_debug;
        private static StreamWriter m_debugWriter;

        public static bool ShowPatchNote { get; private set; }
        public static bool DebugMode { get { return App.m_debug; } }

        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowCrashReport((Exception)e.ExceptionObject);
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) => ShowCrashReport(e.Exception);
#endif

            var asm  = Assembly.GetExecutingAssembly();

            Guid    = new Guid((asm.GetCustomAttributes(typeof(GuidAttribute), true)[0] as GuidAttribute).Value).ToString("D").ToLower();
            Version = asm.GetName().Version;
            ExePath = Path.GetFullPath(asm.Location);
            ExeDir  = Path.GetDirectoryName(ExePath);
            
            var byteArray = typeof(byte[]);
            App.m_resourceMethod = Type.GetType("Decchi.Properties.Resources")
                                       .GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty)
                                       .Where(e => e.PropertyType == byteArray)
                                       .ToArray();
            App.m_resourceName = App.m_resourceMethod.Select(e => e.Name.Replace('_', '.'))
                                                     .ToArray();

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            HttpWebRequest.DefaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
#if !DEBUG
            HttpWebRequest.DefaultWebProxy      = null;
#endif
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if !DEBUG
            ShowCrashReport(e.Exception);
#endif
        }

        private InstanceHelper m_instance;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            m_instance = new InstanceHelper(Guid);
            if (!m_instance.Check())
            {
                this.Shutdown();
                return;
            }

            App.ShowPatchNote = e.Args.Length > 0 && e.Args.Contains("--updated");

            App.m_debug = e.Args.Length > 0 && e.Args.Contains("--debug");
            if (App.m_debug)
                App.m_debugWriter = new StreamWriter(Path.Combine(App.ExeDir, "decchi.debug"), true, Encoding.UTF8) { AutoFlush = true };
        }

        private static readonly string[]        m_resourceName;
        private static readonly PropertyInfo[]  m_resourceMethod;
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name.Replace('-', '.');
            if (name.StartsWith("Decchi")) return null;

            for (int i = 0; i < m_resourceName.Length; ++i)
            {
                if (m_resourceName[i].Contains(name))
                {
                    byte[] buff;

                    using (var comp     = new MemoryStream((byte[])m_resourceMethod[i].GetValue(null)))
                    using (var gzip     = new GZipStream(comp, CompressionMode.Decompress))
                    using (var uncomp   = new MemoryStream(4096))
                    {
                        gzip.CopyTo(uncomp);

                        buff = uncomp.ToArray();
                    }

                    return Assembly.Load(buff);
                }
            }

            return null;
        }

        public static bool CheckNewVersion(out string url)
        {
            url = null;

            try
            {
                string body;

                var req = HttpWebRequest.Create("https://api.github.com/repos/Usagination/Decchi/releases/latest") as HttpWebRequest;
                req.Timeout = 10000;
                req.UserAgent = "Decchi";
                using (var res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                    body = reader.ReadToEnd();
                
                var tagName = new Version(Regex.Match(body, "\"tag_name\"[ \t]*:[ \t]*\"([^\"]+)\"").Groups[1].Value);

                url = Regex.Match(body, "\"browser_download_url\"[ \t]*:[ \t]*\"([^\"]+)\"").Groups[1].Value;

                return tagName > Version;
            }
            catch
            {
                return false;
            }
        }
        
        public static void Debug(string format, params object[] args)
        {
            if (App.m_debug)
                lock (App.m_debugWriter)
                    App.Debug(string.Format(format, args));
        }
        public static void Debug(string str)
        {
            if (App.m_debug)
            {
                lock (App.m_debugWriter)
                {
                    App.m_debugWriter.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss "));
                    App.m_debugWriter.WriteLine(str);
                }
            }
        }
        public static void Debug(IEnumerable<string> strs)
        {
            if (App.m_debug)
            {
                lock (App.m_debugWriter)
                {
                    foreach (var str in strs)
                    {
                        App.m_debugWriter.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss "));
                        App.m_debugWriter.WriteLine(str);
                    }
                }
            }
        }
        public static void Debug(Exception exception)
        {
            if (App.m_debug)
            {
                lock (App.m_debugWriter)
                {
                    App.m_debugWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    App.m_debugWriter.WriteLine(exception.ToString());
                }
            }
        }

        private static void ShowCrashReport(Exception exception)
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var file = string.Format("Crash-{0}.txt", date);

            using (var writer = new StreamWriter(Path.Combine(App.ExeDir, file)))
            {
                writer.WriteLine("Decchi Crash Report");
                writer.WriteLine("Date    : " + date);
                writer.WriteLine("Version : " + App.Version);
                writer.WriteLine();
                writer.WriteLine("OS Ver  : " + GetOSInfomation());
                writer.WriteLine("SPack   : " + NativeMethods.GetOSServicePack());
                writer.WriteLine();
                writer.WriteLine("Exception");
                writer.WriteLine(exception.ToString());
            }

            if (Decchi.Core.Windows.MainWindow.Instance != null)
                Decchi.Core.Windows.MainWindow.Instance.Dispatcher.Invoke(() => Decchi.Core.Windows.MainWindow.Instance.CrashReport(file));
            else
                App.Current.Shutdown();
        }

        private static string GetOSInfomation()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion"))
                    return key.GetValue("ProductName").ToString();
            }
            catch
            {
                return "Operating System Information unavailable";
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Globals.Instance.SaveSettings();

            this.m_instance.Dispose();

            foreach (var rule in IParseRule.Rules)
                rule.Dispose();
        }
	}
}
