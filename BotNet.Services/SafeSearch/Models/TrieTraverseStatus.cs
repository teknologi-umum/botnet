namespace BotNet.Services.SafeSearch.Models {
	public enum TrieTraverseStatus {
		Traversing,
		Extraneous,
		Mismatch,
		PartialMatch,
		FullMatch
	}
}
