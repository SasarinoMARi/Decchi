using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Decchi.Utilities
{
    internal static class OpenedWindow
    {
        public struct WindowsInfo
        {
            public IntPtr Handle;
            public string Title;

            public override string ToString()
            {
                return string.Format("0x{0:X8} : {1}", this.Handle, this.Title);
            }
        }

        private delegate bool del(IntPtr handle, int lParam);

        public static IList<WindowsInfo> GetOpenedWindows()
        {
            var lst = new List<WindowsInfo>();
            var shellHwnd = NativeMethods.GetShellWindow();

            NativeMethods.EnumWindows(
                new NativeMethods.EnumWindowsProc(
                    (hwnd, lParam) => {
                        if (shellHwnd != hwnd && NativeMethods.IsWindowVisible(hwnd))
                        {
                            var title = NativeMethods.GetWindowTitle(hwnd);
                            if (!string.IsNullOrWhiteSpace(title))
                                lst.Add(new WindowsInfo { Handle = hwnd, Title = title });
                        }

                        return true;
                    }
                ), 0);

            return lst;
        }
    }
}
