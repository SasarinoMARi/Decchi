using System.Windows.Controls;
using System.Windows.Input;
using Decchi.ParsingModule;
using MahApps.Metro.Controls.Dialogs;

namespace Decchi.Core.Windows.Dialogs
{
    public partial class ClientSelectionDialog : DecchiDialog
    {        
        internal ClientSelectionDialog(DecchiDialogSetting dlgSetting)
            : base(dlgSetting)
        {
            InitializeComponent();

            this.ctlList.ItemsSource = SongInfo.LastResult;
        }

        private void BaseMetroDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.m_dialogSetting.Cancel();
        }

        private void ctlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ctlOK.IsEnabled = true;
            this.m_dialogSetting.Result = this.ctlList.SelectedItem;
        }

        private void ctlOK_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_dialogSetting.OK();
        }
        private void ctlCancel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_dialogSetting.Cancel();
        }
    }
}
