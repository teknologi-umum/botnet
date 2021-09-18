using System;
using System.Diagnostics.CodeAnalysis;

namespace BotNet.Services.Giphy {
	[Obsolete("GiphyClient is deprecated. Use TenorClient instead.")]
	[ExcludeFromCodeCoverage]
	public class GiphyOptions {
		public string? ApiKey { get; set; }
		public int ReadsPerHour { get; set; } = 42;
		public int CallsPerDay { get; set; } = 1000;
	}
}
