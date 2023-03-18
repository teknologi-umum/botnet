using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models;

public record CodeResponse(
	[property: JsonPropertyName("language")] string Language,
	[property: JsonPropertyName("version")] string Version,
	[property: JsonPropertyName("compile")] CodeOutput CompileOutput,
	[property: JsonPropertyName("runtime")] CodeOutput RuntimeOutput
);
