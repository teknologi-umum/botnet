using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE;
using BotNet.Services.PSE.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BotNet.PSE {
	public class PSEService : IHostedService, IDisposable {
		private readonly IServiceProvider _serviceProvider;
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private Timer? _timer;
		private CancellationTokenSource? _cancellationTokenSource;
		private DateTime? _lastGenerated;
		private readonly Dictionary<(Domicile Domicile, Status Status), int> _totalPagesByCategory = new();
		private readonly HashSet<(Domicile Domicile, Status Status, int Page)> _uncrawledPages = new();
		private readonly Dictionary<int, DigitalService> _digitalServiceById = new();
		private readonly ILogger<PSEService> _logger;

		public PSEService(
			IServiceProvider serviceProvider,
			ILogger<PSEService> logger
		) {
			_serviceProvider = serviceProvider;
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
			// Create timeout source and link it together with service cancellation token
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromMinutes(4));
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, _cancellationTokenSource!.Token);
			CancellationToken cancellationToken = linkedSource.Token;

			// Suppress HttpRequestException and OperationCanceledException
			try {
				PSEClient client = _serviceProvider.GetRequiredService<PSEClient>();

				// Check whether data has been updated
				await _semaphore.WaitAsync(cancellationToken);
				try {
					DateTime lastGenerated = await client.GetLastGeneratedAsync(cancellationToken);
					if (_lastGenerated.HasValue && lastGenerated != _lastGenerated) {
						_lastGenerated = lastGenerated;
						_totalPagesByCategory.Clear();
						_uncrawledPages.Clear();
						_digitalServiceById.Clear();
					}
				} catch (Exception exc) {
					_logger.LogError(exc, "Couldn't fetch LastGenerated. Skipping for now.");
				} finally {
					_semaphore.Release();
				}

				// Crawl loop
				int failures = 0;
				while (!cancellationToken.IsCancellationRequested) {
				continueLoop:
					await Task.Delay(millisecondsDelay: 100, cancellationToken);

					try {
						// Check whether there is an uncrawled category
						foreach (Domicile domicile in Enum.GetValues<Domicile>()) {
							foreach (Status status in Enum.GetValues<Status>()) {
								if (!_totalPagesByCategory.ContainsKey((domicile, status))) {
									// Crawl first page of uncrawled category
									await _semaphore.WaitAsync(cancellationToken);
									try {
										(ImmutableList<DigitalService> digitalServices, PaginationMetadata paginationMetadata) =
											await client.GetDigitalServicesAsync(
												domicile: domicile,
												status: status,
												page: 1,
												cancellationToken: cancellationToken
											);

										// Save last page information
										_totalPagesByCategory.Add((domicile, status), paginationMetadata.LastPage);

										// Save uncrawled pages
										for (int page = 2; page <= paginationMetadata.LastPage; page++) {
											_uncrawledPages.Add((domicile, status, page));
										}

										// Save crawled items
										foreach (DigitalService digitalService in digitalServices) {
											_digitalServiceById.Add(digitalService.Id, digitalService);
											_logger.LogInformation("Crawled {0}", digitalService.Attributes.Name);
										}

										// Continue crawl loop
										goto continueLoop;
									} finally {
										_semaphore.Release();
									}
								}
							}
						}

						// Check whether there is an uncrawled page
						if (_uncrawledPages.Count > 0) {
							(Domicile domicile, Status status, int page) = _uncrawledPages.First();

							// Crawl the next uncrawled page
							await _semaphore.WaitAsync(cancellationToken);
							try {
								(ImmutableList<DigitalService> digitalServices, PaginationMetadata paginationMetadata) =
									await client.GetDigitalServicesAsync(
										domicile: domicile,
										status: status,
										page: page,
										cancellationToken: cancellationToken
									);

								// Save crawled items
								foreach (DigitalService digitalService in digitalServices) {
									_digitalServiceById.Add(digitalService.Id, digitalService);
									_logger.LogInformation("Crawled {0}", digitalService.Attributes.Name);
								}

								// Remove page from uncrawled pages
								_uncrawledPages.Remove((domicile, status, page));
							} finally {
								_semaphore.Release();
							}
						}

						failures = 0;
					} catch (Exception exc) when (exc is HttpRequestException or JsonException) {
						_logger.LogError(exc, exc.GetType().Name);
						// Deal with rate limit
						failures++;
						if (failures >= 10) {
							throw;
						}
						await Task.Delay(millisecondsDelay: 2000, cancellationToken);
					}
				}
			} catch (HttpRequestException exc) {
				_logger.LogError(exc, nameof(HttpRequestException));
			} catch (OperationCanceledException) {
				// Do nothing
			}
		}

		public void Dispose() {
			_timer?.Dispose();
			_cancellationTokenSource?.Dispose();
		}
	}
}
