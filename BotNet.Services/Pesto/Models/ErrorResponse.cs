using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models;

public record ErrorResponse(
	[property: JsonPropertyName("message")] string Message
);
