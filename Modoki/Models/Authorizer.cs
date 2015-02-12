using System.Security.Cryptography;
using System.Threading.Tasks;

using AsyncOAuth;
using Livet;
using Modoki.Properties;

namespace Modoki.Models
{
	sealed class Authorizer : NotificationObject
	{
		readonly string requestUrl = "https://api.twitter.com/oauth/request_token";
		readonly string authorizeUrl = "https://api.twitter.com/oauth/authorize";
		readonly string accessUrl = "https://api.twitter.com/oauth/access_token";

		readonly string consumerKey;
		readonly string consumerSecret;

		readonly OAuthAuthorizer authorizer;
		RequestToken requestToken;

		public static bool IsAuthorized
		{
			get { return Setting.IsExist; }
		}

		static Authorizer()
		{
			OAuthUtility.ComputeHash = (key, buffer) =>
			{
				using (var hmac = new HMACSHA1(key))
				{
					return hmac.ComputeHash(buffer);
				}
			};
		}

		public Authorizer(string consumerKey, string consumerSecret)
		{
			this.consumerKey = consumerKey;
			this.consumerSecret = consumerSecret;
			authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);
		}

		public Twitter Load()
		{
			var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, Setting.Load());
			return new Twitter(client);
		}

		public async Task<string> AuthorizeUri()
		{
			requestToken = (await authorizer.GetRequestToken(requestUrl)).Token;
			return authorizer.BuildAuthorizeUrl(authorizeUrl, requestToken);
		}

		public async Task<Twitter> Authorize(string pinCode)
		{
			var result = await authorizer.GetAccessToken(accessUrl, requestToken, pinCode);
			var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, result.Token);
			Setting.Save(result.Token);
			return new Twitter(client);
		}
	}
}
