using System.Diagnostics;

namespace Decchi.ParsingModule
{
	public sealed class iTunesSongInfo : SongInfo
	{
		public override string Client { get { return "iTunes"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/ITunes.png"; } }

		public override bool GetCurrentPlayingSong()
		{
			var procs = Process.GetProcessesByName("itunes");

			if (procs.Length == 0) return false;

			for (int i = 0; i < procs.Length; ++i)
				procs[i].Dispose();

			try
			{
				var itunes = new iTunesLib.iTunesAppClass();
				var track = itunes.CurrentTrack;

				this.Title	= track.Name;
				this.Album	= track.Album;
				this.Artist	= track.Artist;
				this.Loaded	= true;
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
