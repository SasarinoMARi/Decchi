// https://gist.github.com/RyuaNerin/4a36b6d4c254983f679d

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Decchi.Core;

namespace Decchi.Utilities
{
    public sealed class InstanceHelper : IDisposable
    {
        private const uint CustomMsg = 0x7A8F;

        private readonly string m_uniqueName;
        public InstanceHelper(string uniqueName)
        {
            this.m_uniqueName = uniqueName;
        }
        ~InstanceHelper()
        {
            this.Dispose(false);
        }
        private bool m_disposed;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (this.m_disposed) return;
            this.m_disposed = true;

            this.DestroyWindow();

            if (this.m_mutex != null)
            {
                this.m_mutex.Close();
                this.m_mutex = null;
            }
        }

        private NativeMethods.WndProc m_proc;
        private IntPtr  m_customHwnd;
        private Mutex   m_mutex;

        public bool Check()
        {
            bool createdNew;
            this.m_mutex = new Mutex(true, this.m_uniqueName, out createdNew);

            if (createdNew && this.m_mutex.WaitOne(0))
            {
                this.CreateWindow();
                return true;
            }
            else
            {
                var hwnd = NativeMethods.FindWindow(this.m_uniqueName, null);
                if (hwnd != IntPtr.Zero)
                    NativeMethods.SendMessage(hwnd, InstanceHelper.CustomMsg, IntPtr.Zero, IntPtr.Zero);

                this.m_mutex.Close();
                return false;
            }
        }

        private void CreateWindow()
        {
            this.m_proc = new NativeMethods.WndProc(this.CustomProc);

            var wndClass			= new NativeMethods.WNDCLASS();
            wndClass.lpszClassName	= this.m_uniqueName;
            wndClass.lpfnWndProc	= Marshal.GetFunctionPointerForDelegate(this.m_proc);

            var resRegister	= NativeMethods.RegisterClass(ref wndClass);
            var resError	= Marshal.GetLastWin32Error();

            if (resRegister == 0 && resError != NativeMethods.ERROR_CLASS_ALREADY_EXISTS)
                throw new Exception();

            this.m_customHwnd = NativeMethods.CreateWindowEx(0, this.m_uniqueName, null, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
        private void DestroyWindow()
        {
            if (this.m_customHwnd != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(this.m_customHwnd);
                this.m_customHwnd = IntPtr.Zero;
            }
        }

        private IntPtr CustomProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == CustomMsg && App.Current.MainWindow != null)
            {
                try
                {
                    App.Current.Dispatcher.Invoke<bool>(App.Current.MainWindow.Activate);
                }
                catch
                {
                }
            }

            return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private static class NativeMethods
        {
            public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(
                string lpClassName,
                string lpWindowName);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr SendMessage(
                IntPtr hWnd,
                uint Msg,
                IntPtr wParam,
                IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern ushort RegisterClass(
                ref WNDCLASS pcWndClassEx);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr CreateWindowEx(
                int dwExStyle,
                string lpClassName,
                string lpWindowName,
                int dwStyle,
                int x,
                int y,
                int nWidth,
                int nHeight,
                IntPtr hWndParent,
                IntPtr hMenu,
                IntPtr hInstance,
                IntPtr lpParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr DefWindowProc(
                IntPtr hWnd,
                uint msg,
                IntPtr wParam,
                IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DestroyWindow(
                IntPtr hWnd);

            //////////////////////////////////////////////////

            [StructLayout(LayoutKind.Sequential)]
            public struct WNDCLASS
            {
                public int		style;
                public IntPtr	lpfnWndProc;
                public int		cbClsExtra;
                public int		cbWndExtra;
                public IntPtr	hInstance;
                public IntPtr	hIcon;
                public IntPtr	hCursor;
                public IntPtr	hbrBackground;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string	lpszMenuName;
                [MarshalAs(UnmanagedType.LPTStr)]
                public string	lpszClassName;
            }

            //////////////////////////////////////////////////

            public const int ERROR_CLASS_ALREADY_EXISTS = 1410;
        }
    }
}
