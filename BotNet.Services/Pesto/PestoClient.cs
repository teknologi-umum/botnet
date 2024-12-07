using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Pesto.Exceptions;
using BotNet.Services.Pesto.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Pesto;

public sealed class PestoClient : IDisposable {
	private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() }
	};

	private const int GracePeriod = 1_500_000;

	private static SemaphoreSlim? _semaphore;
	private bool _disposedValue;
	private readonly HttpClient _httpClient;
	private readonly HttpClientHandler _httpClientHandler;
	private readonly string _token;
	private readonly Uri _baseUrl;
	private readonly int _compileTimeout;
	private readonly int _runTimeout;
	private readonly TimeSpan _executeTimeout;
	private readonly ILogger<PestoClient> _logger;

	public PestoClient(
		IOptions<PestoOptions> pestoOptionsAccessor,
		ILogger<PestoClient> logger
	) {
		PestoOptions options = pestoOptionsAccessor.Value;
		if (string.IsNullOrWhiteSpace(options.Token)) throw new InvalidProgramException("PestoOptions:Token not configured.");
		if (string.IsNullOrWhiteSpace(options.BaseUrl)) throw new InvalidProgramException("PestoOptions:BaseUrl not configured.");

		_httpClientHandler = new HttpClientHandler {
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
		};
		_httpClient = new(
			handler: _httpClientHandler,
			disposeHandler: false
		);
		_logger = logger;
		_semaphore ??= new SemaphoreSlim(options.MaxConcurrentExecutions, options.MaxConcurrentExecutions);
		_token = options.Token;
		_baseUrl = new Uri(options.BaseUrl);
		_compileTimeout = options.CompileTimeout;
		_runTimeout = options.RunTimeout;
		_executeTimeout = TimeSpan.FromMilliseconds(_compileTimeout + _runTimeout + GracePeriod);
	}

	/// <summary>
	/// Call the ping API. This is not rate limited.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>PingResponse record which shows some message</returns>
	/// <exception cref="PestoServerRateLimitedException">Too many request to the API. Client should try again in a few minutes</exception>
	/// <exception cref="PestoApiException"></exception>
	public async Task<PingResponse> PingAsync(CancellationToken cancellationToken) {
		Uri requestUrl = new(_baseUrl, "api/ping");
		using HttpResponseMessage response = await _httpClient.GetAsync(
			requestUri: requestUrl,
			cancellationToken: cancellationToken
		);

		if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new PestoServerRateLimitedException();

		response.EnsureSuccessStatusCode();
		PingResponse? pingResponse =
			await response.Content.ReadFromJsonAsync<PingResponse>(JsonSerializerOptions, cancellationToken);

		return pingResponse ?? throw new PestoApiException();
	}

	/// <summary>
	/// List runtimes available on the Pesto's API. This shouldn't be needed, because we already have the Language
	/// class, and you can set the version as "latest" for every language.
	///
	/// This endpoint is not rate limited.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Array of runtimes</returns>
	/// <exception cref="PestoServerRateLimitedException">Too many request to the API. Client should try again in a few minutes</exception>
	/// <exception cref="PestoApiException"></exception>
	public async Task<RuntimeResponse> ListRuntimesAsync(CancellationToken cancellationToken) {
		Uri requestUrl = new(_baseUrl, "api/list-runtimes");
		using HttpRequestMessage request = new(HttpMethod.Get, requestUrl);
		request.Headers.Add("X-Pesto-Token", _token);
		request.Headers.Add("Accept", "application/json");

		using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

		if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new PestoServerRateLimitedException();

		response.EnsureSuccessStatusCode();
		RuntimeResponse? runtimeResponse =
			await response.Content.ReadFromJsonAsync<RuntimeResponse>(JsonSerializerOptions, cancellationToken);

		return runtimeResponse ?? throw new PestoApiException();
	}

	/// <summary>
	/// Execute code on Pesto.
	/// </summary>
	/// <param name="language"></param>
	/// <param name="code"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The code execution result</returns>
	/// <exception cref="PestoEmptyCodeException">Code parameter is empty</exception>
	/// <exception cref="PestoMonthlyLimitExceededException">Token has exceeded the allowed monthly limit. User should contact the Pesto team to increase their allowed limit.</exception>
	/// <exception cref="PestoRuntimeNotFoundException">THe runtime specified (language-version combination) is not allowed at Pesto's API</exception>
	/// <exception cref="PestoServerRateLimitedException">Too many request to the API. Client should try again in a few minutes</exception>
	/// <exception cref="PestoApiException"></exception>
	public async Task<CodeResponse> ExecuteAsync(Language language, string code, CancellationToken cancellationToken) {
		if (string.IsNullOrWhiteSpace(code)) throw new PestoEmptyCodeException();

		using CancellationTokenSource timeoutSource = new(_executeTimeout);
		using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
			token1: timeoutSource.Token,
			token2: cancellationToken
		);

		await _semaphore!.WaitAsync(linkedSource.Token);

		_logger.LogInformation($"Executing code on Pesto:\n{code}");

		try {
			Uri requestUrl = new(_baseUrl, "api/execute");
			using HttpRequestMessage request = new(HttpMethod.Post, requestUrl);
			request.Headers.Add("X-Pesto-Token", _token);
			request.Headers.Add("Accept", "application/json");
			request.Content = JsonContent.Create(
				inputValue: new CodeRequest(
					Language: language,
					Code: code,
					Version: "latest",
					CompileTimeout: _compileTimeout,
					RunTimeout: _runTimeout
				),
				options: JsonSerializerOptions
			);

			using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, linkedSource.Token).ConfigureAwait(false);

			if (response.StatusCode != HttpStatusCode.OK) {
				ErrorResponse? errorResponse =
					await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerOptions, linkedSource.Token);

				throw response.StatusCode switch {
					HttpStatusCode.TooManyRequests when errorResponse?.Message == "Monthly limit exceeded" =>
						new PestoMonthlyLimitExceededException(),
					HttpStatusCode.TooManyRequests => new PestoServerRateLimitedException(),
					HttpStatusCode.BadRequest when errorResponse?.Message == "Runtime not found" =>
						new PestoRuntimeNotFoundException(language.ToString()),
					_ => new PestoApiException(errorResponse?.Message)
				};
			}

			response.EnsureSuccessStatusCode();
			CodeResponse? codeResponse =
				await response.Content.ReadFromJsonAsync<CodeResponse>(JsonSerializerOptions, linkedSource.Token);

			return codeResponse ?? throw new PestoApiException();
		} finally {
			_semaphore.Release();
		}
	}

	private void Dispose(bool disposing) {
		if (!_disposedValue) {
			if (disposing) {
				// dispose managed state (managed objects)
				_httpClient.Dispose();
				_httpClientHandler.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
	}
}
