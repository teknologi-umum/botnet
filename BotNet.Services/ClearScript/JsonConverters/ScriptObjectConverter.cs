using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.ClearScript;

namespace BotNet.Services.ClearScript.JsonConverters {
	public class ScriptObjectConverter : JsonConverter<ScriptObject> {
		public override bool CanConvert(Type typeToConvert) => typeof(ScriptObject).IsAssignableFrom(typeToConvert);

		public override ScriptObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

		public override void Write(Utf8JsonWriter writer, ScriptObject value, JsonSerializerOptions options) {
			if (value is IList) {
				writer.WriteStartArray();
				foreach (int index in value.PropertyIndices) {
					JsonSerializer.Serialize(writer, value[index], options);
				}
				writer.WriteEndArray();
			} else {
				writer.WriteStartObject();
				foreach (string propertyName in value.PropertyNames) {
					writer.WritePropertyName(propertyName);
					JsonSerializer.Serialize(writer, value[propertyName], options);
				}
				writer.WriteEndObject();
			}
		}
	}
}
