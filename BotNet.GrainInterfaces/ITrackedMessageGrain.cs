using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;

namespace BotNet.GrainInterfaces {
	/// <summary>
	/// Key: Message id
	/// </summary>
	public interface ITrackedMessageGrain : IGrainWithIntegerKey {
		Task TrackMessageAsync(string sender, string text, long? replyToMessageId);
		Task<(string? Sender, string? Text, long? ReplyToMessageId)> GetMessageAsync();
		Task<ImmutableList<(string Sender, string Text)>> GetThreadAsync(int maxLines);
	}
}
