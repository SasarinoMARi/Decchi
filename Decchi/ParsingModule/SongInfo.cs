using System.Text.RegularExpressions;

namespace ParsingModule
{
	public abstract class SongInfo
	{
		public const string Via = "#뎃찌NP";

		protected abstract string Client { get; }

		public string Title { get; protected set; }
		public string Album { get; protected set; }
		public string Artist { get; protected set; }
		public bool Loaded { get; protected set; }

		public abstract bool GetCurrentPlayingSong( );

		public const string defaultFormat = "{/Artist/의 }{/Title/을 }듣고 있어요! {/Via/} - {/Client/}";
		public override string ToString( )
		{
			return ToString( defaultFormat );
		}
		public string ToString( string format )
		{
			string str = format;

			str = Replace( str, "Title", this.Title );
			str = Replace( str, "Artist", this.Artist );
			str = Replace( str, "Album", this.Album );
			str = Replace( str, "Client", this.Client );
			str = Replace( str, "Via", SongInfo.Via );

			return str;
		}

		private string Replace( string SourceString, string TargetString, string Value )
		{
			string replaceString = string.Empty;
			var regex = new Regex( @"{[^{]*/" + TargetString + "/[^}]*}" );
			var match = regex.Match(SourceString);
			if ( match.Success )
			{
				if ( !string.IsNullOrEmpty(Value) )
				{
					replaceString = match.Value.Replace( "{", "" ).Replace( "}", "" ).Replace( "/" + TargetString + "/", Value );
				}
				else
				{
					replaceString = string.Empty;
				}
				SourceString = regex.Replace( SourceString, replaceString );
			}
			return SourceString;
		}
	}
}