using System;
using System.Text;

namespace Decchi.ParsingModule
{
	public class AlsongSongInfo : SongInfo
	{
		protected override string Client { get { return "알송"; } }

		public override bool GetCurrentPlayingSong( )
		{
			var hwnd = NativeMethods.FindWindow("ALSong_Class", null);

			if (hwnd == IntPtr.Zero)
				return false;

			// WM_GETTEXTLENGTH = 0x000E;
			var length = NativeMethods.SendMessage(hwnd, 0x000E, IntPtr.Zero, IntPtr.Zero).ToInt32();
			var lpString = new StringBuilder(length + 1);

			// WM_GETTEXT = 0x000D;
			NativeMethods.SendMessage(hwnd, 0x000D, new IntPtr(length), lpString);

			var str = lpString.ToString();
			var sep = str.IndexOf('-');

			this.Title	= str.Substring(sep + 1).Trim();
			this.Album	= null;
			this.Artist	= str.Substring(0, sep).Trim();
			this.Loaded = true;

			return true;
		}
	}
}