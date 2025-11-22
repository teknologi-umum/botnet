using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Mermaid;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace BotNet.Tests.Services.Mermaid {
	public class MermaidRendererTests : IDisposable {
		private readonly MermaidRenderer _mermaidRenderer;
		private readonly HttpClient _httpClient;

		public MermaidRendererTests() {
			_httpClient = new HttpClient();
			_mermaidRenderer = new MermaidRenderer(_httpClient);
		}

		public void Dispose() {
			_httpClient.Dispose();
		}

		[Fact(Skip = "Requires internet access to Kroki API")]
		public async Task RenderMermaidAsync_WithValidSimpleDiagram_ReturnsNonEmptyByteArray() {
			// Arrange
			string mermaidCode = "graph TD; A-->B;";

			// Act
			byte[] result = await _mermaidRenderer.RenderMermaidAsync(mermaidCode, CancellationToken.None);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
		}

		[Fact(Skip = "Requires internet access to Kroki API")]
		public async Task RenderMermaidAsync_WithValidSimpleDiagram_ReturnsValidPngImage() {
			// Arrange
			string mermaidCode = "graph TD; A-->B;";

			// Act
			byte[] result = await _mermaidRenderer.RenderMermaidAsync(mermaidCode, CancellationToken.None);

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
			bitmap.Width.ShouldBeGreaterThan(0);
			bitmap.Height.ShouldBeGreaterThan(0);
		}

		[Fact(Skip = "Requires internet access to Kroki API")]
		public async Task RenderMermaidAsync_WithIncompleteDiagram_ThrowsMermaidRenderException() {
			// Arrange
			string mermaidCode = "graph TD; A";

			// Act & Assert
			await Should.ThrowAsync<MermaidRenderException>(async () => 
				await _mermaidRenderer.RenderMermaidAsync(mermaidCode, CancellationToken.None));
		}

		[Fact]
		public void RenderMermaidAsync_WithNullCode_ThrowsArgumentException() {
			// Act & Assert
			Should.Throw<ArgumentException>(() => 
				_mermaidRenderer.RenderMermaidAsync(null!, CancellationToken.None));
		}

		[Fact]
		public void RenderMermaidAsync_WithEmptyCode_ThrowsArgumentException() {
			// Act & Assert
			Should.Throw<ArgumentException>(() => 
				_mermaidRenderer.RenderMermaidAsync("", CancellationToken.None));
		}

		[Fact]
		public void RenderMermaidAsync_WithWhitespaceCode_ThrowsArgumentException() {
			// Act & Assert
			Should.Throw<ArgumentException>(() => 
				_mermaidRenderer.RenderMermaidAsync("   ", CancellationToken.None));
		}

		[Theory(Skip = "Requires internet access to Kroki API")]
		[InlineData("graph TD; A-->B;")]
		[InlineData("graph LR; Start-->Stop;")]
		[InlineData("sequenceDiagram\nAlice->>Bob: Hello Bob!")]
		[InlineData("flowchart TD\nA[Start] --> B[Process]")]
		public async Task RenderMermaidAsync_WithVariousDiagramTypes_ReturnsValidImage(string mermaidCode) {
			// Act
			byte[] result = await _mermaidRenderer.RenderMermaidAsync(mermaidCode, CancellationToken.None);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}
	}
}
