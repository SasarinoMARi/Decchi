namespace Decchi.ParsingModule.Rules
{
    internal sealed class Foobar : IParseRule
    {
        public Foobar() : base(            
            new IParseRuleOption
            {
                Client      = "Foobar 2000",
                ParseFlag   = ParseFlags.Default | ParseFlags.Pipe,
                Regex       = @"^((?<artist>.+) - )?(\[(?<album>.+)( CD\d+)?( #\d+)?\] )?(?<title>.+)(\/\/.+)? +\[foobar2000 .+\]$",
                WndClass    = "{97E27FAA-C0B3-4b8e-A693-ED7881E99FC1}",
                WndClassTop = true,
                ClientIcon  = "foobar",
                PluginUrl   = "https://github.com/Usagination/Decchi/blob/master/Decchi-Plugins/foo_decchi/README.md",
                PipeName    = "93A0636E-29FB-4DF5-AA8E-C78860C4A624",
            })
        { }
    }
}
