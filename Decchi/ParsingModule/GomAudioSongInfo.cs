using System.Diagnostics;

namespace ParsingModule
{
	public class GomAudioSongInfo : SongInfo
	{
		protected override string Client { get { return "곰오디오"; } }

		public override bool GetCurrentPlayingSong()
		{
			// Checking Thread Apartment State
			//ApartmentState state = Thread.CurrentThread.GetApartmentState();
			//if state == ApartmentState.STA)
			//	throw new InvalidOperationException
			//	"You cannot be in Single Thread Apartment (STA) State.");

			// Finding the GomAudio window
			var procs = Process.GetProcesses();
			for (int i = 0; i < procs.Length; i++)
			{
				var str = procs[i].MainWindowTitle;
				if (procs[i].MainWindowTitle.Contains("곰오디오"))
				{
					var sep = str.IndexOf('-');
					
					this.Title	= str.Substring(sep + 1).Trim();
					this.Album	= null;
					this.Artist	= str.Substring(0, sep).Trim();

					this.Loaded = true;
					return true;
				}
			}

			return false;
		}
	}


}