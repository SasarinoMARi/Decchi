using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        private MainWindowWnc m_wnc;
        private Storyboard m_toMini;
        private Storyboard m_toNormal;
        private string     m_newUrl;

		public MainWindow( )
        {
            MainWindow.Instance = this;
            App.Current.MainWindow = this;

            InitializeComponent( );
            this.ctlVersion.Text = App.Version.ToString();
            this.ctlFormat.Text = Globals.Instance.PublishFormat;

            this.m_toMini   = (Storyboard)this.Resources["ToMini"];
            this.m_toNormal = (Storyboard)this.Resources["ToNormal"];

            this.ctlElements.Visibility = Visibility.Collapsed;
            this.ctlTweet.IsEnabled = false;
            
            this.m_formatOK = ( Brush ) this.FindResource( "BlackColorBrush" );
            this.m_formatErr = Brushes.Red;

            var g = !Globals.Instance.MiniMode;
            this.Width  = g ? 240 : 132;
            this.Height = g ? 380 : 80;
            this.ShowMinButton = g;

            this.ctlTray.Visibility = Globals.Instance.TrayVisible ? Visibility.Visible : Visibility.Collapsed;
            if (Globals.Instance.TrayStart)
                this.HideWindow();

            this.m_wnc = new MainWindowWnc(this);
        }

        private void ctlSettingFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            Globals.Instance.SaveSettings();
        }

        private void ctlToMiniMode_Click(object sender, RoutedEventArgs e)
        {
            this.ctlSettingFlyout.IsOpen = this.ctlPluginFlyout.IsOpen = false;

            this.ShowMinButton = false;
            Globals.Instance.MiniMode = true;

            this.m_toMini.Begin();
        }

        private void ctlToNormalMode_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMinButton = true;
            Globals.Instance.MiniMode = false;

            this.m_toNormal.Begin();
        }

        private void MetroWindow_Deactivated(object sender, EventArgs e)
        {
            this.ctlPluginFlyout.IsOpen = this.ctlSettingFlyout.IsOpen = false;
        }

        private async void ctlUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.ctlElements.Visibility = Visibility.Hidden;
            this.ctlUpdate.IsEnabled = false;

            var progress = await this.ShowProgressAsync("XD", "새 파일 다운로드중!");
            progress.Minimum = 0;
            progress.Maximum = 1000;

            var newFile = App.ExePath + ".new";
            var downloadSuccess = false;
            try
            {
                var req = WebRequest.Create(this.m_newUrl) as HttpWebRequest;
                req.UserAgent = "Decchi";
                req.Timeout = 5000;
                req.ReadWriteTimeout = 3000;
                using (var res = await req.GetResponseAsync())
                using (var stream = res.GetResponseStream())
                using (var file = File.Create(newFile))
                {
                    var buff = new byte[4096];
                    int read;
                    int down = 0;
                    int total = (int)res.ContentLength;
                    while ((read = await stream.ReadAsync(buff, 0, 4096)) > 0)
                    {
                        down += read;
                        await file.WriteAsync(buff, 0, read);

                        progress.SetProgress(down * 1000 / total);
                    }
                    file.Flush();
                }

                downloadSuccess = true;
            }
            catch
            {
                downloadSuccess = false;
            }

            await progress.CloseAsync();

            if (!downloadSuccess)
            {
                this.ctlElements.Visibility = Visibility.Visible;
                this.ctlUpdate.IsEnabled = true;
                await this.ShowMessageAsync("X(", "다운로드 실패 :(");
                return;
            }

            var batchPath = Path.Combine("Decchi.bat");
            var sb = new StringBuilder();
            sb.AppendFormat("@echo off\r\n");
            sb.AppendFormat(":del\r\n");
            sb.AppendFormat("del \"{0}\"\r\n", App.ExePath);
            sb.AppendFormat("if exist \"{0}\" goto del\r\n", App.ExePath);
            sb.AppendFormat("move \"{0}\" \"{1}\"\r\n", newFile, App.ExePath);
            sb.AppendFormat("\"{0}\" --updated\r\n", App.ExePath);
            sb.AppendFormat("del \"{0}\"\r\n", batchPath);
            File.WriteAllText(batchPath, sb.ToString());

            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + batchPath) { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
            Application.Current.Shutdown();
        }

        private void ctlHomepage_Click(object sender, RoutedEventArgs e)
        {
            Globals.OpenWebSite("http://usagination.github.io/Decchi/");
        }

        private void ctlPluginHelp_Click(object sender, RoutedEventArgs e)
        {
            this.ctlPluginFlyout.IsOpen = false;
            Globals.OpenWebSite("https://github.com/Usagination/Decchi/blob/master/README.md#뎃찌ext");
        }

        private void ctlTrayVisible_IsCheckedChanged(object sender, EventArgs e)
        {
            this.ctlTray.Visibility = Globals.Instance.TrayVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ctlTray_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            this.ShowWindow();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Globals.Instance.TrayWhenClose)
            {
                this.HideWindow();
                e.Cancel = true;
            }
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (Globals.Instance.TrayWhenMinimize && this.WindowState == WindowState.Minimized)
                this.HideWindow();
            else if (!Globals.Instance.TrayVisible && this.WindowState == WindowState.Normal)
                this.ctlTray.Visibility = Visibility.Collapsed;
        }

        private void HideWindow()
        {
            if (!Globals.Instance.TrayVisible)
                this.ctlTray.Visibility = Visibility.Visible;

            this.Hide();
            this.ctlTray.ShowBalloonTip(this.Title, "트레이에서 실행 중이에요!", BalloonIcon.Info);
        }

        private void ShowWindow()
        {
            if (!Globals.Instance.TrayVisible)
                this.ctlTray.Visibility = Visibility.Collapsed;

            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ctlTrayShow_Click(object sender, RoutedEventArgs e)
        {
            this.ShowWindow();
        }

        private void ctlTrayDecchi_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ctlTweet.IsEnabled)
                Task.Run(new Action(DecchiCore.Run));
        }

        private void ctlExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void ctlShowSetting_Click(object sender, RoutedEventArgs e)
        {
            this.ctlSettingFlyout.IsOpen = !this.ctlSettingFlyout.IsOpen;
            this.ctlPluginFlyout.IsOpen = false;
        }

        private void ctlShowPlugin_Click(object sender, RoutedEventArgs e)
        {
            this.ctlSettingFlyout.IsOpen = false;
            this.ctlPluginFlyout.IsOpen = !this.ctlPluginFlyout.IsOpen;
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
        
        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 왜인지 몰라도 Login 함수에 async 시켜서 실행하면 씹고 다음라인 실행하더라
            if (!DecchiCore.Login())
            {
                var requestToken = await Task.Run(new Func<OAuth.TokenPair>(TwitterCommunicator.Instance.OAuth.RequestToken));

                Globals.OpenWebSite("https://api.twitter.com/oauth/authorize?oauth_token=" + requestToken.Token);

                var key = (string)await MainWindow.Instance.ShowBaseMetroDialog(new VerifierDialog(this));

                if (string.IsNullOrWhiteSpace(key))
                {
                    await this.ShowMessageAsync("X(", "트위터에 로그인 하지 못했어요");

                    Application.Current.Shutdown();

                    return;
                }

                TwitterCommunicator.Instance.OAuth.User.Token  = requestToken.Token;
                TwitterCommunicator.Instance.OAuth.User.Secret = requestToken.Secret;

                var userToken = await Task.Run(new Func<OAuth.TokenPair>(() => TwitterCommunicator.Instance.OAuth.AccessToken(key)));
                if (userToken == null)
                {
                    await this.ShowMessageAsync("X(", "트위터에 로그인 하지 못했어요");

                    Application.Current.Shutdown();

                    return;
                }

                Globals.Instance.TwitterToken  = TwitterCommunicator.Instance.OAuth.User.Token  = userToken.Token;
                Globals.Instance.TwitterSecret = TwitterCommunicator.Instance.OAuth.User.Secret = userToken.Secret;
                Globals.Instance.SaveSettings();
            }

            // 두개 병렬처리
            var thdSongInfo = Task.Run(new Func<bool>(SongInfo.InitSonginfo));
            var thdTwitter  = Task.Run(new Func<TwitterUser>(TwitterCommunicator.Instance.RefrashMe));
            var thdUpdate   = Task.Run(new Func<bool>(() => App.CheckNewVersion(out m_newUrl)));

            // 폼에 트위터 유저 정보 매핑
            var me = await thdTwitter;
            if ( me == null )
            {
                await this.ShowMessageAsync("X(", "트위터에서 정보를 가져오지 못했어요");

                Globals.Instance.TwitterToken  = null;
                Globals.Instance.TwitterSecret = null;
                Globals.Instance.SaveSettings();

                Application.Current.Shutdown();

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
            image.DownloadCompleted += (ls, le) =>
            {
                this.ctlShowSetting.IsEnabled = true;
                this.ctlToNormalMode.IsEnabled = true;
                this.ctlElements.Visibility = Visibility.Visible;
                DecchiCore.Inited();
            };
            this.ctlProfileImage.ImageSource = image;

            // 서버에서 SongInfo 데이터를 가져옴
            if (!await thdSongInfo)
            {
                await this.ShowMessageAsync("X(", "서버에 연결하지 못했어요");

                Application.Current.Shutdown();

                return;
            }
            this.ctlTweet.IsEnabled = true;
            this.ctlShowPlugin.IsEnabled = true;
            this.ctlPluginsList.ItemsSource = SongInfo.RulesWithP;
            
            // 패치노트 읽을 것인지 물어봄
            if (App.ShowPatchNote)
                if (await this.ShowMessageAsync("뎃찌", "이번 패치노트 읽어볼래요?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "읽기", NegativeButtonText = "닫기" }) == MessageDialogResult.Affirmative)
                    Globals.OpenWebSite(string.Format("https://https://github.com/Usagination/Decchi/blob/master/patch-note/{0}.md", App.Version));

            // 업데이트를 확인함
            if (await thdUpdate)
                this.ctlUpdate.Visibility = Visibility.Visible;
        }

        public async void ShowSelectWindow()
        {
            this.ShowWindow();

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

        private void ctlPluginInstall_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.ctlPluginFlyout.IsOpen = false;

            var songinfo = (sender as FrameworkElement).Tag as SongInfo.SongInfoRule;
            
            Globals.OpenWebSite(songinfo.PluginUrl);
        }

        public async void CrashReport(string crashfile)
        {
            await this.ShowMessageAsync("X(", "심각한 오류가 발생했어요\n\n파일 이름 : " + crashfile, MessageDialogStyle.Affirmative , new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });

            App.Current.Shutdown();
        }

        private class MainWindowWnc : System.Windows.Forms.NativeWindow
        {
            private MainWindow m_window;

            public MainWindowWnc(MainWindow window)
            {
                this.m_window = window;

                var helper = new WindowInteropHelper(window);
                if (helper.Handle == IntPtr.Zero)
                    helper.EnsureHandle();

                this.AssignHandle(helper.Handle);
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == 0x056F) // WM_User Range (0x0400 ~ 0x07FF)
                {
                    var l = new IntPtr(0xAB55);
                    if (m.LParam == l && m.WParam == l)
                    {
                        this.m_window.ShowWindow();
                        this.m_window.Activate();

                        m.Result = new IntPtr(1);

                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }
    }
}
