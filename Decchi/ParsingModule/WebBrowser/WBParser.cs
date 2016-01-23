using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decchi.Core;

namespace Decchi.ParsingModule.WebBrowser
{
    internal abstract class WBParser
    {
        private static readonly WBParser[] Parsers;

        static WBParser()
        {
            var type = typeof(WBParser);
            WBParser.Parsers = type.Assembly.GetTypes().Where(e => e.IsClass && e.IsSealed && e.IsSubclassOf(type)).Select(e => (WBParser)Activator.CreateInstance(e)).ToArray();
        }

        public static IList<WBResult> Parse(bool detail)
        {
            var lst = new List<WBResult>();

            Parallel.ForEach(Parsers, e => e.ParsePriv(detail, lst));

            return lst;
        }

        protected virtual string WndClassName { get { return null; } } 

        private void ParsePriv(bool detail, IList<WBResult> lst)
        {
            string title;

            var hwnd = IntPtr.Zero;
            while ((hwnd = NativeMethods.FindWindowEx(IntPtr.Zero, hwnd, this.WndClassName, null)) != IntPtr.Zero)
            {
                App.Debug("0x{0:X} : {1}", hwnd, this.WndClassName);

                if (detail)
                {
                    try
                    {
                        this.GetByUIAutomation(hwnd, lst);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        App.Debug(ex);
                    }
                }
                
                title = NativeMethods.GetWindowTitle(hwnd);
                if (string.IsNullOrWhiteSpace(title)) continue;

                lst.Add(new WBResult { Handle = hwnd, Title = title, MainTab = true });
            }
        }

        protected string GetUrl(string url)
        {
            if (url == null) return null;

            if (!url.StartsWith("http"))
                url = "http://" + url;

            try
            {
                return new Uri(url).ToString();
            }
            catch
            { }

            return null;
        }

        protected virtual void GetByUIAutomation(IntPtr handle, IList<WBResult> lst)
        { }

        protected string DeleteEndString(string src, string findString)
        {
            return src.EndsWith(findString) ? src.Substring(0, src.Length - findString.Length) : src;
        }
    }
}
