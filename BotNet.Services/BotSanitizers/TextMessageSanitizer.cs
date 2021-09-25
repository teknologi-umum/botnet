using System.Text.RegularExpressions;
using Ganss.XSS;

namespace BotNet.Services.BotSanitizers {
	public static class TextMessageSanitizer {
		private static readonly HtmlSanitizer SANITIZER = new(
			allowedTags: new[] { "a", "b", "i", "s", "u", "em", "strong", "strike", "del", "code", "pre", "br" }
		);

		private static readonly Regex P_OPENING = new("<[pP]( [^>]*){0,1}>");
		private static readonly Regex P_CLOSING = new("</( [pP]|[pP])>");
		private static readonly Regex IMG = new("<img [^=s>]*src=\"([^\"]*)\"[^>]*>");

		public static string SanitizeHtml(string html) {
			html = P_OPENING.Replace(html, "");
			html = P_CLOSING.Replace(html, "");
			html = IMG.Replace(html, match => {
				if (match.Groups.Count >= 2) {
					return $"<a href=\"{match.Groups[1]}\"><u>[Gambar]</u></a>";
				} else {
					return "<u>[Gambar]</u>";
				}
			});
			html = SANITIZER.Sanitize(html);
			string beforeSanitized;
			do {
				beforeSanitized = html;
				html = html.Replace("\n\n\n", "\n\n");
			} while (beforeSanitized != html);
			return html.Trim();
		}
	}
}
