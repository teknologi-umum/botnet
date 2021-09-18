using System;
using System.Diagnostics.CodeAnalysis;

namespace BotNet.Services.Giphy.Models {
	/// <summary>
	/// https://developers.giphy.com/docs/api/endpoint#search
	/// </summary>
	[Obsolete("GiphyClient is deprecated. Use TenorClient instead.")]
	[ExcludeFromCodeCoverage]
	public record GifSearchResult(
		GifObject[] Data
	);
}
