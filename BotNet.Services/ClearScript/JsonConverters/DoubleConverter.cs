using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BotNet.Services.ClearScript.JsonConverters {
	public class DoubleConverter : JsonConverter<double> {
		public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

		public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) {
			switch (value) {
				case double.PositiveInfinity:
					writer.WriteRawValue("Infinity", skipInputValidation: true);
					break;
				case double.NegativeInfinity:
					writer.WriteRawValue("-Infinity", skipInputValidation: true);
					break;
				case double.NaN:
					writer.WriteRawValue("NaN", skipInputValidation: true);
					break;
				default:
					writer.WriteNumberValue(value);
					break;
			}
		}
	}
}
