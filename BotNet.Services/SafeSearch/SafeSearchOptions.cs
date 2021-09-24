namespace BotNet.Services.SafeSearch {
	public class SafeSearchOptions {
		public string? BadWordListOwner { get; set; }
		public string? BadWordListRepository { get; set; }
		public string? DisallowedWebsitesPath { get; set; }
		public string? DisallowedWordsPath { get; set; }
	}
}
