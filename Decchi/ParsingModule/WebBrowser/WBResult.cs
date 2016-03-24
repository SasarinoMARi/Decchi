using System;
using System.Diagnostics;

namespace Decchi.ParsingModule.WebBrowser
{
    [DebuggerDisplay("{Title} : {Url}")]
    public struct WBResult
    {
        public string Title;
        public string Url;
        public IntPtr Handle;
        public bool   MainTab;

        public override string ToString()
        {
            return string.Format("0x{0:X8} : {1} : {2}", this.Handle, this.Title, this.Url);
        }
    }
}
