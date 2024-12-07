using System.Net.Http;

namespace BotNet.Services.BMKG {
	public class Bmkg {
		protected const string UriTemplate = "https://data.bmkg.go.id/DataMKG/TEWS/{0}.json";
		protected readonly HttpClient HttpClient;

		protected Bmkg(HttpClient client) {
			HttpClient = client;
		}
	}
}
