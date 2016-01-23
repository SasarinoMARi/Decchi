namespace Decchi.ParsingModule.Rules
{
    internal sealed class Winamp : IParseRule
    {
        public Winamp() : base(            
            new IParseRuleOption
            {
                Client      = "Winamp",
                ParseFlag   = ParseFlags.Default,
                Regex       = @"^([0-9]+\. )?((?<artist>.+) - )?(?<title>.+) - Winamp$",
                WndClass    = "Winamp v1.x",
                WndClassTop = true,
                ClientIcon  = "winamp"
            })
        { }
    }
}
