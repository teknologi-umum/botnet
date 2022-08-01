namespace BotNet.Services.TrustPositif.JsonModels {
	public record BlacklistLookupResult(
		string Domain,
		string Status
	);
}
