using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.OpenAI.Models {
	public record ChatMessage(
		string Role,
		List<ChatContent> Content
	) {
		public static ChatMessage FromText(string role, string text) => new(
			Role: role,
			Content: [
				new ChatContent(
					Type: "text",
					Text: text,
					ImageUrl: null
				)
			]
		);

		public static ChatMessage FromTextWithImageBase64(string role, string text, string imageBase64) => new(
			Role: role,
			Content: [
				new ChatContent(
					Type: "text",
					Text: text,
					ImageUrl: null
				),
				new ChatContent(
					Type: "image_url",
					Text: null,
					ImageUrl: new(
						Url: $"data:image/png;base64,{imageBase64}"
					)
				)
			]
		);

		public static ChatMessage FromImageBase64(string role, string imageBase64) => new(
			Role: role,
			Content: [
				new ChatContent(
					Type: "image_url",
					Text: null,
					ImageUrl: new(
						Url: $"data:image/png;base64,{imageBase64}"
					)
				)
			]
		);
	}

	public record ChatContent(
		string Type,
		[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text,
		[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ImageUrl? ImageUrl
	);

	public record ImageUrl(
		string Url
	);
}
