// http://www.codeproject.com/Tips/714342/Getting-Currently-Playing-Song-in-Windows-Media-Pl

using System;
using System.Collections.Generic;
using System.Windows.Automation;
using Decchi;

namespace ParsingModule
{
	public class WMPSongInfo
	{
		public const string clientName = "Windows Media Player";

		public SongInfo GetCurrentPlayingSong( )
		{
			// Finding the WMP window
			IntPtr handle = Globals.FindWindow("WMPlayerApp", "Windows Media Player");
			if ( handle == IntPtr.Zero ) return SongInfo.Empty;

			TreeWalker walker = TreeWalker.ControlViewWalker;
			// Whole WMP window
			AutomationElement wmpPlayer = AutomationElement.FromHandle(handle);
			AutomationElement wmpAppHost = walker.GetFirstChild(wmpPlayer);
			AutomationElement wmpSkinHost = walker.GetFirstChild(wmpAppHost);
			// All elements in WMP window
			AutomationElement wmpContainer = walker.GetFirstChild(wmpSkinHost);
			// Container with song information
			AutomationElement wmpSongInfo = walker.GetFirstChild(wmpContainer);

			if ( wmpSongInfo == null )
			{
                // 전체화면이 아님
			}

			// Iterating through all components in container - searching for container with song information
			while ( wmpSongInfo.Current.ClassName != "CWmpControlCntr" )
			{
				wmpSongInfo = walker.GetNextSibling( wmpSongInfo );

				if ( wmpSongInfo == null )
					break;
			}

			// Walking through children (image, hyperlink, song info etc.)
			List<AutomationElement> info = GetChildren(wmpSongInfo);
			info = GetChildren( info[0] );
			info = GetChildren( info[1] );
			info = GetChildren( info[2] );

			// Obtaining elements with desired information
			AutomationElement songE = info[0];
			AutomationElement albumE = info[3];
			AutomationElement artistE = info[4];

			string name = string.Empty;
			string album = string.Empty;
			string artist = string.Empty;

			try
			{
				name = songE.Current.Name;
				album = albumE.Current.Name;
				artist = artistE.Current.Name;
			}
			catch
			{

			}

			return new SongInfo( clientName, name, album, artist );
		}

		// Returns all child AutomationElement nodes in "element" node
		private List<AutomationElement> GetChildren( AutomationElement element )
		{
			List<AutomationElement> result = new List<AutomationElement>();
			TreeWalker walker = TreeWalker.ControlViewWalker;
			AutomationElement child = walker.GetFirstChild(element);
			result.Add( child );

			while ( child != null )
			{
				child = walker.GetNextSibling( child );
				result.Add( child );
			}

			return result;
		}
	}


}