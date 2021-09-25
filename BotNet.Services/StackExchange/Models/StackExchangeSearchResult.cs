using System.Collections.Immutable;

namespace BotNet.Services.StackExchange.Models {
	public record StackExchangeSearchResult(
		ImmutableList<StackExchangeQuestionSnippet> Items,
		bool HasMore,
		int QuotaMax,
		int QuotaRemaining
	);
}
