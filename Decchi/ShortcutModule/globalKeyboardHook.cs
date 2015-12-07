using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Utilities
{
	/// <summary>
	/// A class that manages a global low level keyboard hook
	/// </summary>
	public class globalKeyboardHook : IDisposable
	{
		#region Constant, Structure and Delegate Definitions
		/// <summary>
		/// defines the callback type for the hook
		/// </summary>
		public delegate IntPtr keyboardHookProc(int code, IntPtr wParam, ref keyboardHookStruct lParam);

		public struct keyboardHookStruct
		{
			public int vkCode;
			public int scanCode;
			public int flags;
			public int time;
			public int dwExtraInfo;
		}

		const int WH_KEYBOARD_LL = 13;
		const int WM_KEYDOWN = 0x100;
		const int WM_KEYUP = 0x101;
		const int WM_SYSKEYDOWN = 0x104;
		const int WM_SYSKEYUP = 0x105;
		#endregion

		#region Instance Variables
		/// <summary>
		/// The collections of keys to watch for
		/// </summary>
		public List<Key> HookedKeys = new List<Key>();
		/// <summary>
		/// Handle to the hook, need this to unhook and call the next hook
		/// </summary>
		IntPtr hhook = IntPtr.Zero;
		#endregion
		
		public delegate void KeyEvent(ref bool handled, Key key);

		#region Events
		/// <summary>
		/// Occurs when one of the hooked keys is pressed
		/// </summary>
		public event KeyEvent KeyDown;
		/// <summary>
		/// Occurs when one of the hooked keys is released
		/// </summary>
		public event KeyEvent KeyUp;
		#endregion

		#region Constructors and Destructors

		keyboardHookProc khp;

		/// <summary>
		/// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
		/// </summary>
		public globalKeyboardHook()
		{
			khp = new keyboardHookProc(hookProc);
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
		~globalKeyboardHook()
		{
			this.Dispose(false);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Installs the global hook
		/// </summary>
		public void hook()
		{
			IntPtr hInstance = NativeMethods.LoadLibrary("User32");
			hhook = NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, khp, hInstance, 0);
		}

		/// <summary>
		/// Uninstalls the global hook
		/// </summary>
		public void unhook()
		{
			NativeMethods.UnhookWindowsHookEx(hhook);
		}

		/// <summary>
		/// The callback for the keyboard hook
		/// </summary>
		/// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
		/// <param name="wParam">The event type</param>
		/// <param name="lParam">The keyhook event information</param>
		/// <returns></returns>
		public IntPtr hookProc(int code, IntPtr wParam, ref keyboardHookStruct lParam)
		{
			if (code >= 0)
			{
				Key key = KeyInterop.KeyFromVirtualKey(lParam.vkCode);
				if (HookedKeys.Contains(key))
				{
					var wparam = wParam.ToInt64();
					var handeled = false;

					if ((wparam == WM_KEYDOWN || wparam == WM_SYSKEYDOWN) && (KeyDown != null))
					{
						KeyDown(ref handeled, key);
					}
					else if ((wparam == WM_KEYUP || wparam == WM_SYSKEYUP) && (KeyUp != null))
					{
						KeyUp(ref handeled, key);
					}

					if (handeled)
						return new IntPtr(1);
				}
			}
			return NativeMethods.CallNextHookEx(hhook, code, wParam, ref lParam);
		}
		#endregion

		private static class NativeMethods
		{
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