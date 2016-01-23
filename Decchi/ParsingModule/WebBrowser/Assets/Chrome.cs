using System;
using System.Collections.Generic;
using System.Windows.Automation;

namespace Decchi.ParsingModule.WebBrowser
{
    internal sealed class Chrome : WBParser
    {
        protected override string WndClassName { get { return "Chrome_WidgetWin_1"; } }

        protected override void GetByUIAutomation(IntPtr handle, IList<WBResult> lst)
        {
            string title;
            string url = null;
            bool currentTab;

            var rootElement  = AutomationElement.FromHandle(handle);

            var chrome = rootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Chrome", PropertyConditionFlags.IgnoreCase));
            if (chrome == null) return;

            var tab = chrome.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab));
            if (tab == null) return;

            var tabitems = tab.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
            if (tabitems.Count == 0)
            {
                // 최소화중일때는 탭 인식이 안된다.
                // 현재 탭만 가지고 인식                
                title = NativeMethods.GetWindowTitle(handle);
                url = GetCurrentUrl(chrome);

                lock (lst)
                    lst.Add(new WBResult { Title = title, Url = url, Handle = handle, MainTab = true });
            }
            else
            {
                foreach (AutomationElement tabitem in tabitems)
                {
                    title = base.DeleteEndString(tabitem.Current.Name, "- Chrome");
                    if (!string.IsNullOrWhiteSpace(title)) continue;

                    currentTab = (bool)tabitem.GetCurrentPropertyValue(AutomationElementIdentifiers.IsLegacyIAccessiblePatternAvailableProperty) &&
                                 (((int)tabitem.GetCurrentPropertyValue(LegacyIAccessiblePattern.StateProperty) & 2) == 2);

                    url = currentTab ? GetCurrentUrl(chrome) : null;

                    lock (lst)
                        lst.Add(new WBResult { Title = title, Url = url, Handle = handle, MainTab = currentTab });
                }
            }
        }

        private string GetCurrentUrl(AutomationElement chrome)
        {
            try
            {
                string url = null;

                var edits = chrome.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                if (edits.Count == 0) return null;

                foreach (AutomationElement edit in edits)
                {
                    url = ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;

                    if (!string.IsNullOrWhiteSpace(url))
                        return base.GetUrl(url);
                }
            }
            catch
            { }

            return null;
        }
    }
}
