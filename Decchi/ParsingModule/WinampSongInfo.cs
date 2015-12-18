using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Decchi.ParsingModule
{
	class WinampSongInfo : SongInfo
	{
		public override string Client { get { return "Winamp"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/Winamp.png"; } }

		public override bool GetCurrentPlayingSong( )
		{
			var b = false;
			var procs = Process.GetProcessesByName("winamp");
			string str;

			for ( int i = 0; i < procs.Length; i++ )
			{
				using ( procs[i] )
				{
					if ( !b )
					{
						str = procs[i].MainWindowTitle;
						if ( procs[i].MainWindowTitle.Contains( "Winamp" ) )
						{
							var sep = str.IndexOf('-');
							var sep2 = str.LastIndexOf('-');

							this.Title = str.Substring( sep + 1, sep2 - sep - 1 ).Trim( );
							this.Album = null;
							var artistWithIndex = str.Substring( 0, sep ).Trim( );
							var rgx = new Regex("[0-9]+. ");
							this.Artist = rgx.Replace( artistWithIndex, string.Empty, 1 );

							this.Loaded = true;
							b = true;
						}
					}
				}
			}

			return b;
		}
	}
}