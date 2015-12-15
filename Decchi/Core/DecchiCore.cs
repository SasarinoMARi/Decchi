using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Decchi.ParsingModule;
using Decchi.PublishingModule.Twitter;
using Decchi.Utilities;

namespace Decchi
{
	/// <summary>
	/// 뎃찌의 내부 프레임워크를 정의합니다.
	/// 항시 사용하는 클래스이므로 싱글턴에서 static 으로 변경
	/// </summary>
	public static class DecchiCore
	{
		private static globalKeyboardHook manager = new globalKeyboardHook();

		private static Type[] m_types;

		static DecchiCore()
		{
			int i = 0;

			var lst = new List<Type>();

			var songInfoType = typeof(SongInfo);

			var types = Assembly.GetExecutingAssembly().GetTypes();
			for (i = 0; i < types.Length; ++i)
			{
				if (!types[i].IsClass) continue;
				if (types[i].Namespace != songInfoType.Namespace) continue;
				if (!types[i].IsSubclassOf(songInfoType)) continue;

				lst.Add(types[i]);
			}

			m_types = lst.ToArray();

			// 전역 키보드 후킹 이벤트를 초기화합니다,.
			manager.HookedKeys.Add( Key.Q );
			manager.KeyDown += HookManager_KeyDown;
		}
		
		/// <summary>
		/// 전역 키보드 이벤트 정의부
		/// </summary>
		/// <param name="sender">이벤트 발생 오브젝트</param>
		/// <param name="e">키 이벤트</param>
		static void HookManager_KeyDown(ref bool handeled, Key key)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
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
			int i;

			var format = Globals.GetValue("PublishFormat");
			if (string.IsNullOrEmpty(format)) format = Decchi.ParsingModule.SongInfo.defaultFormat;

			SongInfo[] songs = new SongInfo[m_types.Length];

			for (i = 0; i < m_types.Length; ++i)
				songs[i] = (SongInfo)Activator.CreateInstance(m_types[i]);

			var playingCount = 0;
			Parallel.ForEach(songs, e => { e.GetCurrentPlayingSong(); if (e.Loaded) Interlocked.Increment(ref playingCount); } );

			if (playingCount >= 2)
			{
				// 두 개 이상의 곡이 재생중인 경우

			}
			else if (playingCount == 0)
			{
				// 재생중인 곡이 없는 경우

			}
			else
			{
				// 하나의 곡이 재생중인 경우
				for (i = 0; i < songs.Length; ++i)
				{
					if (songs[i].Loaded)
					{
						TwitterCommunicator.Instance.Publish(songs[i].ToString(format));
						break;
					}
				}
			}
		}
		public static void Run(Action callback)
		{
			Run();
			callback();
		}
	}
}
