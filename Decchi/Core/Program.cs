using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Decchi.Core.Windows;
using Decchi.ParsingModule;

namespace Decchi.Core
{
    static class Program
    {
        public const string lpClassName = "{E6EB2FB5-1079-4ECD-97C2-7CEB04765320}";

        [STAThread]
        static void Main()
        {
            var hwnd = NativeMethods.FindWindow(lpClassName, null);
            if (hwnd == IntPtr.Zero)
            {
                CreateCustonWindow();

                HttpWebRequest.DefaultCachePolicy   = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                HttpWebRequest.DefaultWebProxy      = null;

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                App.Main();
            }
            else
            {
                int pid;
                NativeMethods.GetWindowThreadProcessId(hwnd, out pid);

                using (var proc = Process.GetProcessById(pid))
                {
                    hwnd = proc.MainWindowHandle;
                    if (hwnd != IntPtr.Zero && !NativeMethods.IsIconic(hwnd))
                        NativeMethods.SetForegroundWindow(hwnd);
                }
            }
        }

        private static NativeMethods.WndProc m_wndProc;
        private static void CreateCustonWindow()
        {
            var wndClass            = new NativeMethods.WNDCLASS();
            wndClass.lpszClassName  = Program.lpClassName;
            wndClass.lpfnWndProc    = (m_wndProc = NativeMethods.DefWindowProc);

            if (NativeMethods.RegisterClass(ref wndClass) == 0 && 
                Marshal.GetLastWin32Error() != NativeMethods.ERROR_CLASS_ALREADY_EXISTS)
                return;

            NativeMethods.CreateWindowEx(0, Program.lpClassName, String.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        static Program()
        {
            m_assembly  = Assembly.GetExecutingAssembly();
            m_resources = m_assembly.GetManifestResourceNames();

            Version = m_assembly.GetName().Version;
            ExePath = Path.GetFullPath(m_assembly.Location);
            ExeDir  = Path.GetDirectoryName(ExePath);
        }

        public  static readonly string      ExePath;
        public  static readonly string      ExeDir;
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

            if (SongInfo.Assemblies != null)
                for (int i = 0; i < SongInfo.Assemblies.Length; ++i)
                    if (eInfo.FullName == SongInfo.Assemblies[i].FullName)
                        return SongInfo.Assemblies[i];

            return null;
        }
        
        public static bool CheckNewVersion()
        {
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
