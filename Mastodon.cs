using System;
using Mastonet;

namespace CaptainRexEbooks
{
    static class Mastodon
    {
        public static void PostStatus(string status)
        {
            string instance = System.Environment.GetEnvironmentVariable("mastodonInstance");
            string accessToken = System.Environment.GetEnvironmentVariable("mastodonAccessToken");
            MastodonClient client = new MastodonClient(instance, accessToken);

            Console.WriteLine("Publishing toot: " + status);
            var _ = client.PublishStatus(status).Result;
        }
    }
}