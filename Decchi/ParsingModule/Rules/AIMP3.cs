namespace Decchi.ParsingModule.Rules
{
    internal sealed class AIMP3 : IParseRule
    {
        public AIMP3() : base(
            new IParseRuleOption
            {
                Client      = "AIMP3",
                ParseFlag   = ParseFlags.Default,// | ParseFlags.Pipe,
                Regex       = "^((?<artist>.+) - )?(?<title>.+)$",
                Ignore      = "^AIMP$",
                WndClass    = "TAIMPMainForm",
                WndClassTop = true,
                ClientIcon  = "aimp3",
                /*
                PluginUrl   = "https://github.com/Usagination/Decchi/blob/master/Decchi-Plugins/aimp_decchi/README.md",
                PipeName    = "468A8F7A-4F9C-4E12-BD34-9A23C1FCDE65"
                */
            })
        { }
    }
}
