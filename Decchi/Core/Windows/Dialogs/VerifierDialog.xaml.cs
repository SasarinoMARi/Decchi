using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Decchi.Core.Windows.Dialogs
{
    public partial class VerifierDialog : BaseMetroDialog
    {
        private TaskCompletionSource<object> m_tcs;
        private CancellationTokenRegistration m_cancel;

        internal VerifierDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }
        internal VerifierDialog(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();

            this.m_tcs = new TaskCompletionSource<object>();
            this.m_cancel = DialogSettings.CancellationToken.Register(() => this.m_tcs.TrySetResult(null));
        }
        
        protected override void OnLoaded()
        {
            switch (this.DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    ctlOK.Style = this.FindResource("AccentedDialogHighlightedSquareButton") as Style;
                    ctlText.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    ctlText.SetResourceReference(ControlsHelper.FocusBorderBrushProperty, "TextBoxFocusBorderBrush");
                    break;
            }
        }
        protected override void OnClose()
        {
            this.m_cancel.Dispose();
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
