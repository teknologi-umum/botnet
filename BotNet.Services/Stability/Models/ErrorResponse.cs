namespace BotNet.Services.Stability.Models {
	public sealed record ErrorResponse(
		string? Id,
		string? Message,
		string? Name
	);
}
