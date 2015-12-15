namespace ParsingModule
{
	public abstract class SongInfo
	{
		public const string Via = "#뎃찌NP";

		protected abstract string Client { get; }

		public string	Title	{ get; protected set; }
		public string	Album	{ get; protected set; }
		public string	Artist	{ get; protected set; }
		public bool		Loaded	{ get; protected set; }

		public abstract bool GetCurrentPlayingSong();

		public const string defaultFormat = "/artist/ - /title/ via (/client/)";
		public override string ToString()
		{
			return ToString(defaultFormat);
		}
		public string ToString(string format)
		{
			string str = format
			.Replace("/title/", this.Title)
			.Replace("/artist/", this.Artist)
			.Replace("/album/", this.Album)
			.Replace("/client/", this.Client)
			.Replace("/via/", SongInfo.Via);

			return str;
		}
	}
}
