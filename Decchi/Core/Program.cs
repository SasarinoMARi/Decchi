using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

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
            m_assembly = Assembly.GetExecutingAssembly();
            m_resources = m_assembly.GetManifestResourceNames();
        }
        
        static readonly Assembly    m_assembly;
        static readonly string[]    m_resources;
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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

            return null;
        }
        
    }
}
