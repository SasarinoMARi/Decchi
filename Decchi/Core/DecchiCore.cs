using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Decchi.Core.Windows;
using Decchi.ParsingModule;
using Decchi.PublishingModule.Twitter;
using Decchi.Utilities;

namespace Decchi.Core
{
    /// <summary>
    /// 뎃찌의 내부 프레임워크를 정의합니다.
    /// 항시 사용하는 클래스이므로 싱글턴에서 static 으로 변경
    /// </summary>
    public static class DecchiCore
    {
        private static GlobalKeyboardHook manager;

        static DecchiCore()
        {
            manager = new GlobalKeyboardHook();
            manager.KeyDown += HookManager_KeyDown;
        }

        private static bool m_inited = false;
        public static void Inited()
        {
            m_inited = true;

            HookSetting();
        }

        /// <summary>
        /// 설정 값에 따라서 후킹 이벤트를 등록하거나 해제합니다
        /// </summary>
        public static void HookSetting()
        {
            if (!m_inited) return;

            if (Globals.Instance.UseShortcut)
            {
                manager.hook();
                manager.HookedKeys.Clear();
                manager.HookedKeys.Add(Globals.Instance.Shortcut.Key);
            }
            else
            {
                manager.unhook();
            }
        }

        /// <summary>
        /// 전역 키보드 이벤트를 중지합니다
        /// </summary>
        public static bool DisableKeyEvent { get; set; }

        /// <summary>
        /// 전역 키보드 이벤트 정의부
        /// </summary>
        /// <param name="sender">이벤트 발생 오브젝트</param>
        /// <param name="e">키 이벤트</param>
        static void HookManager_KeyDown(object sender, GlobalKeyboardHook.KeyHookEventArgs e)
        {
            if (DisableKeyEvent || (Globals.Instance.SkipFullscreen && NativeMethods.DetectFullscreenMode())) return;

            if (Keyboard.Modifiers == Globals.Instance.Shortcut.Modifier)
            {
                Task.Run(new Action(DecchiCore.Run));
            }
        }

        /// <summary>
        /// 정의된 퍼블리싱 모듈들의 로그인 처리
        /// </summary>
        public static bool Login()
        {
            return TwitterCommunicator.Instance.Login();
        }

        /// <summary>
        /// 뎃찌에서 실행중인 음악 리스트를 만들어 퍼블리싱 모듈에 전달합니다.
        /// </summary>
        public static void Run()
        {
            App.Debug("Run");
            App.Debug("Invoke");
            if (!(bool)MainWindow.Instance.Dispatcher.Invoke(new Func<bool, bool>(MainWindow.Instance.SetButtonState), false))
                return;

            App.Debug("Get");
            var infos = SongInfo.GetCurrentPlayingSong();
            
            if (infos.Length >= 2)
            {
                // 두 개 이상의 곡이 재생중인 경우
                if (!Globals.Instance.AutoSelect)
                {
                    MainWindow.Instance.Dispatcher.Invoke(new Action(MainWindow.Instance.ShowSelectWindow));
                    return;
                }

                var topHwnd = NativeMethods.GetTopMostWindow(infos.Select(e => e.Handle).ToArray());

                var top = infos.Where(e => e.Handle == topHwnd).ToArray();

                if (top.Length == 1)
                    TwitterCommunicator.Instance.Publish(top[1]);
                else
                {
                    // 가장 위에 있는 창이 여러개면 그건 웹브라우저다!! (아마도)
                    // 메인 창을 찾는다

                    var main = top.Where(e => e.MainTab).ToArray();

                    if (main.Length == 1)
                        TwitterCommunicator.Instance.Publish(main[0]);
                    else
                        // 설마 이런경우가 있겠어?
                        TwitterCommunicator.Instance.Publish(top[0]);
                }
            }
            else
            {
                if (infos.Length == 0)
                {
                    // 재생중인 곡이 없는 경우
                }
                else
                {
                    // 하나의 곡이 재생중인 경우
                    TwitterCommunicator.Instance.Publish(infos[0]);
                }

            }

            SongInfo.Clear();

            MainWindow.Instance.Dispatcher.Invoke(new Func<bool, bool>(MainWindow.Instance.SetButtonState), true);
        }
    }
}
