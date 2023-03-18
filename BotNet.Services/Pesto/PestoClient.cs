using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Pesto.Exceptions;
using BotNet.Services.Pesto.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Pesto;

public class PestoClient {
	private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() }
	};

	private static SemaphoreSlim? _semaphore;
	private readonly HttpClient _httpClient;
	private readonly string _token;
	private readonly Uri _baseUrl;
	private readonly int _compileTimeout;
	private readonly int _runTimeout;
	private readonly int _memoryLimit;
	private readonly ILogger<PestoClient> _logger;

	public PestoClient(
		HttpClient httpClient,
		IOptions<PestoOptions> pestoOptionsAccessor,
		ILogger<PestoClient> logger
	) {
		PestoOptions options = pestoOptionsAccessor.Value;
		if (string.IsNullOrWhiteSpace(options.Token)) throw new InvalidProgramException("PestoOptions:Token not configured.");
		if (string.IsNullOrWhiteSpace(options.BaseUrl)) throw new InvalidProgramException("PestoOptions:BaseUrl not configured.");

		_httpClient = httpClient;
		_logger = logger;
		_semaphore ??= new SemaphoreSlim(options.MaxConcurrentExecutions, options.MaxConcurrentExecutions);
		_token = options.Token;
		_baseUrl = new Uri(options.BaseUrl);
		_compileTimeout = options.CompileTimeout;
		_runTimeout = options.RunTimeout;
		_memoryLimit = options.MemoryLimit;
	}

	/// <summary>
	/// Call the ping API. This is not rate limited.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>PingResponse record which shows some message</returns>
	/// <exception cref="PestoServerRateLimitedException">Too many request to the API. Client should try again in a few minutes</exception>
	/// <exception cref="PestoAPIException"></exception>
	public async Task<PingResponse> PingAsync(CancellationToken cancellationToken) {
		Uri requestUrl = new(_baseUrl, "api/ping");
		using HttpResponseMessage response = await _httpClient.GetAsync(
			requestUri: requestUrl,
			cancellationToken: cancellationToken
		);

		if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new PestoServerRateLimitedException();

		response.EnsureSuccessStatusCode();
		PingResponse? pingResponse =
			await response.Content.ReadFromJsonAsync<PingResponse>(JSON_SERIALIZER_OPTIONS, cancellationToken);

		return pingResponse ?? throw new PestoAPIException();
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
	/// <exception cref="PestoAPIException"></exception>
	public async Task<RuntimeResponse> ListRuntimesAsync(CancellationToken cancellationToken) {
		Uri requestUrl = new(_baseUrl, "api/list-runtimes");
		using HttpRequestMessage request = new(HttpMethod.Get, requestUrl);
		request.Headers.Add("X-Pesto-Token", _token);
		request.Headers.Add("Accept", "application/json");

		using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

		if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new PestoServerRateLimitedException();

		response.EnsureSuccessStatusCode();
		RuntimeResponse? runtimeResponse =
			await response.Content.ReadFromJsonAsync<RuntimeResponse>(JSON_SERIALIZER_OPTIONS, cancellationToken);

		return runtimeResponse ?? throw new PestoAPIException();
	}

	/// <summary>
	/// Execute code on Pesto.
	/// </summary>
	/// <param name="language"></param>
	/// <param name="code"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The code execution result</returns>
	/// <exception cref="PestoEmptyCodeException">Code parameter is empty</exception>
	/// <exception cref="PestoMonthlyLimitExceededException">Token has exceed the allowed monthly limit. User should contact the Pesto team to increase their allowed limit.</exception>
	/// <exception cref="PestoRuntimeNotFoundException">THe runtime specified (language-version combination) is not allowed at Pesto's API</exception>
	/// <exception cref="PestoServerRateLimitedException">Too many request to the API. Client should try again in a few minutes</exception>
	/// <exception cref="PestoAPIException"></exception>
	public async Task<CodeResponse> ExecuteAsync(Language language, string code, CancellationToken cancellationToken) {
		if (string.IsNullOrWhiteSpace(code)) throw new PestoEmptyCodeException();

		await _semaphore!.WaitAsync(cancellationToken);

		_logger.LogInformation($"Executing code on Pesto:\n{code}");

		try {
			Uri requestUrl = new(_baseUrl, "api/execute");
			using HttpRequestMessage request = new(HttpMethod.Post, requestUrl);
			request.Headers.Add("X-Pesto-Token", _token);
			request.Headers.Add("Accept", "application/json");
			string content = JsonSerializer.Serialize(
				value: new CodeRequest(
					Language: language,
					Code: code,
					Version: "latest",
					CompileTimeout: _compileTimeout,
					RunTimeout: _runTimeout,
					MemoryLimit: _memoryLimit
				),
				options: JSON_SERIALIZER_OPTIONS
			);
			request.Content = new StringContent(
				content: content,
				encoding: Encoding.UTF8,
				mediaType: "application/json"
			);

			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

			if (response.StatusCode != HttpStatusCode.OK) {
				ErrorResponse? errorResponse =
					await response.Content.ReadFromJsonAsync<ErrorResponse>(JSON_SERIALIZER_OPTIONS, cancellationToken);

				throw response.StatusCode switch {
					HttpStatusCode.TooManyRequests when errorResponse?.Message == "Monthly limit exceeded" =>
						new PestoMonthlyLimitExceededException(),
					HttpStatusCode.TooManyRequests => new PestoServerRateLimitedException(),
					HttpStatusCode.BadRequest when errorResponse?.Message == "Runtime not found" =>
						new PestoRuntimeNotFoundException(language.ToString()),
					_ => new PestoAPIException(errorResponse?.Message)
				};
			}

			response.EnsureSuccessStatusCode();
			CodeResponse? codeResponse =
				await response.Content.ReadFromJsonAsync<CodeResponse>(JSON_SERIALIZER_OPTIONS, cancellationToken);

			return codeResponse ?? throw new PestoAPIException();
		} finally {
			_semaphore.Release();
		}
	}
}
