using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Pemilu2024 {
	public sealed class SirekapClient(
		HttpClient httpClient
	) {
		public async Task<IDictionary<string, Paslon>> GetPaslonByKodeAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<IDictionary<string, Paslon>>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/pemilu/ppwp.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<IDictionary<string, IDictionary<string, Caleg>>> GetCalegByKodeByKodePartaiAsync(string kodeDapil, CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<IDictionary<string, IDictionary<string, Caleg>>>(
				requestUri: $"https://sirekap-obj-data.kpu.go.id/pemilu/caleg/partai/{kodeDapil}.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<IDictionary<string, Partai>> GetPartaiByKodeAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<IDictionary<string, Partai>>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/pemilu/partai.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<IList<Wilayah>> GetPronvisiListAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<IList<Wilayah>>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/wilayah/pemilu/ppwp/0.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<IList<Wilayah>> GetDapilDprListAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<IList<Wilayah>>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/wilayah/pemilu/pdpr/dapil_dpr.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<IList<Wilayah>> GetSubWilayahListAsync(string kodeWilayah, CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<IList<Wilayah>>(
				requestUri: $"https://sirekap-obj-data.kpu.go.id/wilayah/pemilu/ppwp/{kodeWilayah}.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<ReportPilpres> GetReportPilpresAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<ReportPilpres>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/pemilu/hhcw/ppwp.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<ReportPilpres> GetReportPilpresByWilayahAsync(string kodeWilayah, CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<ReportPilpres>(
				requestUri: $"https://sirekap-obj-data.kpu.go.id/pemilu/hhcw/ppwp/{kodeWilayah}.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<ReportPilegDprByWilayah> GetReportPilegDprByProvinsiAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<ReportPilegDprByWilayah>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/pemilu/hhcw/pdpr.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<ReportPilegDprByDapil> GetReportPilegDprByDapilAsync(CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<ReportPilegDprByDapil>(
				requestUri: "https://sirekap-obj-data.kpu.go.id/pemilu/hhcd/pdpr/0.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}

		public async Task<ReportCalegDpr> GetReportCalegDprAsync(string kodeDapil, CancellationToken cancellationToken) {
			return await httpClient.GetFromJsonAsync<ReportCalegDpr>(
				requestUri: $"https://sirekap-obj-data.kpu.go.id/pemilu/hhcd/pdpr/{kodeDapil}.json",
				cancellationToken: cancellationToken
			) ?? throw new JsonException("Unexpected response");
		}
	}
}
