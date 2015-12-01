using System.Threading.Tasks;
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
	public class TwitterCommunicator : IDecchiPublisher
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
			Consumer_Key = "MSBnclhghiW4yd9Hehddpg";
			Consumer_Secret = "L1sk8qcpxP2NRADf3wPQh6r2NE3rZvw5BCXyJCIaN6w";
		}

		public bool Login()
		{
			try
			{
				var accessToken = Globals.GetValue("AccessToken");
				var accessTokenSecret = Globals.GetValue("AccessTokenSecret");
				if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(accessTokenSecret))
				{
					var user = NewAuth();
					accessToken = user.AccessToken;
					accessTokenSecret = user.AccessTokenSecret;
					Globals.SetValue("AccessToken", accessToken);
					Globals.SetValue("AccessTokenSecret", accessTokenSecret);
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
		private ITwitterCredentials NewAuth()
		{
			var applicationCredentials = new ConsumerCredentials(Consumer_Key, Consumer_Secret);
			var url = CredentialsCreator.GetAuthorizationURL(applicationCredentials);

			var form = new InputCaptcha(url);
			form.ShowDialog();
			var captcha = form.Captcha;

			var newCredentials = CredentialsCreator.GetCredentialsFromVerifierCode(captcha, applicationCredentials);

			// 인증 실패
			if (newCredentials == null)
			{

			}

			return newCredentials;
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
				return me ?? (me = User.GetLoggedUser());
			}
		}

		/// <summary>
		/// 로그인된 유저 정보를 갱신하고, Me 객체를 반환합니다.
		/// </summary>
		/// <returns></returns>
		public Tweetinvi.Core.Interfaces.ILoggedUser RefrashMe()
		{
			me = User.GetLoggedUser();
			return me;
		}
	}
}
