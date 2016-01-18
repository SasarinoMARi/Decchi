using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Decchi.ParsingModule.WebBrowser
{
    internal abstract class WBParser
    {
        private static readonly WBParser[] Parsers;

        static WBParser()
        {
            var type = typeof(WBParser);

            var lst = new List<WBParser>();

            var types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                if (!types[i].IsClass ||
                    !types[i].IsSealed ||
                    !types[i].IsSubclassOf(type)) continue;

                lst.Add((WBParser)Activator.CreateInstance(types[i]));
            }

            Parsers = lst.ToArray();
        }

        public static WBResult[] Parse(bool detail)
        {
            var lst = new List<WBResult>();

            Parallel.ForEach(Parsers, e => e.ParsePriv(detail, lst));

            return lst.ToArray();
        }


        protected virtual string WndClassName { get { return null; } } 

        private void ParsePriv(bool detail, IList<WBResult> lst)
        {
            string title;

            var hwnd = IntPtr.Zero;
            while ((hwnd = NativeMethods.FindWindowEx(IntPtr.Zero, hwnd, this.WndClassName, null)) != IntPtr.Zero)
            {
                title = NativeMethods.GetWindowTitle(hwnd);
                if (string.IsNullOrWhiteSpace(title)) continue;

                if (!detail)
                    lst.Add(new WBResult { Handle = hwnd, Title = title, MainTab = true });
                else
                {
                    try
                    {
                        GetByUIAutomation(hwnd, lst);
                    }
                    catch
                    { }
                }
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
    }
}
