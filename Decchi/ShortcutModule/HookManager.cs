using System.Windows.Forms;
using Utilities;

namespace ShortcutModule
{
	class HookManager
	{
		private static HookManager _instance = null;
		public static HookManager Instance
		{
			get
			{
				return _instance ?? (_instance = new HookManager());
			}
		}
		private HookManager()
		{
			hookManager = new globalKeyboardHook();
		}

		private globalKeyboardHook hookManager;

		public void AddKeyDownListner(Keys HookedKey, KeyEventHandler callback)
		{
			hookManager.HookedKeys.Add(HookedKey);
			hookManager.KeyDown += callback;
		}
		public void RemoveKeyDownListner(KeyEventHandler callback)
		{
			hookManager.KeyDown -= callback;
		}

		public void AddKeyUpListner(Keys HookedKey, KeyEventHandler callback)
		{
			hookManager.HookedKeys.Remove(HookedKey);
			hookManager.KeyUp += callback;
		}
		public void RemoveKeyUpListner(KeyEventHandler callback)
		{
			hookManager.KeyUp -= callback;
		}
	}
}
