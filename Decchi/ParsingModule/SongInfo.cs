using System.Text.RegularExpressions;

namespace Decchi.ParsingModule{
	public abstract class SongInfo
	{
		public const string Via = "#뎃찌NP";

		public abstract string Client { get; }
		public abstract string ClientIcon { get; }

		public string Title { get; protected set; }
		public string Album { get; protected set; }
		public string Artist { get; protected set; }
		public bool Loaded { get; protected set; }

		public abstract bool GetCurrentPlayingSong( );

		public const string defaultFormat = "{/Artist/의 }{/Title/을(를) }듣고 있어요! {/Via/} - {/Client/}";
		public override string ToString( )
		{
			return ToString( defaultFormat );
		}
		public string ToString( string format )
		{
			string str = format;

			str = Replace( str, "/Title/", this.Title );
			str = Replace( str, "/Artist/", this.Artist );
			str = Replace( str, "/Album/", this.Album );
			str = Replace( str, "/Client/", this.Client );
			str = Replace( str, "/Via/", SongInfo.Via );

			return str;
		}

		private string Replace( string SourceString, string TargetString, string Value )
		{
			// 구조좀 바꿈
			var match = Regex.Match(SourceString , @"{([^{/]*" + TargetString + "[^}]*)}", RegexOptions.IgnoreCase);
			if ( match.Success )
			{
				var replaceString = match.Groups[1].Value;

				if (!string.IsNullOrEmpty(Value))
				{
					replaceString = match.Groups[1].Value.Replace(TargetString, Value);
				}
				else
				{
					replaceString = string.Empty;
				}

				SourceString = SourceString.Replace(match.Value, replaceString);
			}
			return SourceString;
		}
	}
}