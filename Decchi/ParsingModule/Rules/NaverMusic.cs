namespace Decchi.ParsingModule.Rules
{
    internal sealed class NaverMusic : IParseRule
    {
        public NaverMusic() : base(            
            new IParseRuleOption
            {
                Client      = "네이버 뮤직",
                ParseFlag   = ParseFlags.WebBrowser,
                Regex       = "^네이버 뮤직 :: (?<title>.+)",
                ClientIcon  = "navermusic"
            })
        { }
    }
}
