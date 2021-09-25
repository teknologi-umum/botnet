using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BotNet.Services.Json {
	public class UnixDateTimeConverter : JsonConverter<DateTime> {
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			if (reader.TryGetInt64(out long value)) {
				if (value > 253402300799) {
					return DateTimeOffset.FromUnixTimeMilliseconds(value).DateTime;
				} else {
					return DateTimeOffset.FromUnixTimeSeconds(value).DateTime;
				}
			} else {
				return default;
			}
		}
		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
			throw new NotImplementedException("This converter is only for deserializing purpose.");
	}
}
