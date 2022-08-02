using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BotNet.PSE {
	public class PSEService : IHostedService, IDisposable {
		private readonly PSECrawler _crawler;
		private readonly ILogger<PSEService> _logger;
		private Timer? _timer;
		private CancellationTokenSource? _cancellationTokenSource;

		public PSEService(
			PSECrawler crawler,
			ILogger<PSEService> logger
		) {
			_crawler = crawler;
			_logger = logger;
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
			try {
				await _crawler.CrawlAsync(_cancellationTokenSource!.Token);
			} catch (OperationCanceledException) {
				return;
			} catch (Exception exc) {
				_logger.LogError(exc, "Error while crawling PSE");
			}
		}

		public void Dispose() {
			_timer?.Dispose();
			_cancellationTokenSource?.Dispose();
		}
	}
}
