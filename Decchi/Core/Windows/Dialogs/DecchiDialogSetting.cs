using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;

namespace Decchi
{
    internal class DecchiDialogSetting : INotifyPropertyChanged
    {
        public class SimpleCommand : ICommand
        {
            private readonly Action m_execute;

            public SimpleCommand(Action execute)
            {
                this.m_execute = execute;
            }

            public bool CanExecute(object parameter)
                => true;

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            
            public void Execute(object parameter) => this.m_execute?.Invoke();
        }

        private readonly ICommand m_commandOK;
        private readonly ICommand m_commandCancel;

        private bool m_handled;
        private object m_result;

        public DecchiDialogSetting(Action<DecchiDialogSetting, bool> handler)
        {
            this.m_commandOK     = new SimpleCommand(() => { this.m_handled = true;  handler(this, true); });
            this.m_commandCancel = new SimpleCommand(() => { this.m_handled = false; handler(this, false); });
        }

        public ICommand CommandOk     => this.m_commandOK;
        public ICommand CommandCancel => this.m_commandCancel;

        public bool Handled => this.m_handled;

        public object Result
        {
            get => this.m_result;
            set
            {
                this.m_result = value;
                this.OnPropertyChanged();
            }
        }

        public CustomDialog CreateDialog(object content)
        {
            return new CustomDialog
            {
                Content = content,
                DataContext = this,
            };
        }

        public void OK()     => this.m_commandOK    .Execute(null);
        public void Cancel() => this.m_commandCancel.Execute(null);

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
