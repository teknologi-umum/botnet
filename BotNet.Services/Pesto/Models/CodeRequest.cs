using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models; 

public sealed record CodeRequest(
	[property: JsonPropertyName("language"), JsonConverter(typeof(LanguageTitleCaseConverter))] Language Language,
	[property: JsonPropertyName("code")] string Code,
	[property: JsonPropertyName("compileTimeout")] int CompileTimeout = 10_000,
	[property: JsonPropertyName("runTimeout")] int RunTimeout = 10_000,
	[property: JsonPropertyName("version")] string Version = "latest"
);
