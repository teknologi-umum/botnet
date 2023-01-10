using System.Net.Http;

namespace BotNet.Services.BMKG {
	public class BMKG {
		protected string uriTemplate = "https://data.bmkg.go.id/DataMKG/TEWS/{0}.json";
		protected readonly HttpClient httpClient;

		public BMKG(HttpClient client) {
			httpClient = client;
		}
	}
}
