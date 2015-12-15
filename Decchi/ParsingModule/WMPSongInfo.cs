// http://www.codeproject.com/Tips/714342/Getting-Currently-Playing-Song-in-Windows-Media-Pl

using System;
using System.Collections.Generic;
using System.Windows.Automation;

namespace Decchi.ParsingModule
{
	public sealed class WMPSongInfo : SongInfo
	{
		public override string Client { get { return "Windows Media Player"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/WMP.png"; } }

		public override bool GetCurrentPlayingSong()
		{
			// Finding the WMP window
			IntPtr handle = NativeMethods.FindWindow("WMPlayerApp", "Windows Media Player");
			if (handle == IntPtr.Zero) return false;
			
			var walker			= TreeWalker.ControlViewWalker;
			// Whole WMP window
			var wmpPlayer		= AutomationElement.FromHandle(handle);
			var wmpAppHost		= walker.GetFirstChild(wmpPlayer);
			var wmpSkinHost		= walker.GetFirstChild(wmpAppHost);
			// All elements in WMP window
			var wmpContainer	= walker.GetFirstChild(wmpSkinHost);
			// Container with song information
			var wmpSongInfo		= walker.GetFirstChild(wmpContainer);

			if (wmpSongInfo == null)
			{
				// 전체화면이 아님
			}

			// Iterating through all components in container - searching for container with song information
			while (wmpSongInfo.Current.ClassName != "CWmpControlCntr")
			{
				wmpSongInfo = walker.GetNextSibling(wmpSongInfo);

				if (wmpSongInfo == null)
					break;
			}

			// Walking through children (image, hyperlink, song info etc.)
			List<AutomationElement> info = GetChildren(wmpSongInfo);
			info = GetChildren(info[0]);
			info = GetChildren(info[1]);
			info = GetChildren(info[2]);

			// Obtaining elements with desired information
// 			AutomationElement songE = info[0];
// 			AutomationElement albumE = info[3];
// 			AutomationElement artistE = info[4];
// 
// 			string name = string.Empty;
// 			string album = string.Empty;
// 			string artist = string.Empty;

			try
			{
				this.Title	= info[0].Current.Name;
				this.Album	= info[3].Current.Name;
				this.Artist	= info[4].Current.Name;

				this.Loaded = true;
				return true;
			}
			catch
			{ }

			return false;
		}

		// Returns all child AutomationElement nodes in "element" node
		private List<AutomationElement> GetChildren(AutomationElement element)
		{
			List<AutomationElement> result = new List<AutomationElement>();
			TreeWalker walker = TreeWalker.ControlViewWalker;
			AutomationElement child = walker.GetFirstChild(element);
			result.Add(child);

			while (child != null)
			{
				child = walker.GetNextSibling(child);
				result.Add(child);
			}

			return result;
		}
	}


}