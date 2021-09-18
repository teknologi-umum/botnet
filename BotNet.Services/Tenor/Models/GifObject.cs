using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.Tenor.Models {
	public record GifObject(
		string Id,
		[property: JsonPropertyName("hasaudio")] bool HasAudio,
		Dictionary<MediaFormatType, MediaObject>[] Media
	);
}
