using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Decchi.Core.Windows;
using Decchi.ParsingModule;

namespace Decchi.Core
{
    static class Program
    {
        public const string MutextName = "E6EB2FB5-1079-4ECD-97C2-7CEB04765320-";

        [STAThread]
        static void Main()
        {
            int i = 0;
            var procs = Process.GetProcesses();
            int runningId = 0;
            
            for (i = 0; i < procs.Length; ++i)
            {
                using (procs[i])
                {
                    if (runningId == 0)
                    {
                        try
                        {
                            Mutex newinstanceMutex = Mutex.OpenExisting(MutextName + procs[i].Id);

                            try
                            {
                                newinstanceMutex.ReleaseMutex();
                            }
                            catch { }

                            runningId = procs[i].Id;
                        }
                        catch { }
                    }
                }
            }

            if (runningId == 0)
            {
                var mutex = new Mutex(true, MutextName + Process.GetCurrentProcess().Id);
                mutex.ReleaseMutex();

                HttpWebRequest.DefaultCachePolicy   = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                HttpWebRequest.DefaultWebProxy      = null;

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                App.Main();
            }
            else
            {
                using (var proc = Process.GetProcessById(runningId))
                {
                    var hwnd = proc.MainWindowHandle;

                    if (hwnd != IntPtr.Zero && !NativeMethods.IsIconic(hwnd))
                        NativeMethods.SetForegroundWindow(hwnd);
                }
            }
        }

        static Program()
        {
            m_assembly  = Assembly.GetExecutingAssembly();
            m_resources = m_assembly.GetManifestResourceNames();

            Version = m_assembly.GetName().Version;
            ExePath = Path.GetFullPath(m_assembly.Location);
        }
        
        public  static readonly string      ExePath;
        public  static readonly Version     Version;
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

            for (int i = 0; i < SongInfo.Assemblies.Count; ++i)
            {
                if (eInfo.FullName ==  SongInfo.Assemblies[i].FullName)
                    return SongInfo.Assemblies[i];
            }

            return null;
        }
        
        public static bool CheckNewVersion()
        {
            if (DateTime.Now < Globals.Instance.LastUpdateCheck.AddDays(1))
                return false;

            try
            {
                var tag = "0.0.0.0";

                var req = HttpWebRequest.Create("https://api.github.com/repos/Usagination/Decchi/releases/latest") as HttpWebRequest;
                req.Timeout = 10000;
                req.UserAgent = "Decchi";
                using (var res = req.GetResponse())
                using (var reader = new StreamReader(res.GetResponseStream()))
                    tag = Regex.Match(reader.ReadToEnd(), "\"tag_name\"[ \t]*:[ \t]*\"([^\"]+)\"").Groups[1].Value;

                var tagName = new Version(tag);

                Globals.Instance.LastUpdateCheck = DateTime.Now;

                MainWindow.Instance.Dispatcher.Invoke(Globals.Instance.SaveSettings);

                return tagName > Version;
            }
            catch
            {
                return false;
            }
        }
    }
}
