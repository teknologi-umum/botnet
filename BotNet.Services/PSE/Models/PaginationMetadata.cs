using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.Models {
	// While the request uses 0-based index for page, currentPage here is 1-based index
	// from, to, lastPage are also 1-based index
	public record PaginationMetadata(
		[property: JsonPropertyName("currentPage")] int CurrentPage,
		[property: JsonPropertyName("from")] int StartingItemNumber,
		[property: JsonPropertyName("to")] int EndingItemNumber,
		[property: JsonPropertyName("lastPage")] int LastPage,
		[property: JsonPropertyName("perPage")] int ItemsPerPage,
		[property: JsonPropertyName("total")] int TotalItems
	);
}
