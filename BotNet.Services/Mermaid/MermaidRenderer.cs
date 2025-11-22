using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Mermaid {
	public sealed class MermaidRenderer(
		HttpClient httpClient
	) {
		public async Task<byte[]> RenderMermaidAsync(string mermaidCode, CancellationToken cancellationToken) {
			if (string.IsNullOrWhiteSpace(mermaidCode)) {
				throw new ArgumentException("Mermaid code cannot be null or whitespace.", nameof(mermaidCode));
			}

			// Use Kroki API to render mermaid diagrams
			// Kroki is a free service that renders various diagram types
			string base64EncodedCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(mermaidCode));
			string url = $"https://kroki.io/mermaid/png/{base64EncodedCode}";

			HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

			if (!response.IsSuccessStatusCode) {
				string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new MermaidRenderException($"Failed to render mermaid diagram: {response.StatusCode} - {errorContent}");
			}

			return await response.Content.ReadAsByteArrayAsync(cancellationToken);
		}
	}

	public sealed class MermaidRenderException : Exception {
		public MermaidRenderException(string message) : base(message) { }
	}
}
