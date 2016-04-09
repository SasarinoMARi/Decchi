using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Decchi
{
    // https://msdn.microsoft.com/en-us/library/ms182161.aspx
    internal static class NativeMethods
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);  

        /// <summary>
        /// 클래스 이름과 타이틀로 윈도우 핸들 값을 얻어옵니다
        /// </summary>
        /// <param name="strClassName">찾을 윈도우의 클래스 네임</param>
        /// <param name="strWindowName">찾을 윈도우의 타이틀</param>
        /// <returns>윈도우 핸들 값</returns>
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [Out] StringBuilder lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rc);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCommands uCmd);
        
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("Kernel32.dll")]
        public static extern void RtlZeroMemory(IntPtr dest, IntPtr size);
        
        [DllImport("kernel32.Dll")]
        internal static extern short GetVersionEx(ref OsVersioninfo o);
        
        public enum GetWindowCommands : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OsVersioninfo
        {
            public int      dwOSVersionInfoSize;
            public int      dwMajorVersion;
            public int      dwMinorVersion;
            public int      dwBuildNumber;
            public int      dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string   szCSDVersion;
            public ushort   wServicePackMajor;
            public ushort   wServicePackMinor;
            public ushort   wSuiteMask;
            public byte     wProductType;
            public byte     wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //////////////////////////////////////////////////
        
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int WM_GETTEXT = 0x000D;

        public static string GetWindowTitle(IntPtr handle)
        {
            var length = NativeMethods.SendMessage(handle, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32() + 1;
            var lpString = new StringBuilder(length);

            NativeMethods.SendMessage(handle, WM_GETTEXT, new IntPtr(length), lpString);

            return lpString.ToString();
        }

        public static bool DetectFullscreenMode()
        {
            NativeMethods.RECT          appBounds;
            System.Drawing.Rectangle    screenBounds;

            IntPtr hWnd;
            IntPtr desktopHandle    = NativeMethods.GetDesktopWindow();
            IntPtr shellHandle      = NativeMethods.GetShellWindow();

            hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd != IntPtr.Zero)
            {
                if (hWnd != desktopHandle && hWnd != shellHandle)
                {
                    if (NativeMethods.GetWindowRect(hWnd, out appBounds))
                    {
                        screenBounds = Screen.FromHandle(hWnd).Bounds;
                        if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width)
                            return true;
                    }
                }
            }

            return false;
        }
        
        public static string GetOSServicePack()
        {
            try
            {
                var os = new OsVersioninfo();
                os.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OsVersioninfo));
                GetVersionEx(ref os);

                if (!string.IsNullOrWhiteSpace(os.szCSDVersion))
                    return os.szCSDVersion.Trim();
            }
            catch
            { }
            
            return "?";
        }

        public static IntPtr GetTopMostWindow(IntPtr[] hwnds)
        {
            var hCur = IntPtr.Zero;
            var zCur = 0;

            var hMax = IntPtr.Zero;
            var zMax = int.MaxValue;

            for (int i = 0; i < hwnds.Length; ++i)
            {
                hCur = hwnds[i];

                zCur = 0;
                while (hCur != IntPtr.Zero)
                {
                    zCur++;
                    hCur = GetWindow(hCur, GetWindowCommands.GW_HWNDPREV);
                };

                if (zCur < zMax)
                {
                    hMax = hwnds[i];
                    zMax = zCur;
                }
            }

            return hMax;
        }
    }
}
