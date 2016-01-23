using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Decchi.Utilities
{
	/// <summary>
	/// A class that manages a global low level keyboard hook
	/// </summary>
	public class GlobalKeyboardHook : IDisposable
	{
        #region Instance Variables
        private List<Key> m_hookedKeys = new List<Key>();
		/// <summary>
		/// The collections of keys to watch for
		/// </summary>
        public IList<Key> HookedKeys { get { return this.m_hookedKeys; } }
		/// <summary>
		/// Handle to the hook, need this to unhook and call the next hook
		/// </summary>
		IntPtr hhook = IntPtr.Zero;
		#endregion
		
        public class KeyHookEventArgs : EventArgs
        {
            public KeyHookEventArgs(Key key)
            {
                this.Key = key;
            }
            public bool Handled { get; set; }
            public Key  Key     { get; private set; } 
        }

		public delegate void KeyHookEvent(object sender, KeyHookEventArgs e);

		#region Events
		/// <summary>
		/// Occurs when one of the hooked keys is pressed
		/// </summary>
		public event KeyHookEvent KeyDown;
		/// <summary>
		/// Occurs when one of the hooked keys is released
		/// </summary>
		public event KeyHookEvent KeyUp;
		#endregion

		#region Constructors and Destructors

		NativeMethods.keyboardHookProc khp;

		/// <summary>
		/// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
		/// </summary>
		public GlobalKeyboardHook()
		{
			khp = new NativeMethods.keyboardHookProc(hookProc);
			hook();
		}

		private bool m_disposed = false;
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (this.m_disposed) return;
			this.m_disposed = true;

			if (disposing)
			{
				unhook();
			}
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
		/// </summary>
		~GlobalKeyboardHook()
		{
			this.Dispose(false);
		}
		#endregion

		#region Public Methods
        private static IntPtr m_hLibrary = IntPtr.Zero;
		/// <summary>
		/// Installs the global hook
		/// </summary>
		public void hook()
		{
            if (hhook == IntPtr.Zero)
            {
                if (m_hLibrary == IntPtr.Zero)
                    m_hLibrary = NativeMethods.LoadLibrary("User32");

                hhook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, khp, m_hLibrary, 0);
            }
		}

		/// <summary>
		/// Uninstalls the global hook
		/// </summary>
		public void unhook()
		{
            if (hhook != IntPtr.Zero)
            {
			    NativeMethods.UnhookWindowsHookEx(hhook);
                hhook = IntPtr.Zero;
            }
		}

		/// <summary>
		/// The callback for the keyboard hook
		/// </summary>
		/// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
		/// <param name="wParam">The event type</param>
		/// <param name="lParam">The keyhook event information</param>
		/// <returns></returns>
		private IntPtr hookProc(int code, IntPtr wParam, ref NativeMethods.keyboardHookStruct lParam)
		{
			if (code >= 0)
			{
				Key key = KeyInterop.KeyFromVirtualKey(lParam.vkCode);
				if (HookedKeys.Contains(key))
				{
					var wparam = wParam.ToInt64();

                    var args = new KeyHookEventArgs(key);

					if ((wparam == NativeMethods.WM_KEYDOWN || wparam == NativeMethods.WM_SYSKEYDOWN) && (KeyDown != null))
					{
						KeyDown.Invoke(this, args);
					}
					else if ((wparam == NativeMethods.WM_KEYUP || wparam == NativeMethods.WM_SYSKEYUP) && (KeyUp != null))
					{
						KeyUp.Invoke(this, args);
					}

					if (args.Handled)
						return new IntPtr(1);
				}
			}
			return NativeMethods.CallNextHookEx(hhook, code, wParam, ref lParam);
		}
		#endregion

		private static class NativeMethods
		{
            #region Constant, Structure and Delegate Definitions
            /// <summary>
            /// defines the callback type for the hook
            /// </summary>
            public delegate IntPtr keyboardHookProc(int code, IntPtr wParam, ref keyboardHookStruct lParam);

            [StructLayout(LayoutKind.Sequential)]
            public struct keyboardHookStruct
            {
                public int vkCode;
                public int scanCode;
                public int flags;
                public int time;
                public int dwExtraInfo;
            }

            public const int WH_KEYBOARD_LL = 13;
            public const int WM_KEYDOWN = 0x100;
            public const int WM_KEYUP = 0x101;
            public const int WM_SYSKEYDOWN = 0x104;
            public const int WM_SYSKEYUP = 0x105;
            #endregion

			#region DLL imports
			/// <summary>
			/// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
			/// </summary>
			/// <param name="idHook">The id of the event you want to hook</param>
			/// <param name="callback">The callback.</param>
			/// <param name="hInstance">The handle you want to attach the event to, can be null</param>
			/// <param name="threadId">The thread you want to attach the event to, can be null</param>
			/// <returns>a handle to the desired hook</returns>
			[DllImport("user32.dll")]
			public static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

			/// <summary>
			/// Unhooks the windows hook.
			/// </summary>
			/// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
			/// <returns>True if successful, false otherwise</returns>
			[DllImport("user32.dll")]
			public static extern bool UnhookWindowsHookEx(IntPtr hInstance);

			/// <summary>
			/// Calls the next hook.
			/// </summary>
			/// <param name="idHook">The hook id</param>
			/// <param name="nCode">The hook code</param>
			/// <param name="wParam">The wparam.</param>
			/// <param name="lParam">The lparam.</param>
			/// <returns></returns>
			[DllImport("user32.dll")]
			public static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, ref keyboardHookStruct lParam);

			/// <summary>
			/// Loads the library.
			/// </summary>
			/// <param name="lpFileName">Name of the library</param>
			/// <returns>A handle to the library</returns>
			[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
			public static extern IntPtr LoadLibrary(string lpFileName);
			#endregion
		}
	}
}
