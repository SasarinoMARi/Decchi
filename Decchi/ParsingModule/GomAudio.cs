using System.Diagnostics;

namespace ParsingModule
{
	public class GomAudioSongInfo
	{
		public const string clientName = "곰오디오";

		public SongInfo GetCurrentPlayingSong( )
		{
			// Checking Thread Apartment State
			//ApartmentState state = Thread.CurrentThread.GetApartmentState();
			//if ( state == ApartmentState.STA )
			//	throw new InvalidOperationException
			//	( "You cannot be in Single Thread Apartment (STA) State." );

			// Finding the GomAudio window
			var info = SongInfo.Empty;

			var procs = Process.GetProcesses();
			for ( int i = 0; i < procs.Length; i++ )
			{
				var str = procs[i].MainWindowTitle;
				if ( procs[i].MainWindowTitle.Contains( "곰오디오" ))
				{
					var splt = str.Split('-');

					var SongName = splt[1].Trim();
					var SongArtist = splt[0].Trim();
					info = new SongInfo( clientName, SongName, string.Empty, SongArtist );
                }
			}

			return info;
		}
	}


}