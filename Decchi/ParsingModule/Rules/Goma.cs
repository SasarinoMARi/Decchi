namespace Decchi.ParsingModule.Rules
{
    internal sealed class Goma : IParseRule
    {
        public Goma() : base(            
            new IParseRuleOption
            {
                Client      = "곰오디오",
                ParseFlag   = ParseFlags.Default,
                Regex       = @"^((?<artist>.+) - )?(?<title>.+) - 곰오디오$",
                WndClass    = "GomAudio1.x",
                WndClassTop = true,
                ClientIcon  = "goma"
            })
        { }
    }
}
