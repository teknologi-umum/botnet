using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using BotNet.Services.Json;

namespace BotNet.Services.StackExchange.Models {
	public record StackExchangeQuestionSnippet(
		ImmutableHashSet<string> Tags,
		StackExchangeUser Owner,
		bool IsAnswered,
		long ViewCount,
		[property: JsonPropertyName("closed_date"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime? ClosedDateUtc,
		[property: JsonPropertyName("protected_date"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime? ProtectedDateUtc,
		long? AcceptedAnswerId,
		long Score,
		[property: JsonPropertyName("last_activity_date"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime LastActivityDateUtc,
		[property: JsonPropertyName("creation_date"), JsonConverter(typeof(UnixDateTimeConverter))] DateTime CreationDateUtc,
		long QuestionId,
		string ContentLicense,
		string Link,
		string? ClosedReason,
		string Title
	);
}
