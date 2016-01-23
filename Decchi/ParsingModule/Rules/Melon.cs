namespace Decchi.ParsingModule.Rules
{
    internal sealed class Melon : IParseRule
    {
        public Melon() : base(            
            new IParseRuleOption
            {
                Client      = "멜론",
                ParseFlag   = ParseFlags.Default,
                Regex       = "(?<title>.+) - (?<artist>.+) - MelOn Player",
                WndClass    = "MelOnFrameV40",
                WndClassTop = true,
                ClientIcon  = "melon"
            })
        { }
    }
}
