using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Decchi.Utilities;

namespace Decchi.Core.Windows.Dialogs
{
    public partial class VerifierDialog : DecchiDialog
    {
        private readonly MainWindow m_parentWindow;
        private readonly ClipboardMonitor m_clipboardMonitor;

        internal VerifierDialog(DecchiDialogSetting dlgSetting, MainWindow parentWindow)
            : base(dlgSetting)
        {
            InitializeComponent();
        
            this.m_parentWindow = parentWindow;

            this.m_clipboardMonitor = new ClipboardMonitor(this.m_parentWindow);
            this.m_clipboardMonitor.ClipboardChanged += this.m_clipboardMonitor_ClipboardChanged;
        }

        private void m_clipboardMonitor_ClipboardChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                var str = Clipboard.GetText(TextDataFormat.Text);
                if (str.Length == 7 && IsNumbericText(str))
                {
                    this.m_parentWindow.ShowWindow();
                    this.m_parentWindow.Focus();

                    this.ctlText.Text = str;
                    this.ctlText.SelectionStart = str.Length;
                    this.ctlText.Focus();
                }
            }
        }

        private void BaseMetroDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.m_dialogSetting.Cancel();
        }

        private void ctlText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this.ctlText.Text.Length > 0)
                this.m_dialogSetting.OK();
        }
        private void ctlText_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ctlOK.IsEnabled = this.ctlText.Text.Length > 0;
        }
        private void ctlText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumbericText(e.Text);
        }
        private void ctlText_DataObject_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (IsNumbericText(text))
                    return;
            }

            e.CancelCommand();
        }
        private static bool IsNumbericText(string text)
        {
            return int.TryParse(text, out int r);
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
