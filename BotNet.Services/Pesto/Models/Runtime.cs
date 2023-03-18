using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models;

public record Runtime(
	[property: JsonPropertyName("language")] string Language,
	[property: JsonPropertyName("version")] string Version,
	[property: JsonPropertyName("aliases")] List<string> Aliases,
	[property: JsonPropertyName("compiled")] bool Compiled
);
