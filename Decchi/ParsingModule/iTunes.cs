namespace ParsingModule
{
	public class iTunesSongInfo : SongInfo
	{
		protected override string Client { get { return "iTunes"; } }

		public override bool GetCurrentPlayingSong()
		{
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
