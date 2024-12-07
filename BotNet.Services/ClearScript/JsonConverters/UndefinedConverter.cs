using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.ClearScript;

namespace BotNet.Services.ClearScript.JsonConverters {
	public class UndefinedConverter : JsonConverter<Undefined> {
		public override Undefined Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

		public override void Write(Utf8JsonWriter writer, Undefined value, JsonSerializerOptions options) {
			writer.WriteRawValue("undefined", skipInputValidation: true);
		}
	}
}
