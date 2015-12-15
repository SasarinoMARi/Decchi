using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace Decchi.PublishingModule.Twitter
{
	public partial class InputCaptcha : MetroWindow
	{
		public string Password { get; private set; }

		public InputCaptcha()
		{
			InitializeComponent();
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			this.ctlVerifyKey.Focus();
		}

		private void ctlVerifyKey_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.Submit();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Submit();
		}

		private void Submit()
		{
			this.Password = this.ctlVerifyKey.Text;
			this.DialogResult = true;
			this.Close();
		}

		private bool IsNumbericText(string text)
		{
			int r;
			return int.TryParse(text, out r);
		}


		private void ctlVerifyKey_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsNumbericText(e.Text); 
		}

		private void ctlVerifyKey_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(String)))
			{
				var text = (string)e.DataObject.GetData(typeof(string));
				if (IsNumbericText(text))
					return;
			}
			
			e.CancelCommand();
		}
	}
}
