using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Decchi.Utilities;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Decchi.Core.Windows.Dialogs
{
    public partial class VerifierDialog : BaseMetroDialog
    {
        private TaskCompletionSource<object> m_tcs;
        private CancellationTokenRegistration m_cancel;

        private MainWindow m_mainwindow;
        private ClipboardMonitor m_clipboardMonitor;

        internal VerifierDialog(MainWindow parentWindow)
            : base(parentWindow, null)
        {
            InitializeComponent();

            this.m_mainwindow = parentWindow;

            this.m_tcs = new TaskCompletionSource<object>();
            this.m_cancel = DialogSettings.CancellationToken.Register(() => this.m_tcs.TrySetResult(null));
        }
        
        protected override void OnLoaded()
        {
            this.ctlOK.Style = this.FindResource("AccentedDialogHighlightedSquareButton") as Style;
            this.ctlText.SetResourceReference(ForegroundProperty, "BlackColorBrush");
            this.ctlText.SetResourceReference(ControlsHelper.FocusBorderBrushProperty, "TextBoxFocusBorderBrush");

            this.m_clipboardMonitor = new ClipboardMonitor(this.OwningWindow);
            this.m_clipboardMonitor.ClipboardChanged += this.m_clipboardMonitor_ClipboardChanged;
        }

        private void m_clipboardMonitor_ClipboardChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                var str = Clipboard.GetText(TextDataFormat.Text);
                if (str.Length == 7 && IsNumbericText(str))
                {
                    this.m_mainwindow.ShowWindow();
                    this.m_mainwindow.Focus();

                    this.ctlText.Text = str;
                    this.ctlText.SelectionStart = str.Length;
                    this.ctlText.Focus();
                }
            }
        }

        protected override void OnClose()
        {
            this.m_cancel.Dispose();
            this.m_clipboardMonitor.ReleaseHandle();
        }

        public override Task<object> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                this.ctlText.Focus();
            }));
            return this.m_tcs.Task;
        }

        private void BaseMetroDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.m_tcs.TrySetResult(null);
        }

        private void ctlText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_tcs.TrySetResult(this.ctlText.Text);
        }
        private void ctlText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumbericText(e.Text);
        }
        private void ctlText_DataObject_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (IsNumbericText(text))
                    return;
            }

            e.CancelCommand();
        }
        private static bool IsNumbericText(string text)
        {
            int r;
            return int.TryParse(text, out r);
        }

        private void ctlOK_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_tcs.TrySetResult(this.ctlText.Text);
        }
        private void ctlCancel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_tcs.TrySetResult(null);
        }

        private void ctlOK_Click(object sender, RoutedEventArgs e)
        {
            this.m_tcs.TrySetResult(this.ctlText.Text);
            e.Handled = true;
        }
        private void ctlCancel_Click(object sender, RoutedEventArgs e)
        {
            this.m_tcs.TrySetResult(null);
            e.Handled = true;
        }
    }
}
