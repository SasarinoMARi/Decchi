using System;
using System.Runtime.InteropServices;

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
		[DllImport("User32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(string strClassName, string strWindowName);
	}
}
