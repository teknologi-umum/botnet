using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Json;
using BotNet.Services.Piston.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Piston {
	public class PistonClient {
		private static ImmutableList<RuntimeResult>? _runtimes;
		private static SemaphoreSlim? _semaphore;

		private readonly HttpClient _httpClient;
		private readonly PistonOptions _pistonOptions;
		private readonly string _runtimesUrl;
		private readonly string _executeUrl;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public PistonClient(
			HttpClient httpClient,
			IOptions<PistonOptions> pistonOptionsAccessor
		) {
			_pistonOptions = pistonOptionsAccessor.Value;
			_runtimesUrl = _pistonOptions.RuntimesUrl;
			_executeUrl = _pistonOptions.ExecuteUrl;
			_semaphore ??= new SemaphoreSlim(_pistonOptions.MaxConcurrentExecutions, _pistonOptions.MaxConcurrentExecutions);
			_httpClient = httpClient;
			_jsonSerializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = new SnakeCaseNamingPolicy()
			};
		}

		private async Task<RuntimeResult?> GetRuntimeAsync(string language, CancellationToken cancellationToken) {
			if (_runtimes is null) {
				_runtimes = await _httpClient.GetFromJsonAsync<ImmutableList<RuntimeResult>>(_runtimesUrl, cancellationToken);
			}
			return _runtimes!
				.Where(runtime => runtime.Language == language)
				.OrderByDescending(runtime => runtime.Version)
				.FirstOrDefault();
		}

		public async Task<ExecuteResult> ExecuteAsync(string language, string code, CancellationToken cancellationToken) {
			await _semaphore!.WaitAsync(cancellationToken);
			try {
				RuntimeResult runtime = await GetRuntimeAsync(language, cancellationToken) ?? throw new KeyNotFoundException($"Runtime for {language} not found.");
				using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
					requestUri: _executeUrl,
					value: new ExecutePayload(
						Language: language,
						Version: runtime.Version,
						Files: ImmutableList.Create(
							new FilePayload(
								Content: code
							)
						),
						CompileTimeout: _pistonOptions.CompileTimeout,
						RunTimeout: _pistonOptions.RunTimeout,
						CompileMemoryLimit: _pistonOptions.CompileMemoryLimit,
						RunMemoryLimit: _pistonOptions.RunMemoryLimit
					),
					cancellationToken: cancellationToken
				);
				if (response.StatusCode == HttpStatusCode.BadRequest) {
					ErrorMessageResult? errorMessage = await response.Content.ReadFromJsonAsync<ErrorMessageResult>(_jsonSerializerOptions, cancellationToken);
#pragma warning disable CS0618 // Type or member is obsolete
					throw new ExecutionEngineException(errorMessage?.Message);
#pragma warning restore CS0618 // Type or member is obsolete
				}
				response.EnsureSuccessStatusCode();
#pragma warning disable CS0618 // Type or member is obsolete
				return await response.Content.ReadFromJsonAsync<ExecuteResult>(_jsonSerializerOptions, cancellationToken) ?? throw new ExecutionEngineException();
#pragma warning restore CS0618 // Type or member is obsolete
			} finally {
				_semaphore!.Release();
			}
		}
	}
}
