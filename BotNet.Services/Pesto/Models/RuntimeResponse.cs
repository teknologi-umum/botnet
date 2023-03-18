using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models;

public record RuntimeResponse(
	[property: JsonPropertyName("runtime")] List<Runtime> Runtimes
);
