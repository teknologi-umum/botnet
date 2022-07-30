using System;
using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.Models {
	public record DigitalServiceAttributes(
		[property: JsonPropertyName("sistem_elektronik_id")] int ServiceId,
		[property: JsonPropertyName("nomor_pb_umku")] string PBUMKUNumber,
		[property: JsonPropertyName("nama")] string Name,
		[property: JsonPropertyName("website")] string Website,
		[property: JsonPropertyName("sektor")] string Sector,
		[property: JsonPropertyName("nama_perusahaan")] string CompanyName,
		[property: JsonPropertyName("tanggal_daftar")] string Registered,
		[property: JsonPropertyName("nomor_tanda_daftar")] string RegistrationNumber,
		[property: JsonPropertyName("qr_code")] Uri QRCodeUrl,
		[property: JsonPropertyName("status_id")] string PSEStatus
	) {
		[JsonIgnore]
		public Status Status => PSEStatus.ToStatusEnum();
	}
}
