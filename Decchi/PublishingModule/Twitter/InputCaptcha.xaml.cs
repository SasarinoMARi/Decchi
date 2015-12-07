using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Decchi;
using Tweetinvi.Core.Credentials;

namespace PublishingModule.Twitter
{
	public partial class InputCaptcha : Window
	{
		private IConsumerCredentials m_cred;
		public ITwitterCredentials Credentials { get; private set; }

		public InputCaptcha(string consumer_Key, string consumer_Secret)
		{
			InitializeComponent();
			this.m_cred = new ConsumerCredentials(consumer_Key, consumer_Secret);
		}
		
		private bool m_firstActivated = true;
		private async void Window_Activated(object sender, EventArgs e)
		{
			if (this.m_firstActivated)
			{
				this.m_firstActivated = false;

				var url = await Task.Run<string>(() => Tweetinvi.CredentialsCreator.GetAuthorizationURL(m_cred));
				Globals.OpenWebSite(url);

				this.ctl.IsEnabled = true;
			}
		}

		private void ctlVerifyKey_KeyDown(object sender, KeyEventArgs e)
		{
			if ((Key.D0 < e.Key && e.Key < Key.D9) ||
				(Key.NumPad0 < e.Key && e.Key < Key.NumPad9) || 
				e.Key == Key.Delete ||
				e.Key == Key.Back ||
				Keyboard.Modifiers == ModifierKeys.Control)
			{ }
			else if (e.Key == Key.Enter)
				this.Submit();
			else
				e.Handled = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Submit();
		}

		private async void Submit()
		{
			this.ctl.IsEnabled = false;

			string key = this.ctlVerifyKey.Text.Trim();

			this.Credentials = await Task.Run<ITwitterCredentials>(() => Tweetinvi.CredentialsCreator.GetCredentialsFromVerifierCode(key, this.m_cred));

			this.DialogResult = true;
			this.Close();
		}
	}
}
