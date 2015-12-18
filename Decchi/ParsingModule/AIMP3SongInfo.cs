namespace Decchi.ParsingModule
{
    public sealed class AIMP3SongInfo : SongInfo
    {
        public override string Client { get { return "AIMP3"; } }
        public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/AIMP.png"; } }

        public override bool GetCurrentPlayingSong()
        {
            var str = NativeMethods.GetWindowTitle("TAIMPMainForm", null);
            if (str == null) return false;
            
            try
            {
                var sep = str.IndexOf('-');

                this.Artist	= str.Substring(0, sep).Trim();
                this.Album	= null;
                this.Title	= str.Substring(sep + 1).Trim();
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
