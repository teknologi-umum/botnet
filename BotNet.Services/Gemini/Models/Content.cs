using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public record Content(
		[property: JsonPropertyName("role"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Role,
		[property: JsonPropertyName("parts")] List<Part>? Parts
	) {
		public static Content FromText(string role, string text) => new(
			Role: role,
			Parts: [
				new(Text: text)
			]
		);
	}
}
