using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models {
	public class LanguageTitleCaseConverter : JsonConverter<Language> {
		public override Language Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			return Enum.Parse<Language>(reader.GetString()!, ignoreCase: true);
		}

		public override void Write(Utf8JsonWriter writer, Language value, JsonSerializerOptions options) {
			writer.WriteStringValue(
				CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToString())
			);
		}
	}
}
