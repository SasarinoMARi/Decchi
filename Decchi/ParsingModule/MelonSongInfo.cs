namespace Decchi.ParsingModule
{
	public sealed class MelonSongInfo : SongInfo
	{
		public override string Client { get { return "MelOn"; } }
		public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/Melon.png"; } }

		public override bool GetCurrentPlayingSong()
		{
			var str = NativeMethods.GetWindowTitle("MelOnFrameV40", null);
			if (str == null) return false;

			str = str.Replace(" - MelOn Player", "");
			
			try
			{
				var sep = str.IndexOf('-');

				this.Title	= str.Substring(0, sep).Trim();
				this.Album	= null;
				this.Artist	= str.Substring(sep + 1).Trim();
				this.Loaded	= true;

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
