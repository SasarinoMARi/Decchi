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

		private const string defaultFormat = "/artist/의 /title/을 듣고 있어요! /via/ - /client/";
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
