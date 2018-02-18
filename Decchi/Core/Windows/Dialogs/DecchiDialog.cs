using System.Windows.Controls;

namespace Decchi.Core.Windows.Dialogs
{
    public abstract class DecchiDialog : UserControl
    {
        internal readonly DecchiDialogSetting m_dialogSetting;

        public DecchiDialog()
        { }

        internal DecchiDialog(DecchiDialogSetting dlgSetting)
        {
            this.m_dialogSetting = dlgSetting;
            this.DataContext = dlgSetting;
        }
    }
}
