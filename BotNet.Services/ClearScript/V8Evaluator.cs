using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.ClearScript.JsonConverters;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.Options;

namespace BotNet.Services.ClearScript {
	public class V8Evaluator(
		IOptions<V8Options> v8OptionsAccessor
	) {
		private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
			Converters = {
				new ScriptObjectConverter(),
				new DoubleConverter(),
				new BigIntegerConverter(),
				new UndefinedConverter()
			}
		};
		private readonly V8Options _v8Options = v8OptionsAccessor.Value;

		public async Task<string> EvaluateAsync(string script, CancellationToken cancellationToken) {
			using V8ScriptEngine engine = new();
			engine.MaxRuntimeHeapSize = _v8Options.HeapSize;
			engine.MaxRuntimeStackUsage = _v8Options.StackUsage;
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromSeconds(1));
			timeoutSource.Token.Register(engine.Interrupt);
			cancellationToken.Register(engine.Interrupt);
			return await Task.Run(() => {
				object? result = engine.Evaluate(script);
				return JsonSerializer.Serialize(result, JsonSerializerOptions);
			}, timeoutSource.Token);
		}
	}
}
