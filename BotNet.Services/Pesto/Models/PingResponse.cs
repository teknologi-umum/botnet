using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models;

public record PingResponse(
	[property: JsonPropertyName("message")] string Message
);
