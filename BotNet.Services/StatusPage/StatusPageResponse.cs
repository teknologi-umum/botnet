using System.Text.Json.Serialization;

namespace BotNet.Services.StatusPage {
	public sealed record StatusPageResponse {
		[JsonPropertyName("status")]
		public StatusInfo? Status { get; init; }
	}

	public sealed record StatusInfo {
		[JsonPropertyName("indicator")]
		public string? Indicator { get; init; }

		[JsonPropertyName("description")]
		public string? Description { get; init; }
	}

	public sealed record ServiceStatus {
		public string ServiceName { get; init; } = default!;
		public bool IsOperational { get; init; }
		public string? Description { get; init; }
	}
}
