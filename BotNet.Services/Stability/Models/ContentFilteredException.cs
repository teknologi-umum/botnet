using System;

namespace BotNet.Services.Stability.Models {
	public sealed class ContentFilteredException : Exception {
		public ContentFilteredException() { }

		public ContentFilteredException(
			string? message
		) : base(message) { }
	}
}
