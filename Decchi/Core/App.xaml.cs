using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Decchi.ParsingModule;

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
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowCrashReport((Exception)e.ExceptionObject);
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) => ShowCrashReport(e.Exception);
#endif

            var asm  = Assembly.GetExecutingAssembly();

            Version = asm.GetName().Version;
            ExePath = Path.GetFullPath(asm.Location);
            ExeDir  = Path.GetDirectoryName(ExePath);

            LockPath = Path.Combine(App.ExeDir, "Decchi.lock");
            
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
                File.SetAttributes(App.LockPath, FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
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
        }

        private static readonly string[]        m_resourceName;
        private static readonly PropertyInfo[]  m_resourceMethod;
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            if (name.StartsWith("Decchi")) return null;

            for (int i = 0; i < m_resourceName.Length; ++i)
            {
                if (m_resourceName[i].Contains(name))
                    return Assembly.Load((byte[])m_resourceMethod[i].GetValue(null));
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

            foreach (var rule in SongInfo.Rules)
                rule.Dispose();

            if (this.m_lock != null)
                this.m_lock.Dispose();
        }
	}
}
