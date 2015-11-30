using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ParsingModule;
using PublishingModule.Twitter;
using Utilities;

namespace Decchi
{
    /// <summary>
    /// 뎃찌의 내부 프레임워크를 정의합니다.
    /// 이 클래스는 싱글턴 패턴으로 설계되었습니다. DecchiCore.Instance를 통해 인스턴스에 접근할 수 있습니다.
    /// </summary>
    public class DecchiCore
    {
        private static DecchiCore _instance = null;
        //globalKeyboardHook manager = new globalKeyboardHook();

        public static DecchiCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DecchiCore();
                }
                return _instance;
            }
        }
        private DecchiCore()
        {
            // 전역 키보드 후킹 이벤트를 초기화합니다, 만 잘 되지 않네요.
            //manager.HookedKeys.Add(Keys.Q);
            //manager.KeyDown += HookManager_KeyDown;
        }

        /// <summary>
        /// 전역 키보드 이벤트 정의부
        /// </summary>
        /// <param name="sender">이벤트 발생 오브젝트</param>
        /// <param name="e">키 이벤트</param>
        void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                DecchiCore.Instance.Run();
            }
        }

        /// <summary>
        /// 정의된 퍼블리싱 모듈들의 로그인 처리
        /// </summary>
        public void Login()
        {
            TwitterCommunicator.Instance.Login();
        }

        /// <summary>
        /// 뎃찌에서 실행중인 음악 리스트를 만들어 퍼블리싱 모듈에 전달합니다.
        /// </summary>
        public void Run()
        {
            var nowPlayings = new Dictionary<string, SongInfo>();

            var wmpNP = new WMPSongInfo().GetCurrentPlayingSong();
            var gomNP = new GomAudioSongInfo( ).GetCurrentPlayingSong( );

            if (SongInfo.IsEmpty(wmpNP)) nowPlayings[wmpNP.Client] = wmpNP;
            if (SongInfo.IsEmpty(gomNP)) nowPlayings[gomNP.Client] = gomNP;

            var keys = new List<string>(nowPlayings.Keys);

            if(nowPlayings.Count >= 2)
            {
                // 두 개 이상의 곡이 재생중인 경우

            }
            else if(nowPlayings.Count == 0)
            {
                // 재생중인 곡이 없는 경우

            }
            else
            {
                // 하나의 곡이 재생중인 경우
                TwitterCommunicator.Instance.Publish(nowPlayings[keys[0]].ToString());   
            }
        }
    }
}
