using Decchi;
using Tweetinvi;
using Tweetinvi.Core.Credentials;

namespace PublishingModule.Twitter
{
	/// <summary>
	/// 트위터와 통신을 담당하는 TwitterCommunicator를 정의합니다.
	/// 이 클래스는 싱글턴 패턴으로 설계되었습니다. DecchiCore.Instance를 통해 인스턴스에 접근할 수 있습니다.
	/// IDecchiPublisher 인터페이스를 상속받은 퍼블리셔 클래스입니다.
	/// </summary>
	public partial class TwitterCommunicator : IDecchiPublisher
	{
		private static TwitterCommunicator _instance = null;
		string Consumer_Key { get; set; }
		string Consumer_Secret { get; set; }


		public static TwitterCommunicator Instance
		{
			get
			{
				return _instance ?? (_instance = new TwitterCommunicator());
			}
		}

		private TwitterCommunicator()
		{
			// 이 곳에 컨슈머 정보 입력.
			SetConsumer( );
		}

		public bool Login()
		{
			try
			{
				var accessToken = Globals.GetValue("TwitterAccessToken");
				var accessTokenSecret = Globals.GetValue("TwitterAccessTokenSecret");
				if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(accessTokenSecret))
				{
					var user = NewAuth();
					if(user != null)
					{
						accessToken = user.AccessToken;
						accessTokenSecret = user.AccessTokenSecret;
						Globals.SetValue("TwitterAccessToken", accessToken);
						Globals.SetValue("TwitterAccessTokenSecret", accessTokenSecret);
					}
					else
					{
						//트위터 인증 실패
						return false;
					}
				}
				Auth.SetUserCredentials(Consumer_Key, Consumer_Secret, accessToken, accessTokenSecret);
				return true;
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
		/// 
		private ITwitterCredentials NewAuth()
		{
			var form = new InputCaptcha(Consumer_Key, Consumer_Secret);
			form.ShowDialog();
			return form.Credentials;
		}

		/// <summary>
		/// 트윗을 게시합니다.
		/// </summary>
		/// <param name="text">트윗 본문</param>
		/// <returns>트윗 성공 여부</returns>
		public bool Publish(string text)
		{
			return !string.IsNullOrEmpty(text) && Tweet.PublishTweet(text) != null;
		}

		private Tweetinvi.Core.Interfaces.ILoggedUser me;

		/// <summary>
		/// 로그인된 유저 객체를 얻어옵니다.
		/// 유저 정보 갱신이 필요할 경우 RefrashMe() 메서드를 호출합니다.
		/// </summary>
		public Tweetinvi.Core.Interfaces.ILoggedUser Me
		{
			get
			{
				// ?? 쓰려고 했는데 API 에러나면 뻗음 ㅡㅡ
				if (me == null)
				{
					try
					{
						me = User.GetLoggedUser();
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
		public Tweetinvi.Core.Interfaces.ILoggedUser RefrashMe()
		{
			return User.GetLoggedUser();
		}
	}
}
