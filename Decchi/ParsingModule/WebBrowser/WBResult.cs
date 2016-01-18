using System;
using System.Diagnostics;

namespace Decchi.ParsingModule.WebBrowser
{
    [DebuggerDisplay("{Title} : {Url}")]
    internal struct WBResult
    {
        public string Title;
        public string Url;
        public IntPtr Handle;
        public bool MainTab;
    }
}
