using System.Diagnostics;

namespace Decchi.ParsingModule
{
	public sealed class GomAudioSongInfo : SongInfo
	{
		public override string Client { get { return "곰오디오"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/GomAudio.png"; } }

		public override bool GetCurrentPlayingSong()
		{
			var b = false;
			var procs = Process.GetProcessesByName("goma");
			string str;

			for (int i = 0; i < procs.Length; i++)
			{
				using (procs[i])
				{
					if (!b)
					{
						str = procs[i].MainWindowTitle;
						if (procs[i].MainWindowTitle.Contains("곰오디오"))
						{
							var sep = str.IndexOf('-');
							var sep2 = str.LastIndexOf('-');
					
							this.Title	= str.Substring(sep + 1, sep2 - sep - 1).Trim();
							this.Album	= null;
							this.Artist	= str.Substring(0, sep).Trim();

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