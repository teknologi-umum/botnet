using System;
using System.Collections.Immutable;
using System.Net;
using System.Text.RegularExpressions;
using BotNet.Services.Hosting;
using Microsoft.Extensions.Options;

namespace BotNet.Services.OpenAI {
	public partial class AttachmentGenerator(
		IOptions<HostingOptions> hostingOptionsAccessor
	) {
		private static readonly Regex HexColorCodePattern = HexColorCodeRegex();
		private readonly HostingOptions _hostingOptions = hostingOptionsAccessor.Value;

		public ImmutableList<Uri> GenerateAttachments(string message) {
			ImmutableList<Uri>.Builder builder = ImmutableList<Uri>.Empty.ToBuilder();

			// Detect hex color codes
			MatchCollection matches = HexColorCodePattern.Matches(message);
			foreach (Match match in matches) {
				builder.Add(new Uri($"https://{_hostingOptions.HostName}/renderer/color?name={WebUtility.UrlEncode(match.Value)}"));
			}

			return builder.ToImmutable();
		}

		[GeneratedRegex("#[a-fA-F0-9]{6}\\b")]
		private static partial Regex HexColorCodeRegex();
	}
}
