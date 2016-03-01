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
        
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wnd = d as MainWindow;
            if (e.Property == TweetableProp)
                wnd.ctlTrayDecchi.IsEnabled = (bool)e.NewValue;
        }
        public static readonly DependencyProperty TweetableProp = DependencyProperty.Register("Tweetable", typeof(bool), typeof(MainWindow), new FrameworkPropertyMetadata(true, MainWindow.PropertyChangedCallback));
        public bool Tweetable
        {
            get { return (bool)this.GetValue(TweetableProp); }
            set { this.SetValue(TweetableProp, value); }
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

        private MainWindowWnc m_wnc;
        private Storyboard m_toMini;
        private Storyboard m_toNormal;

        private bool   m_updatable;
        private string m_updateUrl;

        private bool m_exit;
        
        public MainWindow( )
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant);

            MainWindow.Instance = this;
            App.Current.MainWindow = this;
            
            InitializeComponent( );

            Globals.Instance.LoadSettings();

            this.m_toMini   = (Storyboard)this.Resources["ToMini"];
            this.m_toNormal = (Storyboard)this.Resources["ToNormal"];

            this.m_formatOK = (Brush)this.FindResource("BlackColorBrush");
            this.m_formatErr = Brushes.Red;
            this.m_formatEdit = Brushes.Blue;

            this.ctlVersion.Text = App.Version.ToString();
            this.ctlFormat.Foreground = this.m_formatOK;

            this.ctlElements.Visibility = Visibility.Collapsed;

            var g = !Globals.Instance.MiniMode;
            this.Width  = g ? 240 : 110;
            this.Height = g ? 400 : 80;
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
            this.ToMiniMode();
        }
        private void ctlToNormalMode_Click(object sender, RoutedEventArgs e)
        {
            this.ToNormalMode();
        }

        private void ToMiniMode()
        {
            this.ctlSettingFlyout.IsOpen = this.ctlPluginFlyout.IsOpen = false;

            Globals.Instance.MiniMode = true;
            this.ShowMinButton = false;
            this.ctlUpdate.Visibility = Visibility.Collapsed;

            this.m_toMini.Begin();
        }
        private void ToNormalMode()
        {
            Globals.Instance.MiniMode = false;
            this.ShowMinButton = true;

            if (this.m_updatable)
                this.ctlUpdate.Visibility = Visibility.Visible;

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

            App.Debug("===== Update =====");
            App.Debug(this.m_updateUrl);
            var progress = await this.ShowProgressAsync("XD", "새 파일 다운로드중!", true);
            progress.Minimum = 0;
            progress.Maximum = 1000;
            
            var newFile = App.ExePath + ".new";
            var downloadSuccess = false;
            try
            {
                var req = WebRequest.Create(this.m_updateUrl) as HttpWebRequest;
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
                    while (!progress.IsCanceled && (read = await stream.ReadAsync(buff, 0, 4096)) > 0)
                    {
                        down += read;
                        await file.WriteAsync(buff, 0, read);

                        progress.SetProgress(1000d / total * down);
                    }
                    file.Flush();
                    App.Debug("===== Download Complete");
                }

                if (!progress.IsCanceled)
                    downloadSuccess = true;
            }
            catch (Exception ex)
            {
                downloadSuccess = false;

                App.Debug("===== Download Fail");
                App.Debug(ex);
            }

            await progress.CloseAsync();

            if (!downloadSuccess)
            {
                this.ctlElements.Visibility = Visibility.Visible;
                this.ctlUpdate.IsEnabled = true;

                try
                {
                    if (File.Exists(newFile))
                        File.Delete(newFile);
                }
                catch
                { }

                if (!progress.IsCanceled)
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

            await this.ShowMessageAsync(": )", "업데이트를 위해서 재시작할게요");

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
            if (!this.m_exit && Globals.Instance.TrayWhenClose)
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
        public void ShowWindow()
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
            Task.Run(new Action(DecchiCore.Run));
        }

        private void ctlExit_Click(object sender, RoutedEventArgs e)
        {
            this.m_exit = true;
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

        public void SetButtonState( bool progress )
        {
            this.Tweetable = progress;
        }

        private void ctlTweet_Click( object sender, RoutedEventArgs e )
        {
            Task.Run( new Action( DecchiCore.Run ) );
        }
        public void PublishError()
        {
            if (!Globals.Instance.MiniMode)
                this.ShowMessageAsync("X(", "뎃찌!! 하지 못했어요\n\n" + TwitterCommunicator.Instance.GetLastError());
        }
        
        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 왜인지 몰라도 Login 함수에 async 시켜서 실행하면 씹고 다음라인 실행하더라
            if (!DecchiCore.Login())
            {
                this.ToNormalMode();

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
            var thdTwitter  = Task.Run(new Func<TwitterUser>(TwitterCommunicator.Instance.RefrashMe));
            var thdUpdate   = Task.Run(new Func<bool>(() => App.CheckNewVersion(out m_updateUrl)));

            // 폼에 트위터 유저 정보 매핑
            var me = await thdTwitter;
            if ( me == null )
            {
                this.ToNormalMode();
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

                this.ctlShowPlugin.IsEnabled = true;
                this.ctlPluginsList.ItemsSource = SongInfo.RulesPipe;

                DecchiCore.Inited();
            };

            this.ctlProfileImage.ImageSource = image;
            
            // 패치노트 읽을 것인지 물어봄
            if (App.ShowPatchNote && !Globals.Instance.MiniMode)
                if (await this.ShowMessageAsync(": )", "이번 패치노트 읽어볼래요?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "좋아요!", NegativeButtonText = "됐어요;(" }) == MessageDialogResult.Affirmative)
                    Globals.OpenWebSite(string.Format("https://github.com/Usagination/Decchi/releases/tag/{0}", App.Version));

            // 업데이트를 확인함
            this.m_updatable = await thdUpdate;
            if (this.m_updatable && !Globals.Instance.MiniMode)
                this.ctlUpdate.Visibility = Visibility.Visible;
        }

        public async void ShowSelectWindow()
        {
            this.ShowWindow();

            var songinfo = (SongInfo)(await this.ShowBaseMetroDialog(new ClientSelectionDialog(this)));
            if (songinfo != null && !await Task.Run(new Func<bool>(() => TwitterCommunicator.Instance.Publish(songinfo))))
                MainWindow.Instance.PublishError();

            SongInfo.AllClear();
            this.SetButtonState(true);

            DecchiCore.Sync();
        }

        private Brush m_formatOK;
        private Brush m_formatEdit;
        private Brush m_formatErr;
        private void ctlFormat_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
                this.ctlFormat_LostFocus( null, null );
            else if (e.Key == Key.Escape)
            {
                this.ctlFormat.Text = Globals.Instance.PublishFormat;
                this.ctlFormat.Foreground = this.m_formatOK;
            }
        }
        private void ctlFormat_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.ctlFormat.Foreground = this.m_formatEdit;
        }
        private void ctlFormat_LostFocus( object sender, RoutedEventArgs e )
        {
            this.ctlFormat.IsUndoEnabled = false;
            this.ctlFormat.IsUndoEnabled = true;

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

            var songinfo = (sender as FrameworkElement).Tag as IParseRule;
            
            Globals.OpenWebSite(songinfo.PluginUrl);
        }

        public async void CrashReport(string crashfile)
        {
            this.ToNormalMode();
            await this.ShowMessageAsync("X(", "심각한 오류가 발생했어요\n\n파일 이름 : " + crashfile, MessageDialogStyle.Affirmative , new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });

            App.Current.Shutdown();
        }

        private async void ctlAutoDecchi_IsCheckedChanged(object sender, EventArgs e)
        {
            if (this.ctlAutoDecchi.IsChecked.Value)
            {
                var rule = (IParseRule)(await this.ShowBaseMetroDialog(new ADSelectionDialog(this)));

                if (rule != null)
                {
                    DecchiCore.DisableKeyEvent = true;

                    Globals.Instance.AutoDecchi = rule;

                    this.Tweetable = false;
                    
                    return;
                }
            }
            this.ctlAutoDecchi.IsChecked = false;

            DecchiCore.DisableKeyEvent = false;

            Globals.Instance.AutoDecchi = null;

            this.Tweetable = true;
        }

        private async void ctlPluginFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => SongInfo.CheckPipe());
        }
    }
}
