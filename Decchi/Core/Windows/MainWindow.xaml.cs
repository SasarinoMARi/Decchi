using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Decchi.ParsingModule;
using Decchi.PublishingModule.Twitter;
using Hardcodet.Wpf.TaskbarNotification;
using TweetSharp;

namespace Decchi.Core.Windows
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private static MainWindow   m_instance;
        public  static MainWindow     Instance { get { return m_instance; } }

        public MainWindow( )
        {
            MainWindow.m_instance = this;

            InitializeComponent( );

            this.ctlElements.Visibility = Visibility.Hidden;

            var format = Globals.GetValue("PublishFormat");
            if ( string.IsNullOrEmpty( format ) ) format = Decchi.ParsingModule.SongInfo.defaultFormat;
            this.textbox_FormatString.Text = format;

            this.m_formatOK = ( Brush ) this.FindResource( "BlackColorBrush" );
            this.m_formatErr = Brushes.Red;
        }

        private void ctlTray_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = System.Windows.WindowState.Normal;
            this.Focus();
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
                this.ctlTray.ShowBalloonTip(this.Title, "트레이에서 실행 중이에요!", BalloonIcon.Info);
            }
        }

        public bool SetButtonState( bool progress )
        {
            var b = this.ctlTweet.IsEnabled;
            this.ctlTweet.IsEnabled = progress;
            return b ^ progress;
        }

        private void ctlTweet_Click( object sender, RoutedEventArgs e )
        {
            Task.Run( new Action( DecchiCore.Run ) );
        }

        private bool m_firstActivated = true;
        private async void Window_Activated( object sender, EventArgs e )
        {
            if ( !this.m_firstActivated ) return;
            this.m_firstActivated = false;

            DecchiCore.Login( );

            // 폼에 트위터 유저 정보 매핑
            var me = await Task.Run(new Func<TwitterUser>(() => TwitterCommunicator.Instance.Me));
            if ( me == null )
            {
                MessageBox.Show( "유저 정보를 받아오는데 실패했습니다.", "네트워크 오류" );
                this.Close( );

                return;
            }

            var image = new BitmapImage();
            image.BeginInit( );
            image.UriSource = new Uri( me.ProfileImageUrl.Replace( "_normal", "" ) );
            image.EndInit( );
            image.DownloadCompleted += ( ls, le ) => this.ctlElements.Visibility = Visibility.Visible;

            this.ctlProfile.ImageSource = image;
            this.ctlName.Text = me.Name;
            this.ctlScreenName.Text = "@" + me.ScreenName;
        }

        private Brush m_formatOK;
        private Brush m_formatErr;
        private void textbox_FormatString_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
                this.textbox_FormatString_LostFocus( null, null );
        }

        private void textbox_FormatString_LostFocus( object sender, RoutedEventArgs e )
        {
            if ( string.IsNullOrEmpty( this.textbox_FormatString.Text ) )
            {
                var format = this.textbox_FormatString.Text = SongInfo.defaultFormat;

                Globals.SetValue( "PublishFormat", format );
            }
            else
            {
                if ( SongInfo.CheckFormat( this.textbox_FormatString.Text ) )
                {
                    this.textbox_FormatString.Foreground = this.m_formatOK;

                    var format = this.textbox_FormatString.Text;
                    Globals.SetValue( "PublishFormat", format );
                }
                else
                {
                    this.textbox_FormatString.Foreground = this.m_formatErr;
                }
            }
        }
    }
}
