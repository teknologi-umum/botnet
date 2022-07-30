using System;

namespace BotNet.Services.PSE.Models {
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
			"DIHENTIKAN_SEMENTARA" => Status.Suspended,
			"DICABUT" => Status.Revoked,
			_ => throw new ArgumentOutOfRangeException(nameof(pseStatus))
		};
	}
}
