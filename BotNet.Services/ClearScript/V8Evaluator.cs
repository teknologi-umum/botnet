using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.ClearScript.JsonConverters;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.Options;

namespace BotNet.Services.ClearScript {
	public class V8Evaluator {
		private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
			Converters = {
				new ScriptObjectConverter(),
				new DoubleConverter(),
				new UndefinedConverter()
			}
		};
		private readonly V8Options _v8Options;

		public V8Evaluator(
			IOptions<V8Options> v8OptionsAccessor
		) {
			_v8Options = v8OptionsAccessor.Value;
		}

		public async Task<string> EvaluateAsync(string script, CancellationToken cancellationToken) {
			using V8ScriptEngine engine = new() {
				MaxRuntimeHeapSize = _v8Options.HeapSize,
				MaxRuntimeStackUsage = _v8Options.StackUsage
			};
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromSeconds(1));
			using CancellationTokenRegistration timeoutRegistration = timeoutSource.Token.Register(engine.Interrupt);
			using CancellationTokenRegistration cancellationRegistration = cancellationToken.Register(engine.Interrupt);
			return await Task.Run(() => {
				object? result = engine.Evaluate(script);
				return JsonSerializer.Serialize(result, JSON_SERIALIZER_OPTIONS);
			});
		}
	}
}
