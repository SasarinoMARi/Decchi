using System.Collections.Generic;
using System.Text;

namespace Decchi.ParsingModule
{
	public abstract class SongInfo
	{
		public const string Via = "#뎃찌NP";

		public abstract string Client		{ get; }
		public abstract string ClientIcon	{ get; }

		public string	Title		{ get; protected set; }
		public string	Album		{ get; protected set; }
		public string	Artist		{ get; protected set; }
		public bool		Loaded		{ get; protected set; }

		/// <summary>트윗할 섬네일은 항상 임시 파일로 지정해야합니다</summary>
		public string	Thumbnail	{ get; protected set; }

		public abstract bool GetCurrentPlayingSong( );

		public const string defaultFormat = "{/Artist/의 }{/Title/{ (/Album/)}을/를 }듣고 있어요! {/Via/} - {/Client/} #NowPlaying";
		public override string ToString( )
		{
			var format = Globals.GetValue("PublishFormat");
			if (string.IsNullOrEmpty(format)) format = Decchi.ParsingModule.SongInfo.defaultFormat;

			try
			{
				return ToFormat(format);
			}
			catch
			{
				return null;
			}
		}

		private string ToFormat(string format)
		{
			var total = new StringBuilder();

			StringBuilder sb = null;
			var queue = new Queue<StringBuilder>();
			char c;

			bool b;

			string str;

			int i = 0;
			while (i < format.Length)
			{
				// { 부터 } 까지다
				c = format[i];

				switch (c)
				{
					case '{':
						{
							if (sb != null) queue.Enqueue(sb);
							sb = new StringBuilder();
						}
						break;

					case '}':
						{
							str = sb.ToString();

							b = false;
							str = Replace(str, "/Title/",	this.Title,		ref b);
							str = Replace(str, "/Artist/",	this.Artist,	ref b);
							str = Replace(str, "/Album/",	this.Album,		ref b);
							str = Replace(str, "/Client/",	this.Client,	ref b);
							str = Replace(str, "/Via/",		SongInfo.Via,	ref b);

							if (b)
							{
								if (queue.Count > 0)
								{
									sb = queue.Dequeue();
									sb.Append(str);
								}
								else
								{
									total.Append(str);
									sb = null;
								}
							}
							else
							{
								if (queue.Count > 0)
									sb = queue.Dequeue();
								else
									sb = null;
							}
						}
						break;

					default:
						if (sb == null)
							total.Append(c);
						else
							sb.Append(c);
						break;
				}

				i++;
			}

			return total.ToString();
		}

		private string Replace(string str, string find, string replace, ref bool b)
		{
			if (str.IndexOf(find) >= 0 && !string.IsNullOrEmpty(replace))
			{
				b = true;
				return str.Replace(find, replace);
			}
			return str;
		}
	}
}