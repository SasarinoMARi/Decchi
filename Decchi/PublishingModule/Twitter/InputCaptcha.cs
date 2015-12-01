using System;
using System.Windows.Forms;
using Decchi;

namespace PublishingModule.Twitter
{
	public partial class InputCaptcha : Form
	{
		private string _captcha = string.Empty;
		private bool _safeExit = false;
        public string Captcha { get { return _captcha; } }
		public bool SafeExit { get { return _safeExit; } }

		public InputCaptcha(string url)
		{
			InitializeComponent( );
			Globals.OpenWebSite( url );
		}

		private void btn_summit_Click( object sender, EventArgs e )
		{
			_captcha = textBox_captcha.Text;
			_safeExit = true;
            this.Close( );
		}

		private void textBox_captcha_KeyPress( object sender, KeyPressEventArgs e )
		{
			if ( !( Char.IsDigit( e.KeyChar ) ) && e.KeyChar != ( int ) Keys.Back )
			{
				e.Handled = true;
			}
		}

		private void textBox_captcha_KeyDown( object sender, KeyEventArgs e )
		{
			if(e.KeyData == Keys.Enter)
			{
				_captcha = textBox_captcha.Text;
				this.Close( );
			}
		}
	}
}
