namespace BotNet.Services.StackExchange.Models {
	public record StackExchangeUser(
		long AccountId,
		long Reputation,
		long UserId,
		string UserType,
		string ProfileImage,
		string DisplayName,
		string Link
	);
}
