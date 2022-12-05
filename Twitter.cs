using System;
using System.IO;

namespace CaptainRexEbooks
{
    static class Twitter
    {
        public class CustomJsonLanguageConverter : Tweetinvi.Logic.JsonConverters.JsonLanguageConverter
        {
            public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return reader.Value != null
                    ? base.ReadJson(reader, objectType, existingValue, serializer)
                    : Tweetinvi.Models.Language.English;
            }
        }

        public static void InitializeCredentials()
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

            Tweetinvi.Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            // TODO: Remove
            Tweetinvi.Logic.JsonConverters.JsonPropertyConverterRepository.JsonConverters.Remove(typeof(Tweetinvi.Models.Language));
            Tweetinvi.Logic.JsonConverters.JsonPropertyConverterRepository.JsonConverters.Add(typeof(Tweetinvi.Models.Language), new CustomJsonLanguageConverter());

        }

        public static void PostStatus(string status)
        {
            Console.WriteLine("Publishing tweet: " + status);
            Tweetinvi.Tweet.PublishTweet(status);
        }
    }
}

