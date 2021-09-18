using System;
using System.Diagnostics.CodeAnalysis;

namespace BotNet.Services.Giphy.Models {
	[Obsolete("GiphyClient is deprecated. Use TenorClient instead.")]
	[ExcludeFromCodeCoverage]
	public record Images(
		Mp4Image Preview,
		GifImage PreviewGif
	);
}
