using System;
using System.Collections.Generic;
using System.Windows.Automation;

namespace Decchi.ParsingModule.Rules
{
    internal sealed class WMP : IParseRule
    {
        public WMP() : base(            
            new IParseRuleOption
            {
                Client      = "WMP",
                ParseFlag   = ParseFlags.Local | ParseFlags.ManualOne,
                ClientIcon  = "wmp",
                WndClass    = "WMPlayerApp",
                WndClassTop = true,
            })
        { }

        public override bool Get(SongInfo si, IntPtr hwnd)
        {
            // Finding the WMP window
            hwnd = NativeMethods.FindWindow("WMPlayerApp", "Windows Media Player");
            if (hwnd == IntPtr.Zero) return false;

            var walker          = TreeWalker.ControlViewWalker;
            // Whole WMP window
            var wmpPlayer       = AutomationElement.FromHandle(hwnd);
            var wmpAppHost      = walker.GetFirstChild(wmpPlayer);
            var wmpSkinHost     = walker.GetFirstChild(wmpAppHost);
            // All elements in WMP window
            var wmpContainer    = walker.GetFirstChild(wmpSkinHost);
            // Container with song information
            var wmpSongInfo     = walker.GetFirstChild(wmpContainer);

            // 전체화면이 아님
            if (wmpSongInfo == null) return false;

            // Iterating through all components in container - searching for container with song information
            while (wmpSongInfo.Current.ClassName != "CWmpControlCntr")
            {
                wmpSongInfo = walker.GetNextSibling(wmpSongInfo);

                if (wmpSongInfo == null)
                    return false;
            }

            // Walking through children (image, hyperlink, song info etc.)
            var info = GetChildren(wmpSongInfo);
            info = GetChildren(info[0]);
            info = GetChildren(info[1]);
            info = GetChildren(info[2]);

            // Obtaining elements with desired information
            si.Title  = info[0].Current.Name;
            si.Album  = info[3].Current.Name;
            si.Artist = info[4].Current.Name;
            si.Handle = hwnd;

            return true;
        }

        // Returns all child AutomationElement nodes in "element" node
        private static IList<AutomationElement> GetChildren(AutomationElement element)
        {
            var result = new List<AutomationElement>();
            var walker = TreeWalker.ControlViewWalker;
            var child = walker.GetFirstChild(element);
            result.Add(child);

            while (child != null)
            {
                child = walker.GetNextSibling(child);
                result.Add(child);
            }

            return result;
        }
    }
}
