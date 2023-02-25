using System;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Pesto.Exception;
using BotNet.Services.Pesto.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Pesto; 

public class PestoClient {
	private static SemaphoreSlim? _semaphore;
	private readonly HttpClient _httpClient;
	private readonly PestoOptions _options;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public PestoClient(
		HttpClient httpClient,
		IOptions<PestoOptions> pestoOptionsAccessor
	) {
		_options = pestoOptionsAccessor.Value;
		_httpClient = httpClient;
		_semaphore ??= new SemaphoreSlim(_options.MaxConcurrentExecutions, _options.MaxConcurrentExecutions);
		_jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
	}

	/// <summary>
	/// Call the ping API. This is not rate limited.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>PingResponse record which shows some message</returns>
	/// <exception cref="PestoServerRateLimitedException">Too many request to the API. Client should try again in a few minutes</exception>
	/// <exception cref="PestoAPIException"></exception>
	public async Task<PingResponse> PingAsync(CancellationToken cancellationToken) {
		using HttpResponseMessage response = await _httpClient.GetAsync(
			requestUri: new Uri(new Uri(_options.BaseUrl), "api/ping"),
			cancellationToken: cancellationToken
		);

		if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new PestoServerRateLimitedException();
		
		response.EnsureSuccessStatusCode();
		PingResponse? pingResponse =
			await response.Content.ReadFromJsonAsync<PingResponse>(_jsonSerializerOptions, cancellationToken);

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
		using HttpResponseMessage response = await _httpClient.GetAsync(
			requestUri: new Uri(new Uri(_options.BaseUrl), "api/list-runtimes"),
			cancellationToken: cancellationToken
		);
		
		if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new PestoServerRateLimitedException();
		
		response.EnsureSuccessStatusCode();
		RuntimeResponse? runtimeResponse =
			await response.Content.ReadFromJsonAsync<RuntimeResponse>(_jsonSerializerOptions, cancellationToken);

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
		if (String.IsNullOrWhiteSpace(code)) throw new PestoEmptyCodeException();
		
		await _semaphore!.WaitAsync(cancellationToken);

		try {
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
				requestUri: new Uri(new Uri(_options.BaseUrl), "api/execute"),
				value: new CodeRequest(
					Language: language,
					Code: code,
					Version: "latest",
					CompileTimeout: _options.CompileTimeout,
					RunTimeout:_options.RunTimeout,
					MemoryLimit: _options.MemoryLimit
				),
				cancellationToken: cancellationToken
			);

			if (response.StatusCode != HttpStatusCode.OK) {
				ErrorResponse? errorResponse =
					await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonSerializerOptions, cancellationToken);

				throw response.StatusCode switch {
					HttpStatusCode.TooManyRequests when errorResponse?.Message == "Monthly limit exceeded" =>
						new PestoMonthlyLimitExceededException(),
					HttpStatusCode.TooManyRequests => new PestoServerRateLimitedException();
					HttpStatusCode.BadRequest when errorResponse?.Message == "Runtime not found" =>
						new PestoRuntimeNotFoundException(language.ToString()),
					_ => new PestoAPIException(errorResponse?.Message)
				};
			}

			response.EnsureSuccessStatusCode();
			CodeResponse? codeResponse =
				await response.Content.ReadFromJsonAsync<CodeResponse>(_jsonSerializerOptions, cancellationToken);

			return codeResponse ?? throw new PestoAPIException();
		} finally {
			_semaphore.Release();
		}
	}
}
