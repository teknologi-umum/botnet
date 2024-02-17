using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

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
}
