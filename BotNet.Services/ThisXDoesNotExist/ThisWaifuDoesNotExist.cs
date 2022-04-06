using System;

namespace BotNet.Services.ThisXDoesNotExist {
	public static class ThisWaifuDoesNotExist {
		public static Uri GetRandomUrl() {
			return new Uri($"https://www.thiswaifudoesnotexist.net/example-{new Random().Next(100001)}.jpg");
		}
	}
}
