using System;
using System.IO;

namespace CaptainRexEbooks
{
    static class Twitter
    {
        public static void PostStatus(string status)
        {
            string consumerKey = System.Environment.GetEnvironmentVariable("twitterConsumerKey");
            string consumerSecret = System.Environment.GetEnvironmentVariable("twitterConsumerSecret");
            string accessToken = System.Environment.GetEnvironmentVariable("twitterAccessToken");
            string accessTokenSecret = System.Environment.GetEnvironmentVariable("twitterAccessTokenSecret");

            if (consumerKey == null)
            {
                using (StreamReader fs = File.OpenText("localconfig/twitterKeys.txt"))
                {
                    consumerKey = fs.ReadLine();
                    consumerSecret = fs.ReadLine();
                    accessToken = fs.ReadLine();
                    accessTokenSecret = fs.ReadLine();
                }
            }

            var userClient = new Tweetinvi.TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            Console.WriteLine("Publishing tweet: " + status);
            var _ = userClient.Tweets.PublishTweetAsync(status).Result;
        }
    }
}

