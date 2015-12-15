using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Decchi.ParsingModule;
using Decchi.PublishingModule.Twitter;
using TweetSharp;

namespace Decchi.Core.Windows
{
	public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
	{
		private static MainWindow m_instance;
		public static MainWindow Instance { get { return m_instance; } }

		public MainWindow()
		{
			MainWindow.m_instance = this;

			InitializeComponent();
			var format = Globals.GetValue("PublishFormat");
			if (string.IsNullOrEmpty(format)) format = Decchi.ParsingModule.SongInfo.defaultFormat;
			this.textbox_FormatString.Text = format;
		}

		private async void ctlTweet_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(new Action(DecchiCore.Run));
		}

		private bool m_firstActivated = true;
		private async void Window_Activated(object sender, EventArgs e)
		{
			if (!this.m_firstActivated) return;
			this.m_firstActivated = false;
			
			DecchiCore.Login();

			// 폼에 트위터 유저 정보 매핑
			var me = await Task.Run(new Func<TwitterUser>(() => TwitterCommunicator.Instance.Me));
			if (me == null)
			{
				MessageBox.Show("유저 정보를 받아오는데 실패했습니다.", "네트워크 오류");
				this.Close();

				return;
			}

			this.ctlTweet.IsEnabled = true;

 			var image = new BitmapImage();
 			image.BeginInit();
 			image.UriSource = new Uri(me.ProfileImageUrl.Replace("_normal", ""));
 			image.EndInit();
			image.DownloadCompleted += (ls, le) => this.ctlProfile.Visibility = Visibility.Visible;

			this.ctlProfileImage.Source = image;
			this.ctlName.Text			= me.Name;
			this.ctlScreenName.Text		= "@" + me.ScreenName;
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			Globals.SaveSettings( );
		}

		private void textbox_FormatString_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			var format = this.textbox_FormatString.Text;
			Globals.SetValue( "PublishFormat", format );
		}

		private void textbox_FormatString_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(this.textbox_FormatString.Text))
				this.textbox_FormatString.Text = SongInfo.defaultFormat;
		}
	}
}
