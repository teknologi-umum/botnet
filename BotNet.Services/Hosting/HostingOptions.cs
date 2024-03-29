﻿using System.Diagnostics.CodeAnalysis;

namespace BotNet.Services.Hosting {
	[ExcludeFromCodeCoverage]
	public class HostingOptions {
		public string? HostName { get; set; }
		public long Memory { get; set; }
		public bool UseLongPolling { get; set; }
	}
}
