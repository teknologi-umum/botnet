using System.Collections.Immutable;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BotNet.Services.Antutu.Models;

namespace BotNet.Services.Antutu {
	public class AntutuScraper(HttpClient httpClient) {
		private readonly HttpClient _httpClient = httpClient;

		public async Task<ImmutableList<AntutuBenchmarkData>> GetAndroidRankingAsync(CancellationToken cancellationToken) {
			const string url = "https://www.antutu.com/en/ranking/rank1.htm";
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlCollection<IElement> rows = document.QuerySelectorAll(".newrank-b");

			ImmutableList<AntutuBenchmarkData>.Builder builder = ImmutableList.CreateBuilder<AntutuBenchmarkData>();
			foreach (IElement row in rows) {
				string device = row.QuerySelector(".newrank-b-name")?.TextContent?.Trim() ?? "Unknown";
				int cpu = int.Parse(row.QuerySelector(".newrank-b-cpu")?.TextContent?.Trim() ?? "0");
				int gpu = int.Parse(row.QuerySelector(".newrank-b-gpu")?.TextContent?.Trim() ?? "0");
				int mem = int.Parse(row.QuerySelector(".newrank-b-mem")?.TextContent?.Trim() ?? "0");
				int ux = int.Parse(row.QuerySelector(".newrank-b-ux")?.TextContent?.Trim() ?? "0");
				int total = int.Parse(row.QuerySelector(".newrank-b-total")?.TextContent?.Trim() ?? "0");
				builder.Add(new AntutuBenchmarkData(device, cpu, gpu, mem, ux, total));
			}

			return builder.ToImmutable();
		}
	}
}
