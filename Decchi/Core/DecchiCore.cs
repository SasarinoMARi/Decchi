using System;
using System.Threading;
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
        public static void Login()
        {
            TwitterCommunicator.Instance.Login();
        }

        /// <summary>
        /// 뎃찌에서 실행중인 음악 리스트를 만들어 퍼블리싱 모듈에 전달합니다.
        /// </summary>
        public static void Run()
        {
            if (!(bool)MainWindow.Instance.Dispatcher.Invoke(new Func<bool, bool>(MainWindow.Instance.SetButtonState), false))
                return;

            int i;

            var playingCount = 0;
            SongInfo.Clear();
            Parallel.ForEach(SongInfo.SongInfos, e => { e.GetCurrentPlayingSong(); if (e.Loaded) Interlocked.Increment(ref playingCount); } );
            
            /*
            for (i = 0; i < SongInfo.SongInfos.Length; ++i)
            {
                SongInfo.SongInfos[i].GetCurrentPlayingSong();
            
                if (SongInfo.SongInfos[i].Loaded)
                    Interlocked.Increment(ref playingCount);
            }
            */
            
            if (playingCount >= 2)
            {
                // 두 개 이상의 곡이 재생중인 경우

                TwitterCommunicator.Instance.Publish(MainWindow.Instance.Dispatcher.Invoke<SongInfo>(ShowSelectWindow));
            }
            else if (playingCount == 0)
            {
                // 재생중인 곡이 없는 경우
            }
            else
            {
                // 하나의 곡이 재생중인 경우
                for (i = 0; i < SongInfo.SongInfos.Length; ++i)
                {
                    if (SongInfo.SongInfos[i].Loaded)
                    {
                        TwitterCommunicator.Instance.Publish(SongInfo.SongInfos[i]);
                        break;
                    }
                }
            }

            SongInfo.Clear();
            MainWindow.Instance.Dispatcher.Invoke(new Func<bool, bool>(MainWindow.Instance.SetButtonState), true);
        }

        private static SongInfo ShowSelectWindow()
        {
            var window = new ClientSelectWindow();
            window.Owner = MainWindow.Instance;
            window.ShowDialog();

            return window.SongInfo;
        }
    }
}
