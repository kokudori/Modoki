using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Modoki.Models;
using Modoki.Properties;
using Modoki.Views;

namespace Modoki.ViewModels
{
	sealed class TimeLineViewModel : ViewModel
	{
		readonly Authorizer authorizer = new Authorizer(Resources.ConsumerKey, Resources.ConsumerSecret);

		Twitter twitter;

		public TimeLineViewModel()
		{
			tweetCommand = new ViewModelCommand(Tweet, () => !String.IsNullOrEmpty(Text));
		}

		public async void Initialize()
		{
			twitter = Authorizer.IsAuthorized ? authorizer.Load() : await Authorize();
			twitter.ReadTimeLine()
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(UpdateTweet);
			MySelf = await twitter.MySelf();
		}

		void UpdateTweet(IEnumerable<TweetContent> tweets)
		{
			foreach (var tweet in tweets.Reverse())
			{
				if (!Tweets.Any(x => x.Id == tweet.Id))
				{
					Tweets.Insert(0, tweet);
				}
			}
			Status = "ツイートを取得しました";
		}

		async Task<Twitter> Authorize()
		{
			Process.Start(await authorizer.AuthorizeUri());
			var config = new ConfigViewModel();
			var message = new TransitionMessage(typeof(ConfigWindow), config, TransitionMode.Modal);
			await Messenger.RaiseAsync(message);
			return await authorizer.Authorize(config.PinCode);
		}

		async void Tweet()
		{
			var id = await twitter.Tweet(Text);
			if (!String.IsNullOrEmpty(id))
			{
				Tweets.Insert(0, new TweetContent(id, Text, MySelf));
				MySelf = new User(MySelf.Name, MySelf.Image, MySelf.TweetCount + 1);
				Status = "ツイートを送信しました";
			}
			else
			{
				Status = "ツイートに失敗しました";
			}
			Text = "";
		}


		#region Text変更通知プロパティ
		private string text;

		public string Text
		{
			get
			{ return text; }
			set
			{
				if (text == value)
					return;
				text = value;
				RaisePropertyChanged();
				TweetCommand.RaiseCanExecuteChanged();
			}
		}
		#endregion


		#region Status変更通知プロパティ
		private string status;

		public string Status
		{
			get
			{ return status; }
			set
			{ 
				if (status == value)
					return;
				status = value;
				RaisePropertyChanged();
			}
		}
		#endregion


		#region MySelf変更通知プロパティ
		private User myself = new User("", null, 0);

		public User MySelf
		{
			get
			{ return myself; }
			set
			{
				if (myself == value)
					return;
				myself = value;
				RaisePropertyChanged();
			}
		}
		#endregion


		readonly ObservableCollection<TweetContent> tweets = new ObservableCollection<TweetContent>();
		public ObservableCollection<TweetContent> Tweets { get { return tweets; } }

		readonly ViewModelCommand tweetCommand;
		public ViewModelCommand TweetCommand { get { return tweetCommand; } }
	}
}
