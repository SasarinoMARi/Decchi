using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Decchi
{
    // https://msdn.microsoft.com/en-us/library/ms182161.aspx
    internal static class NativeMethods
    {
        /// <summary>
        /// 클래스 이름과 타이틀로 윈도우 핸들 값을 얻어옵니다
        /// </summary>
        /// <param name="strClassName">찾을 윈도우의 클래스 네임</param>
        /// <param name="strWindowName">찾을 윈도우의 타이틀</param>
        /// <returns>윈도우 핸들 값</returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [Out] StringBuilder lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowRect(IntPtr hwnd, out RECT rc);

        public const int WM_GETTEXTLENGTH	= 0x000E;
        public const int WM_GETTEXT			= 0x000D;


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //////////////////////////////////////////////////

        public static string GetWindowTitle(string strClassName, string strWindowName)
        {
            var hwnd = NativeMethods.FindWindow(strClassName, strWindowName);
            if (hwnd == IntPtr.Zero) return null;

            return GetWindowTitle(hwnd);
        }
        public static string GetWindowTitle(IntPtr handle)
        {
            var length = NativeMethods.SendMessage(handle, NativeMethods.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32() + 1;
            var lpString = new StringBuilder(length);

            NativeMethods.SendMessage(handle, NativeMethods.WM_GETTEXT, new IntPtr(length), lpString);

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
                    NativeMethods.GetWindowRect(hWnd, out appBounds);

                    screenBounds = Screen.FromHandle(hWnd).Bounds;
                    if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width)
                        return true;
                }
            }

            return false;
        }
    }
}
