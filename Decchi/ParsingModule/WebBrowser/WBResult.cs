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

        public override string ToString()
        {
            return string.Format("{{{0} : {1}}}", this.Title, this.Url);
        }
    }
}
