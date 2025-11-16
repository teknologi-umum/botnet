using System.Text.Json.Serialization;

namespace BotNet.Services.OMDb {
	public sealed record OmdbResponse {
		[JsonPropertyName("Title")]
		public string? Title { get; init; }

		[JsonPropertyName("Year")]
		public string? Year { get; init; }

		[JsonPropertyName("Rated")]
		public string? Rated { get; init; }

		[JsonPropertyName("Released")]
		public string? Released { get; init; }

		[JsonPropertyName("Runtime")]
		public string? Runtime { get; init; }

		[JsonPropertyName("Genre")]
		public string? Genre { get; init; }

		[JsonPropertyName("Director")]
		public string? Director { get; init; }

		[JsonPropertyName("Writer")]
		public string? Writer { get; init; }

		[JsonPropertyName("Actors")]
		public string? Actors { get; init; }

		[JsonPropertyName("Plot")]
		public string? Plot { get; init; }

		[JsonPropertyName("Language")]
		public string? Language { get; init; }

		[JsonPropertyName("Country")]
		public string? Country { get; init; }

		[JsonPropertyName("Awards")]
		public string? Awards { get; init; }

		[JsonPropertyName("Poster")]
		public string? Poster { get; init; }

		[JsonPropertyName("Ratings")]
		public OmdbRating[]? Ratings { get; init; }

		[JsonPropertyName("imdbRating")]
		public string? ImdbRating { get; init; }

		[JsonPropertyName("imdbVotes")]
		public string? ImdbVotes { get; init; }

		[JsonPropertyName("imdbID")]
		public string? ImdbId { get; init; }

		[JsonPropertyName("Type")]
		public string? Type { get; init; }

		[JsonPropertyName("totalSeasons")]
		public string? TotalSeasons { get; init; }

		[JsonPropertyName("Response")]
		public string? Response { get; init; }

		[JsonPropertyName("Error")]
		public string? Error { get; init; }
	}

	public sealed record OmdbRating {
		[JsonPropertyName("Source")]
		public string? Source { get; init; }

		[JsonPropertyName("Value")]
		public string? Value { get; init; }
	}
}
