using System.Windows.Forms;
using Utilities;

namespace ShortcutModule
{
	internal static class HookManager
	{
		private static globalKeyboardHook hookManager = new globalKeyboardHook();

		public static void AddKeyDownListner(Keys HookedKey, KeyEventHandler callback)
		{
			hookManager.HookedKeys.Add(HookedKey);
			hookManager.KeyDown += callback;
		}
		public static void RemoveKeyDownListner(KeyEventHandler callback)
		{
			hookManager.KeyDown -= callback;
		}

		public static void AddKeyUpListner(Keys HookedKey, KeyEventHandler callback)
		{
			hookManager.HookedKeys.Remove(HookedKey);
			hookManager.KeyUp += callback;
		}
		public static void RemoveKeyUpListner(KeyEventHandler callback)
		{
			hookManager.KeyUp -= callback;
		}
	}
}
