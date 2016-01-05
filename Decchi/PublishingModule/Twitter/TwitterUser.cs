using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Decchi.PublishingModule.Twitter
{
    [DataContract]
    public class TwitterUser
    {
        public static TwitterUser Parse(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(TwitterUser));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var a = (TwitterUser)serializer.ReadObject(stream);

                return a;
            }
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "screen_name")]
        public string ScreenName { get; set; }

        [DataMember(Name = "profile_image_url")]
        public string ProfileImageUrl { get; set; }
    }
}
