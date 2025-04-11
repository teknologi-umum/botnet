using System.Net.Http;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Forex {
	public class Forex(
		IOptions<ForexOptions> options,
		HttpClient httpClient
	) {
		protected readonly string? ApiKey = options.Value.ApiKey;
		protected readonly HttpClient HttpClient = httpClient;
	}
}
