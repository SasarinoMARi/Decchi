using System.Diagnostics;

namespace ParsingModule
{
	public class GomAudioSongInfo : SongInfo
	{
		protected override string Client { get { return "곰오디오"; } }

		public override bool GetCurrentPlayingSong()
		{
			var procs = Process.GetProcesses();
			for (int i = 0; i < procs.Length; i++)
			{
				var str = procs[i].MainWindowTitle;
				if (procs[i].MainWindowTitle.Contains("곰오디오"))
				{
					var sep = str.IndexOf('-');
					var sep2 = str.LastIndexOf('-');
					
					this.Title	= str.Substring(sep + 1, sep2 - sep - 1).Trim();
					this.Album	= null;
					this.Artist	= str.Substring(0, sep).Trim();

					this.Loaded = true;
					return true;
				}
			}

			return false;
		}
	}
}