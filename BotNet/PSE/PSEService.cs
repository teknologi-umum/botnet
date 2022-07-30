using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE;
using Microsoft.Extensions.Hosting;

namespace BotNet.PSE {
	public class PSEService : IHostedService, IDisposable {
		private readonly PSECrawler _crawler;
		private Timer? _timer;
		private CancellationTokenSource? _cancellationTokenSource;

		public PSEService(
			PSECrawler crawler
		) {
			_crawler = crawler;
		}

		public Task StartAsync(CancellationToken cancellationToken) {
			_cancellationTokenSource = new();
			_timer = new Timer(CrawlAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
			return Task.CompletedTask;
		}
		public Task StopAsync(CancellationToken cancellationToken) {
			_timer?.Change(Timeout.Infinite, 0);
			_cancellationTokenSource?.Cancel();
			return Task.CompletedTask;
		}

		public async void CrawlAsync(object? state) {
			await _crawler.CrawlAsync(_cancellationTokenSource!.Token);
		}

		public void Dispose() {
			_timer?.Dispose();
			_cancellationTokenSource?.Dispose();
		}
	}
}
