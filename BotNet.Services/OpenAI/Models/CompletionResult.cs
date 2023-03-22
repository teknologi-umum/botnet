using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.OpenAI.Models {
	public record CompletionResult(
		string Id,
		string Object,
		[property: JsonPropertyName("created")] int CreatedUnixTime,
		string? Model,
		List<Choice> Choices
	) {
		[JsonIgnore]
		public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedUnixTime).DateTime;
	}
}
