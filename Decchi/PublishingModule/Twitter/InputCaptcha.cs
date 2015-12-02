using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Decchi;
using Tweetinvi.Core.Credentials;

namespace PublishingModule.Twitter
{
	public partial class InputCaptcha : Form
	{
		private IConsumerCredentials m_cred;
		public ITwitterCredentials Credentials { get; private set; }

		public InputCaptcha(string consumer_Key, string consumer_Secret)
		{
			m_cred = new ConsumerCredentials(consumer_Key, consumer_Secret);

			InitializeComponent();
			this.textBox_captcha.Enabled = this.btn_summit.Enabled = false;
		}

		private async void InputCaptcha_Load(object sender, EventArgs e)
		{
			var url = await Task.Run<string>(() => Tweetinvi.CredentialsCreator.GetAuthorizationURL(m_cred));
			Globals.OpenWebSite(url);

			this.textBox_captcha.Enabled = this.btn_summit.Enabled = true;
		}

		private void textBox_captcha_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				Submit();
			}
		}

		private void btn_summit_Click(object sender, EventArgs e)
		{
			Submit();
		}

		private async void Submit()
		{
			this.textBox_captcha.Enabled = this.btn_summit.Enabled = false;

			string key = this.textBox_captcha.Text;

			this.Credentials = await Task.Run<ITwitterCredentials>(() => Tweetinvi.CredentialsCreator.GetCredentialsFromVerifierCode(key, this.m_cred));

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void textBox_captcha_KeyPress(object sender, KeyPressEventArgs e)
		{
			// 거 사람이 복사좀 할 수도 있지!
			if ((!Char.IsDigit(e.KeyChar)) && e.KeyChar != (int)Keys.Back && !char.IsControl(e.KeyChar))
			{
				e.Handled = true;
			}
		}
	}
}
