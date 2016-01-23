using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Decchi.ParsingModule;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Decchi.Core.Windows.Dialogs
{
    public partial class ADSelectionDialog : BaseMetroDialog
    {
        private TaskCompletionSource<object> m_tcs;
        private CancellationTokenRegistration m_cancel;

        internal ADSelectionDialog(MetroWindow parentWindow)
            : base(parentWindow, null)
        {
            InitializeComponent();

            this.ctlList.ItemsSource = SongInfo.RulesPlayer;

            this.m_tcs = new TaskCompletionSource<object>();
            this.m_cancel = DialogSettings.CancellationToken.Register(() => this.m_tcs.TrySetResult(null));
        }

        protected override void OnLoaded()
        {
            switch (this.DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    ctlOK.Style = this.FindResource("AccentedDialogHighlightedSquareButton") as Style;
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
            }));
            return this.m_tcs.Task;
        }

        private void BaseMetroDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.m_tcs.TrySetResult(null);
        }

        private void ctlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ctlOK.IsEnabled = true;
        }

        private void ctlOK_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_tcs.TrySetResult(ctlList.SelectedItem);
        }
        private void ctlCancel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.m_tcs.TrySetResult(null);
        }

        private void ctlOK_Click(object sender, RoutedEventArgs e)
        {
            this.m_tcs.TrySetResult(ctlList.SelectedItem);
            e.Handled = true;
        }
        private void ctlCancel_Click(object sender, RoutedEventArgs e)
        {
            this.m_tcs.TrySetResult(null);
            e.Handled = true;
        }
    }
}
