using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Decchi.Core.Windows.Dialogs;
using Decchi.ParsingModule;
using Decchi.PublishingModule.Twitter;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls.Dialogs;

namespace Decchi.Core.Windows
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public static MainWindow Instance { get; private set; } 

        public MainWindow( )
        {
            MainWindow.Instance = this;

            InitializeComponent( );
            this.ctlElements.Visibility = Visibility.Hidden;
            this.ctlVersion.Text = Program.Version.ToString();
            
            this.ctlFormat.Text = Globals.Instance.PublishFormat;
            this.m_formatOK = ( Brush ) this.FindResource( "BlackColorBrush" );
            this.m_formatErr = Brushes.Red;

            if (Globals.Instance.TrayStart)
            {
                this.Hide();
                this.ctlTray.ShowBalloonTip(this.Title, "트레이에서 실행 중이에요!", BalloonIcon.Info);
                this.WindowState = WindowState.Minimized;
            }
        }

        private void ctlUpdate_Click(object sender, RoutedEventArgs e)
        {
            Globals.OpenWebSite("https://github.com/Usagination/Decchi/releases/latest");
        }

        private void ctlHomepage_Click(object sender, RoutedEventArgs e)
        {
            Globals.OpenWebSite("http://usagination.github.io/Decchi/");
        }

        private void ctlTray_TrayLeftMouseUp(object sender, RoutedEventArgs e)
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

        private void ctlShowSetting_Click(object sender, RoutedEventArgs e)
        {
            this.ctlSetting.IsOpen = !this.ctlSetting.IsOpen;
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

            // 왜인지 몰라도 Login 함수에 async 시켜서 실행하면 씹고 다음라인 실행하더라
            if (!DecchiCore.Login())
            {
                var requestToken = await Task.Run(new Func<OAuth.TokenPair>(TwitterCommunicator.Instance.OAuth.RequestToken));

                Globals.OpenWebSite("https://api.twitter.com/oauth/authorize?oauth_token=" + requestToken.Token);

                var key = (string)await MainWindow.Instance.ShowBaseMetroDialog(new VerifierDialog(this));

                if (string.IsNullOrEmpty(key))
                {
                    await this.ShowMessageAsync("X(", "트위터에 로그인 하지 못했어요");

                    this.Close();

                    return;
                }

                TwitterCommunicator.Instance.OAuth.User.Token  = requestToken.Token;
                TwitterCommunicator.Instance.OAuth.User.Secret = requestToken.Secret;

                var userToken = await Task.Run(new Func<OAuth.TokenPair>(() => TwitterCommunicator.Instance.OAuth.AccessToken(key)));

                Globals.Instance.TwitterToken  = TwitterCommunicator.Instance.OAuth.User.Token  = userToken.Token;
                Globals.Instance.TwitterSecret = TwitterCommunicator.Instance.OAuth.User.Secret = userToken.Secret;
                Globals.Instance.SaveSettings();
            }

            // 두개 병렬처리
            var thdSongInfo = Task.Run<bool>(new Func<bool>(SongInfo.InitSonginfo));
            var thdTwitter  = Task.Run<TwitterUser>(new Func<TwitterUser>(TwitterCommunicator.Instance.RefrashMe));
            var thdUpdate   = Task.Run(new Func<bool>(Program.CheckNewVersion));

            await thdSongInfo;
            await thdTwitter;

            // 서버에서 SongInfo 데이터를 가져옴
            if (!thdSongInfo.Result)
            {
                await this.ShowMessageAsync("X(", "서버에 연결하지 못했어요");

                this.Close();

                return;
            }

            // 폼에 트위터 유저 정보 매핑
            var me = thdTwitter.Result;
            if ( me == null )
            {
                await this.ShowMessageAsync("X(", "트위터에서 정보를 가져오지 못했어요");

                Globals.Instance.TwitterToken  = null;
                Globals.Instance.TwitterSecret = null;
                Globals.Instance.SaveSettings();

                this.Close( );

                return;
            }

            this.ctlName.Text       = me.Name;
            this.ctlScreenName.Text = "@" + me.ScreenName;

            var image = new BitmapImage();
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.CreateOptions = BitmapCreateOptions.DelayCreation;
            image.BeginInit();
            image.UriSource = new Uri(me.ProfileImageUrl.Replace("_normal", ""));
            image.EndInit();
            image.DownloadCompleted += (ls, le) => { this.ctlElements.Visibility = Visibility.Visible; DecchiCore.Inited(); };

            this.ctlProfileImage.ImageSource = image;

            // 업데이트를 확인함
            if (await thdUpdate)
                this.ctlUpdate.Visibility = Visibility.Visible;
        }

        public async void ShowSelectWindow()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            var songinfo = (SongInfo)(await this.ShowBaseMetroDialog(new ClientSelectionDialog(this)));
            await Task.Run(new Action(() => TwitterCommunicator.Instance.Publish(songinfo)));

            SongInfo.Clear();
            MainWindow.Instance.Dispatcher.Invoke(new Func<bool, bool>(MainWindow.Instance.SetButtonState), true);
        }

        private Brush m_formatOK;
        private Brush m_formatErr;
        private void ctlFormat_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
                this.ctlFormat_LostFocus( null, null );
        }

        private void ctlFormat_LostFocus( object sender, RoutedEventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( this.ctlFormat.Text ) )
            {
                Globals.Instance.PublishFormat = this.ctlFormat.Text = SongInfo.defaultFormat;
                Globals.Instance.SaveSettings();
            }
            else
            {
                if ( SongInfo.CheckFormat( this.ctlFormat.Text ) )
                {
                    this.ctlFormat.Foreground = this.m_formatOK;

                    Globals.Instance.PublishFormat = this.ctlFormat.Text;
                    Globals.Instance.SaveSettings();

                }
                else
                {
                    this.ctlFormat.Foreground = this.m_formatErr;
                }
            }
        }

        private void ctlShortcut_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((
                    e.KeyboardDevice.Modifiers != ModifierKeys.None &&
                    e.KeyboardDevice.Modifiers != ModifierKeys.Windows
                ) &
                (
                    e.Key != Key.LeftShift &&
                    e.Key != Key.RightShift &&
                    e.Key != Key.LeftAlt &&
                    e.Key != Key.RightAlt &&
                    e.Key != Key.LeftCtrl &&
                    e.Key != Key.RightCtrl &&
                    e.Key != Key.System
                ))
                Globals.Instance.Shortcut = new Globals.ShortcutInfo(e.KeyboardDevice.Modifiers, e.Key);
        }

        private void ctlShortcut_GotFocus(object sender, RoutedEventArgs e)
        {
            DecchiCore.DisableKeyEvent = true;
        }

        private void ctlShortcut_LostFocus(object sender, RoutedEventArgs e)
        {
            DecchiCore.DisableKeyEvent = false;
        }
    }
}
