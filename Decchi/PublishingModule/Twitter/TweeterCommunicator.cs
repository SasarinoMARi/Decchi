using System;
using Decchi.Core.Windows;
using TweetSharp;

namespace Decchi.PublishingModule.Twitter
{
	/// <summary>
	/// 트위터와 통신을 담당하는 TwitterCommunicator를 정의합니다.
	/// 이 클래스는 싱글턴 패턴으로 설계되었습니다. DecchiCore.Instance를 통해 인스턴스에 접근할 수 있습니다.
	/// IDecchiPublisher 인터페이스를 상속받은 퍼블리셔 클래스입니다.
	/// </summary>
	public partial class TwitterCommunicator : IDecchiPublisher
	{
		//const string Consumer_Key		= "";
		//const string Consumer_Secret	= "";
		
		private static TwitterCommunicator _instance = null;
		
		public static TwitterCommunicator Instance
		{
			get
			{
				return _instance ?? (_instance = new TwitterCommunicator());
			}
		}

		private TwitterService m_api;

		public bool Login()
		{
			try
			{
				this.m_api = null;

				var token  = Globals.GetValue("TwitterAccessToken");
				var secret = Globals.GetValue("TwitterAccessTokenSecret");

				if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(secret))
					NewAuth();
				else
					this.m_api = new TwitterService(Consumer_Key, Consumer_Secret, token, secret);

				return this.m_api != null;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 새로이 트위터 인증 절차를 진행합니다.
		/// </summary>
		/// <returns></returns>
		private void NewAuth()
		{
			this.m_api = new TwitterService(Consumer_Key, Consumer_Secret);

			var token = this.m_api.GetRequestToken();

			Globals.OpenWebSite(this.m_api.GetAuthorizationUri(token).ToString());

			var window = new InputCaptcha();
			window.Owner = MainWindow.Instance;

			MainWindow.Instance.Dispatcher.Invoke(new Action(() => window.ShowDialog()));

			var key = window.Password.Trim();

			var access = this.m_api.GetAccessToken(token, key);

			Globals.SetValue("TwitterAccessToken",			access.Token);
			Globals.SetValue("TwitterAccessTokenSecret",	access.TokenSecret);
			Globals.SaveSettings();

			this.m_api.AuthenticateWith(access.Token, access.TokenSecret);
		}

		/// <summary>
		/// 트윗을 게시합니다.
		/// </summary>
		/// <param name="text">트윗 본문</param>
		/// <returns>트윗 성공 여부</returns>
		public bool Publish(string text)
		{
			try
			{
				this.m_api.SendTweet(new SendTweetOptions { Status = text });
				return true;
			}
			catch
			{
				return false;
			}
		}

		private TwitterUser me;

		/// <summary>
		/// 로그인된 유저 객체를 얻어옵니다.
		/// 유저 정보 갱신이 필요할 경우 RefrashMe() 메서드를 호출합니다.
		/// </summary>
		public TwitterUser Me
		{
			get
			{
				// ?? 쓰려고 했는데 API 에러나면 뻗음 ㅡㅡ
				if (me == null)
				{
					try
					{
						me = this.m_api.VerifyCredentials(new VerifyCredentialsOptions { IncludeEntities = false, SkipStatus = true });
					}
					catch
					{ }
				}

				return me;
			}
		}

		/// <summary>
		/// 로그인된 유저 정보를 갱신하고, Me 객체를 반환합니다.
		/// </summary>
		/// <returns></returns>
		public TwitterUser RefrashMe()
		{
			me = null;
			return this.Me;
		}
	}
}
