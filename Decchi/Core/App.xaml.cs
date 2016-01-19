using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Decchi.ParsingModule;
using Microsoft.Win32;

namespace Decchi.Core
{
	internal partial class App : Application
    {
        public  static readonly string      ExePath;
        public  static readonly string      ExeDir;
        public  static readonly Version     Version;
        private static readonly string      LockPath;

        private static bool m_debug;
        private static StreamWriter m_debugWriter;

        public static bool ShowPatchNote { get; private set; }
        public static bool DebugMode { get { return App.m_debug; } }

        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowCrashReport((Exception)e.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (s, e) => ShowCrashReport(e.Exception);

            m_assembly  = Assembly.GetExecutingAssembly();
            m_resources = m_assembly.GetManifestResourceNames();

            Version = m_assembly.GetName().Version;
            ExePath = Path.GetFullPath(m_assembly.Location);
            ExeDir  = Path.GetDirectoryName(ExePath);

            LockPath = Path.Combine(App.ExeDir, "Decchi.lock");

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            HttpWebRequest.DefaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
#if !DEBUG
            HttpWebRequest.DefaultWebProxy      = null;
#endif
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowCrashReport(e.Exception);
        }

        private Stream m_lock;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            App.ShowPatchNote = e.Args.Length > 0 && e.Args.Contains("--updated");

            App.m_debug = e.Args.Length > 0 && e.Args.Contains("--debug");
            if (App.m_debug)
                App.m_debugWriter = new StreamWriter(Path.Combine(App.ExeDir, "decchi.debug"), true, Encoding.UTF8) { AutoFlush = true };

            try
            {
            	m_lock = new FileStream(App.LockPath, FileMode.CreateNew, FileSystemRights.Write, FileShare.None, 8, FileOptions.DeleteOnClose);
                File.SetAttributes(App.LockPath, FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
            catch
            { }

            if (this.m_lock == null)
            {
                var curProc = Process.GetCurrentProcess();
#if DEBUG
                var procs = Process.GetProcessesByName(curProc.ProcessName.Replace(".vshost", ""));
#else
                var procs = Process.GetProcessesByName(curProc.ProcessName);
#endif

                if (procs.Length > 0)
                {
                    for (int i = 0; i < procs.Length; ++i)
                    {
#if DEBUG
                        if (procs[i].MainModule.FileName != curProc.MainModule.FileName.Replace(".vshost", "")) continue;
#else
                        if (procs[i].MainModule.FileName != curProc.MainModule.FileName) continue;
#endif
                        if (procs[i].Id == curProc.Id) continue;

                        var hwnd = procs[i].MainWindowHandle;
                        if (hwnd != IntPtr.Zero)
                        {
                            NativeMethods.SendMessage(hwnd, 0x056F, new IntPtr(0xAB55), new IntPtr(0xAB55)); // WM_User Range (0x0400 ~ 0x07FF)

                            break;
                        }
                    }
                }
               
                App.Current.Shutdown();
                return;
            }

            Globals.Instance.LoadSettings();
        }

        private static readonly Assembly    m_assembly;
        private static readonly string[]    m_resources;
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var eInfo = new AssemblyName(args.Name);

            for (int i = 0; i < m_resources.Length; ++i)
            {
                if (m_resources[i].Contains(eInfo.Name))
                {
                    using (var stream = m_assembly.GetManifestResourceStream(m_resources[i]))
                    {
                        byte[] buff = new byte[stream.Length];
                        stream.Read(buff, 0, buff.Length);

                        return Assembly.Load(buff);
                    }
                }
            }

            if (SongInfo.Assemblies != null)
                for (int i = 0; i < SongInfo.Assemblies.Length; ++i)
                    if (eInfo.FullName == SongInfo.Assemblies[i].FullName)
                        return SongInfo.Assemblies[i];

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
        
        public static void Debug(Exception exception)
        {
            if (App.m_debug)
                lock (App.m_debugWriter)
                    App.Debug(exception.ToString());
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
        public static void Debug(string[] str)
        {
            if (App.m_debug)
            {
                lock (App.m_debugWriter)
                {
                    App.m_debugWriter.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss "));
                    for (int i = 0; i < str.Length; ++i)
                        App.m_debugWriter.WriteLine(str[i]);
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

            if (this.m_lock != null)
                this.m_lock.Dispose();
        }
	}
}
