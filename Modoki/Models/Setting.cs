using AsyncOAuth;
using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modoki.Models
{
	static class Setting
	{
		static readonly string path = "setting.json";

		public static bool IsExist
		{
			get { return File.Exists(path); }
		}

		public static void Save(AccessToken token)
		{
			File.WriteAllText(path, DynamicJson.Serialize(token));
		}

		public static AccessToken Load()
		{
			var json = DynamicJson.Parse(File.ReadAllText(path));
			return new AccessToken(json.Key, json.Secret);
		}
	}
}
