﻿using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global

namespace BotNet.Services.Pemilu2024 {
	public sealed record Paslon(
		[property: JsonPropertyName("ts")] string Timestamp,
		string Nama,
		string Warna,
		int NomorUrut
	);

	public sealed record Partai(
		[property: JsonPropertyName("ts")] string Timestamp,
		int IdPartai,
		int IdPilihan,
		bool IsAceh,
		string Nama,
		string NamaLengkap,
		int NomorUrut,
		string Warna
	);

	public sealed record Wilayah(
		string Nama,
		int Id,
		string Kode,
		int Tingkat
	);

	public sealed record Caleg(
		string Nama,
		int NomorUrut,
		string JenisKelamin,
		string TempatTinggal
	);

	public sealed record ReportPilpres(
		[property: JsonPropertyName("ts")] string Timestamp,
		string Psu,
		IDictionary<string, decimal> Chart,
		[property: JsonPropertyName("table")] IDictionary<string, ReportPilpres.Row> RowByKodeWilayah,
		ReportPilpres.Progress Progres
	) {
		public sealed record Row {
			public string? Psu { get; set; }
			public decimal Persen { get; set; }
			public bool StatusProgress { get; set; }

			[JsonExtensionData]
			public IDictionary<string, JsonElement>? VotesByKodeCalonJson { get; set; }

			[JsonIgnore]
			public IDictionary<string, int>? VotesByKodeCalon {
				get {
					if (VotesByKodeCalonJson is null) {
						return null;
					}

					Dictionary<string, int> votesByKodeCalon = [];
					foreach (KeyValuePair<string, JsonElement> kvp in VotesByKodeCalonJson) {
						if (kvp.Value.ValueKind == JsonValueKind.Number
							&& kvp.Value.TryGetInt32(out int votes)) {
							votesByKodeCalon[kvp.Key] = votes;
						}
					}
					return votesByKodeCalon;
				}
			}
		}

		public sealed record Progress(
			int Total,
			int Progres
		);
	}

	public sealed record ReportPilegDprByWilayah(
		[property: JsonPropertyName("ts")] string Timestamp,
		string Psu,
		string Mode,
		IDictionary<string, decimal> Chart,
		[property: JsonPropertyName("table")] IDictionary<string, ReportPilegDprByWilayah.Row> RowByKodeWilayah,
		ReportPilegDprByWilayah.Progress Progres
	) {
		public sealed record Row {
			public string? Psu { get; set; }
			public decimal Persen { get; set; }
			public bool StatusProgress { get; set; }

			[JsonExtensionData]
			public IDictionary<string, JsonElement>? VotesByKodePartaiJson { get; set; }

			[JsonIgnore]
			public IDictionary<string, int>? VotesByKodePartai {
				get {
					if (VotesByKodePartaiJson is null) {
						return null;
					}

					Dictionary<string, int> votesByKodePartai = [];
					foreach (KeyValuePair<string, JsonElement> kvp in VotesByKodePartaiJson) {
						if (kvp.Value.ValueKind == JsonValueKind.Number
							&& kvp.Value.TryGetInt32(out int votes)) {
							votesByKodePartai[kvp.Key] = votes;
						}
					}
					return votesByKodePartai;
				}
			}
		}

		public sealed record Progress(
			int Total,
			int Progres
		);
	}

	public sealed record ReportPilegDprByDapil(
		[property: JsonPropertyName("ts")] string Timestamp,
		string Mode,
		IDictionary<string, decimal> Chart,
		[property: JsonPropertyName("table")] IDictionary<string, ReportPilegDprByDapil.Row?> RowByKodeDapil,
		ReportPilegDprByDapil.Progress Progres
	) {
		public sealed record Row {
			public decimal Persen { get; set; }

			[JsonExtensionData]
			public IDictionary<string, JsonElement>? VotesByKodePartaiJson { get; set; }

			[JsonIgnore]
			public IDictionary<string, int>? VotesByKodePartai {
				get {
					if (VotesByKodePartaiJson is null) {
						return null;
					}

					Dictionary<string, int> votesByKodePartai = [];
					foreach (KeyValuePair<string, JsonElement> kvp in VotesByKodePartaiJson) {
						if (kvp.Value.ValueKind == JsonValueKind.Number
							&& kvp.Value.TryGetInt32(out int votes)) {
							votesByKodePartai[kvp.Key] = votes;
						}
					}
					return votesByKodePartai;
				}
			}
		}

		public sealed record Progress(
			int Total,
			int Progres
		);
	}

	public sealed record ReportCalegDpr(
		[property: JsonPropertyName("ts")] string Timestamp,
		string Mode,
		IDictionary<string, decimal> Chart,
		[property: JsonPropertyName("table")] IDictionary<string, IDictionary<string, int>> VotesByKodeCalegByKodePartai,
		ReportCalegDpr.Progress Progres
	) {
		public sealed record Progress(
			int Total,
			int Progres
		);
	}
}
