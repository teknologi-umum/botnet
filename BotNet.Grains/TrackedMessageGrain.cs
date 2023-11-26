using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using Orleans;

namespace BotNet.Grains {
	public class TrackedMessageGrain : Grain, ITrackedMessageGrain {
		private string? _sender;
		private string? _text;
		private long? _replyToMessageId;

		public Task TrackMessageAsync(string sender, string text, long? replyToMessageId) {
			_sender = sender;
			_text = text;
			_replyToMessageId = replyToMessageId;
			DelayDeactivation(TimeSpan.FromHours(1));
			return Task.CompletedTask;
		}

		public Task<(string? Sender, string? Text, long? ReplyToMessageId)> GetMessageAsync() {
			DelayDeactivation(TimeSpan.FromHours(1));
			return Task.FromResult((_sender, _text, _replyToMessageId));
		}

		public async Task<ImmutableList<(string Sender, string Text)>> GetThreadAsync(int maxLines) {
			if (_sender is null || _text is null) return [];

			ImmutableList<(string Sender, string Text)>.Builder builder = ImmutableList.Create((_sender, _text)).ToBuilder();

			long? messageId = _replyToMessageId;
			for (int i = 1; messageId.HasValue && i < maxLines; i++) {
				ITrackedMessageGrain repledToMessageGrain = GrainFactory.GetGrain<ITrackedMessageGrain>(messageId.Value);
				(string? sender, string? text, messageId) = await repledToMessageGrain.GetMessageAsync();
				if (sender is not null && text is not null) {
					builder.Add((sender, text));
				}
			}

			DelayDeactivation(TimeSpan.FromHours(1));
			return builder.ToImmutableList();
		}
	}
}
