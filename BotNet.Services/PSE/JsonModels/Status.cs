using System;

namespace BotNet.Services.PSE.JsonModels {
	public enum Status {
		Registered,
		Suspended,
		Revoked
	}

	public static class StatusConverter {
		public static string ToPSEStatus(this Status status) => status switch {
			Status.Registered => "TERDAFTAR",
			Status.Suspended => "DIHENTIKAN_SEMENTARA",
			Status.Revoked => "DICABUT",
			_ => throw new ArgumentOutOfRangeException(nameof(status))
		};

		public static Status ToStatusEnum(this string pseStatus) => pseStatus switch {
			"TERDAFTAR" => Status.Registered,
			"DIHENTIKAN_SEMENTARA" or
			"SUSPENDED_BY_TAKEL" => Status.Suspended,
			"DICABUT" => Status.Revoked,
			_ => throw new ArgumentOutOfRangeException(nameof(pseStatus), $"Unknown status {pseStatus}")
		};

		public static string ToStatusEmoji(this Status status) => status switch {
			Status.Registered => "✅",
			Status.Suspended => "⛔",
			Status.Revoked => "❌",
			_ => throw new ArgumentOutOfRangeException(nameof(status))
		};

		public static string ToFriendlyStatus(this Status status) => status switch {
			Status.Registered => "Terdaftar",
			Status.Suspended => "Dihentikan Sementara",
			Status.Revoked => "Dicabut",
			_ => throw new ArgumentOutOfRangeException(nameof(status))
		};
	}
}
