using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Windows.Forms;

namespace ShortcutModule
{
	class HookManager
	{
		private static HookManager _instance = null;
		public static HookManager Instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = new HookManager( );
				}
				return _instance;
			}
		}
		private HookManager( )
		{
			hookManager = new globalKeyboardHook( );
		}

		private globalKeyboardHook hookManager;

		public void AddKeyDownListner( Keys HookedKey, KeyEventHandler callback )
		{
			hookManager.HookedKeys.Add( HookedKey );
			hookManager.KeyDown += callback;
		}
		public void RemoveKeyDownListner( KeyEventHandler callback )
		{
			hookManager.KeyDown -= callback;
		}

		public void AddKeyUpListner( Keys HookedKey, KeyEventHandler callback )
		{
			hookManager.HookedKeys.Remove( HookedKey );
			hookManager.KeyUp += callback;
		}
		public void RemoveKeyUpListner( KeyEventHandler callback )
		{
			hookManager.KeyUp -= callback;
		}
	}
}
