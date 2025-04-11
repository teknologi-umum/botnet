using System;
using System.Web;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// ForexRates provide:
// Currency conversion: latest and historical.
// latest gives latest rates(hourly).
// historical gives historical rates with date provided.
// from the command will be passed `from` -> e.g. <CODE> <AMOUNT>
// and `to` -> e.g. <CODE>
// returning <CODE> <AMOUNT>

namespace BotNet.Services.Forex {
    public class ForexRates(
        IOptions<ForexOptions> options,
        HttpClient httpClient
    ) : Forex(options, httpClient) {
        private const string ConvertEndpoint = "https://api.mfirhas.com/pfm/forex/convert";

        // call ConvertEndpoint, e.g
        // curl --location 'https://api.mfirhas.com/pfm/forex/convert?from=IDR%2080%2C000%2C000%2C000%2C000&to=USD' \
        // --header 'x-api-key: my_api_key'
        // or with date
        // curl --location 'https://api.mfirhas.com/pfm/forex/convert?from=IDR%2080%2C000%2C000%2C000%2C000&to=USE&date=2000-01-01' \
        // --header 'x-api-key: my_api_key'
        public async Task<string> Convert(string from, string to, string? date = null, CancellationToken cancellationToken = default) {
            var builder = new UriBuilder(ConvertEndpoint);
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["from"] = from;
            query["to"] = to;
            if (!string.IsNullOrWhiteSpace(date)) {
                query["date"] = date;
            }

            builder.Query = query.ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

            if (string.IsNullOrWhiteSpace(ApiKey)) {
                throw new InvalidOperationException("API key is not configured.");
            }

            request.Headers.Add("x-api-key", ApiKey);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Detect error response
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("error", out var errorElement)) {
                var message = errorElement.GetString() ?? "Unknown error occurred.";
                throw new Exception($"API error: {message}");
            }

            var result = JsonSerializer.Deserialize<ForexConvertResponse>(content);
            if (result?.Data?.Result is not null && result.Data.Result.Count > 0) {
                var kv = result.Data.Result.First();
                return $"{kv.Key} {kv.Value}";
            }

            throw new Exception("Conversion result is empty.");
        }
    }

    public class ForexConvertResponse {
        public ForexData? Data { get; set; }
    }

    public class ForexData {
        public DateTime Date { get; set; }
        public Dictionary<string, string>? From { get; set; }
        public Dictionary<string, string>? Result { get; set; }
    }
}