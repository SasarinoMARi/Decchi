using System;
using System.Runtime.InteropServices;
using System.Text;

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

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [Out] StringBuilder lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		public const int WM_GETTEXTLENGTH	= 0x000E;
		public const int WM_GETTEXT			= 0x000D;

		//////////////////////////////////////////////////

		public static string GetWindowTitle(string strClassName, string strWindowName)
		{
			var hwnd = NativeMethods.FindWindow(strClassName, strWindowName);
			if (hwnd == IntPtr.Zero) return null;

			var length = NativeMethods.SendMessage(hwnd, NativeMethods.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32() + 1;
			var lpString = new StringBuilder(length);

			NativeMethods.SendMessage(hwnd, NativeMethods.WM_GETTEXT, new IntPtr(length), lpString);

			return lpString.ToString();
		}
	}
}
