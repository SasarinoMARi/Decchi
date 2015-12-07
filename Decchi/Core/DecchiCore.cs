using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ParsingModule;
using PublishingModule.Twitter;
using Utilities;

namespace Decchi
{
	/// <summary>
	/// 뎃찌의 내부 프레임워크를 정의합니다.
	/// 항시 사용하는 클래스이므로 싱글턴에서 static 으로 변경
	/// </summary>
	public static class DecchiCore
	{
		private static globalKeyboardHook manager = new globalKeyboardHook();

		static DecchiCore()
		{
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
				DecchiCore.Run();
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
		public static void Run( string format = SongInfo.defaultFormat )
		{
			var nowPlayings = new Dictionary<string, SongInfo>();
			var playingCount = 0;

			List<SongInfo> songs = new List<SongInfo>();
			songs.Add(new WMPSongInfo());
			songs.Add(new GomAudioSongInfo());
			songs.Add( new iTunesSongInfo( ) );
			songs.Add( new YoutubeSongInfo( ) );

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
				TwitterCommunicator.Instance.Publish(songs.First(e => e.Loaded).ToString(format));   
			}
		}
		public static void Run(Action callback)
		{
			Run();
			callback();
		}
	}
}
