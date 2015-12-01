using System.Diagnostics;

namespace ParsingModule
{
	public class GomAudioSongInfo
	{
		public const string clientName = "곰오디오";

		private GomAudioSongInfo()
		{

		}

		public static SongInfo GetCurrentPlayingSong( )
		{
			// Checking Thread Apartment State
			//ApartmentState state = Thread.CurrentThread.GetApartmentState();
			//if ( state == ApartmentState.STA )
			//	throw new InvalidOperationException
			//	( "You cannot be in Single Thread Apartment (STA) State." );

			// Finding the GomAudio window
			var info = new SongInfo( clientName );

			var procs = Process.GetProcesses();
			for ( int i = 0; i < procs.Length; i++ )
			{
				var str = procs[i].MainWindowTitle;
				if ( procs[i].MainWindowTitle.Contains( "곰오디오" ))
				{
					var splt = str.Split('-');

					// 곡 태그에 - 끼어있으면 에로사항이 꽃핌.
					// 고급스런 방법을 쓰던가 뭔가 수를 마련해야 함.
					var SongName = splt[1].Trim();
					var SongArtist = splt[0].Trim();
					info = new SongInfo( clientName, SongName, string.Empty, SongArtist );
                }
			}

			return info;
		}
	}
}