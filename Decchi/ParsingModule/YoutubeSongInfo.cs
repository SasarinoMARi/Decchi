using System.Diagnostics;

namespace Decchi.ParsingModule
{
	public class YoutubeSongInfo : SongInfo
	{
		protected override string Client { get { return "YouTube"; } }

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
						if (procs[i].MainWindowTitle.Contains("YouTube"))
						{
							var sep = str.LastIndexOf(" - YouTube");

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