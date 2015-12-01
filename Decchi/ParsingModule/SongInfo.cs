namespace ParsingModule
{
	public class SongInfo
	{
		public const string Via = "#뎃찌NP";

        private bool empty;
		public string Client { get; private set; }
		public string Title { get; private set; }
		public string Album { get; private set; }
		public string Artist { get; private set; }

		private SongInfo( )
		{
            empty = true;
		}

		/// <summary>
		///  빈 SongInfo객체. Empty 대신 사용하세요.
		/// </summary>
		/// <param name="player"></param>
		public SongInfo(string player)
		{
			empty = true;
			Client = player;
		}

		public SongInfo( string player, string name, string album, string artist )
		{
            empty = false;
			Client = player;
			Title = name;
			Album = album;
			Artist = artist;
		}

		/// <summary>
		/// SongInfo.Empty 대신 new SongInfo(string 클라이언트_이름)을 사용해주세요.
		/// </summary>
		public static SongInfo Empty { get { return new SongInfo( ); } }

        public static bool IsEmpty(SongInfo info)
        {
            return info.empty;
        }

		private const string defaultFormat = "/artist/의 /title/을 듣고 있어요! /via/";
		public override string ToString( )
		{
			return ToString( defaultFormat );
		}
		public string ToString( string Format )
		{
			string str = Format
			.Replace("/title/", this.Title)
			.Replace("/artist/", this.Artist)
			.Replace("/album/", this.Album)
			.Replace("/client/", this.Client)
			.Replace("/via/", SongInfo.Via);

			return str;
		}
	}
}
