using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public record DigitalService(
		[property: JsonPropertyName("domain")] string Domain,
		[property: JsonPropertyName("is_domestik")] bool IsDomestik,
		[property: JsonPropertyName("nama_se")] string NamaSe,
		[property: JsonPropertyName("nomor_tdpse")] string NomorTdpse,
		[property: JsonPropertyName("pse_name")] string PseName,
		[property: JsonPropertyName("se_id")] string SeId,
		[property: JsonPropertyName("status")] string Status,
		[property: JsonPropertyName("tanggal_terdaftar")] string TanggalTerdaftar
	);
}
