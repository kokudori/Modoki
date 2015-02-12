using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Codeplex.Data;

namespace Modoki.Models
{
	sealed class User
	{
		readonly string name;
		readonly Uri image;
		readonly int tweetCount;

		public string Name { get { return name; } }
		public Uri Image { get { return image; } }
		public int TweetCount { get { return tweetCount; } }

		public User(string name, Uri image, int tweetCount)
		{
			this.name = name;
			this.image = image;
			this.tweetCount = tweetCount;
		}
	}

	sealed class TweetContent
	{
		readonly string id;
		readonly string text;
		readonly User user;

		public string Id { get { return id; } }
		public string Text { get { return text; } }
		public User User { get { return user; } }

		public TweetContent(string id, string text, User user)
		{
			this.id = id;
			this.text = text;
			this.user = user;
		}
	}

	sealed class Twitter
	{
		readonly string myselfUrl = "https://api.twitter.com/1.1/account/verify_credentials.json";
		readonly string tweetUrl = "https://api.twitter.com/1.1/statuses/update.json";
		readonly string timeLineUrl = "https://api.twitter.com/1.1/statuses/home_timeline.json";

		readonly HttpClient client;

		public Twitter(HttpClient client)
		{
			this.client = client;
		}

		public async Task<User> MySelf()
		{
			var json = DynamicJson.Parse(await client.GetStringAsync(myselfUrl));
			return new User(json.name, new Uri(json.profile_image_url), (int)json.statuses_count);
		}

		public async Task<string> Tweet(string text)
		{
			var parameters = new[] { new KeyValuePair<string, string>("status", text) };
			var content = new FormUrlEncodedContent(parameters);
			var response = await client.PostAsync(tweetUrl, content);
			if (response.StatusCode != HttpStatusCode.OK)
				return null;
			var json = await response.Content.ReadAsStringAsync();
			return DynamicJson.Parse(json).id_str;
		}

		IEnumerable<TweetContent> ParseTweets(string json)
		{
			foreach (var tweet in DynamicJson.Parse(json))
			{
				yield return new TweetContent(
					tweet.id_str, tweet.text,
					new User(
						tweet.user.name,
						new Uri(tweet.user.profile_image_url),
						(int)tweet.user.statuses_count
					)
				);
			}
		}

		public IObservable<IEnumerable<TweetContent>> ReadTimeLine()
		{
			return Observable
				.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(90))
				.SelectMany(_ => client.GetStringAsync(timeLineUrl).ToObservable())
				.Select(ParseTweets);
		}
	}
}
