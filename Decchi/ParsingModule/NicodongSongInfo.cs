using System.Diagnostics;

namespace Decchi.ParsingModule
{
	public sealed class NicodongSongInfo : SongInfo
	{
		public override string Client { get { return "ニコニコ動画"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/Nicodong.png"; } }

		public override bool GetCurrentPlayingSong( )
		{
			var b = false;
			var procs = Process.GetProcesses();
			string str;

			// Process is IDisposable
			for ( int i = 0; i < procs.Length; i++ )
			{
				using (procs[i])
				{
					if (!b)
					{
						str = procs[i].MainWindowTitle;
						if (str.Contains("- Niconico"))
						{
							var sep = str.LastIndexOf("- Niconico");
                            if (str.IndexOf("Niconico Live") >= 0)
                                sep = str.LastIndexOf('-', sep - 1);

							this.Title = str.Substring(0, sep).Trim();
							this.Album = null;
							this.Artist = null;

							this.Loaded = true;

							b = true;
						}
					}
				}
			}

			return b;
		}
	}
}