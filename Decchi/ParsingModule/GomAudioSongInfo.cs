using System;
using System.Diagnostics;

namespace Decchi.ParsingModule
{
    public sealed class GomAudioSongInfo : SongInfo
    {
        public override string Client { get { return "곰오디오"; } }
        public override string ClientIcon { get { return "/Decchi;component/ParsingModule/IconImages/GomAudio.png"; } }

        public override bool GetCurrentPlayingSong()
        {
            var hwnd = NativeMethods.FindWindow("GXWINDOW", null);
            if (hwnd == IntPtr.Zero) return false;

            hwnd = NativeMethods.GetParent(hwnd);
            if (hwnd == IntPtr.Zero) return false;

            var str = NativeMethods.GetWindowTitle(hwnd);

            if (!string.IsNullOrEmpty(str) && str.Contains("곰오디오"))
            {
                var sep = str.IndexOf(" - ");
                var sep2 = str.LastIndexOf('-');

                this.Title	= str.Substring(sep + 3, sep2 - sep - 3).Trim();
                this.Album	= null;
                this.Artist	= str.Substring(0, sep).Trim();

                this.Loaded = true;

                return true;
            }

            return false;
        }
    }
}
