using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Decchi
{
    // https://msdn.microsoft.com/en-us/library/ms182161.aspx
    internal static class NativeMethods
    {
        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 클래스 이름과 타이틀로 윈도우 핸들 값을 얻어옵니다
        /// </summary>
        /// <param name="strClassName">찾을 윈도우의 클래스 네임</param>
        /// <param name="strWindowName">찾을 윈도우의 타이틀</param>
        /// <returns>윈도우 핸들 값</returns>
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [Out] StringBuilder lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

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
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClass([In] ref WNDCLASS pcWndClassEx);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern int QueryDosDevice(string lpDeviceName, [Out] StringBuilder lpTargetPath, int ucchMax);

        [DllImport("ntdll.dll")]
        public static extern int NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryObject(IntPtr ObjectHandle, ObjectInformationClass ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);

        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WM_GETTEXT = 0x000D;
        public const int STATUS_INFO_LENGTH_MISMATCH = -1073741820; //0xC0000004;
        public const int DUPLICATE_SAME_ACCESS = 0x2;
        public const int MAX_PATH = 260;
        public const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All                 = 0x001F0FFF,
            Terminate           = 0x00000001,
            CreateThread        = 0x00000002,
            VMOperation         = 0x00000008,
            VMRead              = 0x00000010,
            VMWrite             = 0x00000020,
            DupHandle           = 0x00000040,
            SetInformation      = 0x00000200,
            QueryInformation    = 0x00000400,
            Synchronize         = 0x00100000
        }

        public enum ObjectInformationClass : int
        {
            ObjectBasicInformation      = 0,
            ObjectNameInformation       = 1,
            ObjectTypeInformation       = 2,
            ObjectAllTypesInformation   = 3,
            ObjectHandleInformation     = 4
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASS
        {
            public int		style;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WndProc	lpfnWndProc;
            public int		cbClsExtra;
            public int		cbWndExtra;
            public IntPtr	hInstance;
            public IntPtr	hIcon;
            public IntPtr	hCursor;
            public IntPtr	hbrBackground;
            public string	lpszMenuName;
            public string	lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_HANDLE_INFORMATION
        {
            public int      ProcessID;
            public byte     ObjectTypeNumber;
            public byte     Flags;
            public ushort   Handle;
            //public IntPtr   ObjectPointer;
            public int      ObjectPointer;
            public uint     GrantedAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_BASIC_INFORMATION
        {
            public int Attributes;
            public int GrantedAccess;
            public int HandleCount;
            public int PointerCount;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public int NameInformationLength;
            public int TypeInformationLength;
            public int SecurityDescriptorLength;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_TYPE_INFORMATION
        {
            public UNICODE_STRING Name;
            public int ObjectCount;
            public int HandleCount;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public int Reserved4;
            public int PeakObjectCount;
            public int PeakHandleCount;
            public int Reserved5;
            public int Reserved6;
            public int Reserved7;
            public int Reserved8;
            public int InvalidAttributes;
            public GENERIC_MAPPING GenericMapping;
            public int ValidAccess;
            public byte Unknown;
            public byte MaintainHandleDatabase;
            public int PoolType;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_NAME_INFORMATION
        {
            public UNICODE_STRING Name;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GENERIC_MAPPING
        {
            public int GenericRead;
            public int GenericWrite;
            public int GenericExecute;
            public int GenericAll;
        }

        //////////////////////////////////////////////////

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
    }
}
