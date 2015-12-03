using System.Diagnostics;

namespace ParsingModule
{
	public class YoutubeSongInfo : SongInfo
	{
		protected override string Client { get { return "YouTube"; } }

		public override bool GetCurrentPlayingSong( )
		{
			var procs = Process.GetProcesses();
			for ( int i = 0; i < procs.Length; i++ )
			{
				var str = procs[i].MainWindowTitle;
				if ( procs[i].MainWindowTitle.Contains( "YouTube" ) )
				{
					var sep = str.LastIndexOf( " - YouTube" );

					this.Title = str.Substring( 0, sep ).Trim( );
					this.Album = null;
					this.Artist = null;

					this.Loaded = true;
					return true;
				}
			}

			return false;
		}
	}
}