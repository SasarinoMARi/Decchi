using System;
using System.Collections.Generic;
using System.Windows.Automation;

namespace Decchi.ParsingModule.WebBrowser
{
    internal sealed class FireFox : WBParser
    {
        protected override string WndClassName { get { return "MozillaWindowClass"; } }

        protected override void GetByUIAutomation(IntPtr handle, IList<WBResult> lst)
        {
            string title;
            string url = null;
            bool currentTab;

            var rootElement  = AutomationElement.FromHandle(handle);
            var toolbars = rootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));

            foreach (AutomationElement toolbar in toolbars)
            {
                var tab = toolbar.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab));
                if (tab == null) continue;

                var tabitems = tab.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

                foreach (AutomationElement tabitem in tabitems)
                {
                    title = tabitem.Current.Name;

                    // 0x200000 : 선택 가능
                    currentTab = (bool)tabitem.GetCurrentPropertyValue(AutomationElementIdentifiers.IsLegacyIAccessiblePatternAvailableProperty) &&
                                 (((int)tabitem.GetCurrentPropertyValue(LegacyIAccessiblePattern.StateProperty) & 0x02) == 2);
                    url = currentTab ? GetCurrentUrl(toolbars) : null;

                    lock (lst)
                        lst.Add(new WBResult { Title = title, Url = url, Handle = handle, MainTab = currentTab });
                }

                break;
            }
        }

        private string GetCurrentUrl(AutomationElementCollection toolbars)
        {
            try
            {
                foreach (AutomationElement toolbar in toolbars)
                {
                    var combobox = toolbar.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox));

                    if (combobox != null)
                    {
                        var edit = combobox.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                        return base.GetUrl(edit.GetCurrentPropertyValue(LegacyIAccessiblePattern.ValueProperty) as string);
                    }
                }
            }
            catch
            { }

            return null;
        }
    }
}
