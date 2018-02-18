using System.Windows.Controls;
using System.Windows.Input;
using Decchi.ParsingModule;

namespace Decchi.Core.Windows.Dialogs
{
    public partial class ADSelectionDialog : DecchiDialog
    {
        internal ADSelectionDialog(DecchiDialogSetting dlgSetting)
            : base(dlgSetting)
        {
            InitializeComponent();

            this.ctlList.ItemsSource = IParseRule.RulesPlayer;
        }

        private void BaseMetroDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.m_dialogSetting.CommandCancel.Execute(null);
        }

        private void ctlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ctlOK.IsEnabled = true;
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
