using System;
using System.Threading.Tasks;
using Mastonet;

namespace CaptainRexEbooks
{
    static class Mastodon
    {
        public static Task PostStatus(string status)
        {
            string instance = System.Environment.GetEnvironmentVariable("mastodonInstance");
            string accessToken = System.Environment.GetEnvironmentVariable("mastodonAccessToken");
            MastodonClient client = new MastodonClient(instance, accessToken);

            string statusWithHashtags = status + "\n#StarWars #CloneWars #Rebels";
            Console.WriteLine("Publishing toot: " + statusWithHashtags);
            return client.PublishStatus(statusWithHashtags);
        }
    }
}