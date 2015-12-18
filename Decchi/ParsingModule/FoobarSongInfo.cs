namespace Decchi.ParsingModule
{
    public sealed class FoobarSongInfo : SongInfo
    {
        public override string Client { get { return "Foobar2000"; } }
        public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/Foobar.png"; } }

        public override bool GetCurrentPlayingSong()
        {
            // 자리만 잡아둠
            return false;
        }
    }
}
