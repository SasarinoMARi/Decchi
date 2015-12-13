using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using PublishingModule.Twitter;
using Tweetinvi.Core.Interfaces;

namespace Decchi.Core
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			var format = Globals.GetValue("PublishFormat");
			if ( format == string.Empty ) format = ParsingModule.SongInfo.defaultFormat;
			this.textbox_FormatString.Text = format;
		}

		private async void ctlTWeet_Click(object sender, RoutedEventArgs e)
		{
			this.ctlTWeet.IsEnabled = false;

			await Task.Run( new Action( ( ) => DecchiCore.Run() ) );

			this.ctlTWeet.IsEnabled = true;
		}

		private bool m_firstActivated = true;
		private async void Window_Activated(object sender, EventArgs e)
		{
			if (this.m_firstActivated)
			{
				this.m_firstActivated = false;

				// 폼에 트위터 유저 정보 매핑
				var me = await Task.Run(new Func<ILoggedUser>(() => TwitterCommunicator.Instance.Me));
				if (me != null)
				{

					var image	= new BitmapImage();

					image.CreateOptions = BitmapCreateOptions.None;
					image.CacheOption = BitmapCacheOption.OnLoad;

					image.BeginInit();
					image.UriSource = new Uri(me.ProfileImageUrl.Replace("_normal", ""));
					image.EndInit();

					this.ctlProfileImage.Source = image;

					this.ctlName.Text			= me.Name;
					this.ctlScreenName.Text		= "@" + me.ScreenName;
				}
				else
				{
					MessageBox.Show("유저 정보를 받아오는데 실패했습니다.", "네트워크 오류");
					this.Close();
				}
			}
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
	}
}
