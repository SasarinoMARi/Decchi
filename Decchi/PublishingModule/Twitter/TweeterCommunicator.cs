using Decchi.ParsingModule;
using Decchi.Utilities;
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
        public TwitterService Api { get { return m_api; } }

        public bool Login()
        {
            var token  = Globals.Instance.TwitterToken;
            var secret = Globals.Instance.TwitterSecret;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(secret))
            {
                this.m_api = new TwitterService(Consumer_Key, Consumer_Secret);
                return false;
            }
            else
            {
                this.m_api = new TwitterService(Consumer_Key, Consumer_Secret, token, secret);
                return true;
            }
        }

        /// <summary>
        /// 트윗을 게시합니다.
        /// </summary>
        /// <param name="songinfo">트윗할 SongInfo 객체</param>
        /// <returns>트윗 성공 여부</returns>
        public bool Publish(SongInfo songinfo)
        {
            if (songinfo == null || !songinfo.Loaded) return false;
            
            var text = songinfo.ToString();
            if (string.IsNullOrEmpty(text)) return false;

            string mediaId = null;

            var stream = ImageResize.LoadImageResized(songinfo.Cover);
            if (stream != null)
            {
                using (stream)
                {
                    try
                    {
                        mediaId = this.m_api.UploadMedia(new UploadMediaOptions { Media = new MediaFile { Content = stream } }).Media_Id;
                    }
                    catch
                    { }
                }
            }

            var option = new SendTweetOptions { Status = text };

            if (!string.IsNullOrEmpty(mediaId))
                option.MediaIds = new string[] { mediaId };

            var d = this.m_api.SendTweet(option);

            return d != null;
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
