using System.Collections.Concurrent;
using System;
using System.Collections.Immutable;
using BotNet.Services.PSE.JsonModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace BotNet.Services.PSE {
	public class PSECrawler {
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private DateTime? _lastGenerated;
		private readonly ConcurrentDictionary<(Domicile Domicile, Status Status), int> _totalPagesByCategory = new();
		private readonly HashSet<(Domicile Domicile, Status Status, int Page)> _uncrawledPages = new();
		private readonly ConcurrentDictionary<Domicile, ConcurrentDictionary<int, DigitalService>> _digitalServiceByIdByDomicile = new();
		private readonly PSEClient _client;
		private readonly ILogger<PSECrawler> _logger;

		public PSECrawler(
			PSEClient client,
			ILogger<PSECrawler> logger
		) {
			_client = client;
			_logger = logger;
		}

		public async Task CrawlAsync(CancellationToken cancellationToken) {
			// Create timeout source and link it together with service cancellation token
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromMinutes(4));
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
			cancellationToken = linkedSource.Token;

			// Suppress HttpRequestException and OperationCanceledException
			try {
				// Check whether data has been updated
				await _semaphore.WaitAsync(cancellationToken);
				try {
					DateTime lastGenerated = await _client.GetLastGeneratedAsync(cancellationToken);
					if (lastGenerated != _lastGenerated) {
						_lastGenerated = lastGenerated;
						_totalPagesByCategory.Clear();
						lock (_uncrawledPages) {
							_uncrawledPages.Clear();
						}
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
											await _client.GetDigitalServicesAsync(
												domicile: domicile,
												status: status,
												page: 1,
												cancellationToken: cancellationToken
											);

										// Save last page information
										_totalPagesByCategory.TryAdd((domicile, status), paginationMetadata.LastPage);

										// Save uncrawled pages
										lock (_uncrawledPages) {
											for (int page = 2; page <= paginationMetadata.LastPage; page++) {
												_uncrawledPages.Add((domicile, status, page));
											}
										}

										// Save crawled items
										foreach (DigitalService digitalService in digitalServices) {
											ConcurrentDictionary<int, DigitalService> digitalServiceById = _digitalServiceByIdByDomicile.GetOrAdd(domicile, _ => new ConcurrentDictionary<int, DigitalService>());
											digitalServiceById.TryAdd(digitalService.Id, digitalService);
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
						{
							int uncrawledPagesCount;
							Domicile domicile = default;
							Status status = default;
							int page = 0;
							lock (_uncrawledPages) {
								uncrawledPagesCount = _uncrawledPages.Count;
								if (uncrawledPagesCount > 0) {
									(domicile, status, page) = _uncrawledPages.First();
								}
							}
							if (uncrawledPagesCount > 0) {
								// Crawl the next uncrawled page
								await _semaphore.WaitAsync(cancellationToken);
								try {
									(ImmutableList<DigitalService> digitalServices, PaginationMetadata paginationMetadata) =
										await _client.GetDigitalServicesAsync(
											domicile: domicile,
											status: status,
											page: page,
											cancellationToken: cancellationToken
										);

									// Save crawled items
									foreach (DigitalService digitalService in digitalServices) {
										ConcurrentDictionary<int, DigitalService> digitalServiceById = _digitalServiceByIdByDomicile.GetOrAdd(domicile, _ => new ConcurrentDictionary<int, DigitalService>());
										digitalServiceById.TryAdd(digitalService.Id, digitalService);
										_logger.LogInformation("Crawled {0}", digitalService.Attributes.Name);
									}

									// Remove page from uncrawled pages
									_uncrawledPages.Remove((domicile, status, page));
								} finally {
									_semaphore.Release();
								}
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

		public ImmutableDictionary<(Domicile Domicile, Status Status), (int? Crawled, int? TotalPages)> GetCrawlingStatus() {
			return Enum.GetValues<Domicile>().Zip(Enum.GetValues<Status>(), (domicile, status) => (domicile, status))
				.ToImmutableDictionary(
					keySelector: key => key,
					elementSelector: key => {
						(Domicile domicile, Status status) = key;
						if (!_totalPagesByCategory.TryGetValue(key, out int totalPages)) {
							return ((int?)null, (int?)null);
						}
						if ((
							from p in _uncrawledPages
							where p.Domicile == domicile && p.Status == status
							orderby p.Page
							select p.Page
						).Take(1).ToList() is { Count: 0 } pages) {
							return (pages[0] - 1, totalPages);
						} else {
							return (totalPages, totalPages);
						}
					}
				);
		}

		public ImmutableList<DigitalService> GetAllByCategory(Domicile domicile, Status status, int skip, int take) {
			if (!_digitalServiceByIdByDomicile.TryGetValue(domicile, out ConcurrentDictionary<int, DigitalService>? digitalServiceById)) {
				return ImmutableList<DigitalService>.Empty;
			}

			return (
				from d in digitalServiceById.Values
				where d.Attributes.Status == status
				select d
			).Skip(skip).Take(take).ToImmutableList();
		}

		public ImmutableList<(Domicile Domicile, DigitalService DigitalService)> Search(string keyword, int take) {
			ImmutableList<(Domicile Domicile, DigitalService DigitalService)>.Builder builder = ImmutableList.CreateBuilder<(Domicile Domicile, DigitalService DigitalService)>();

			// Populate by URL
			foreach (Domicile domicile in Enum.GetValues<Domicile>()) {
				if (_digitalServiceByIdByDomicile.TryGetValue(domicile, out ConcurrentDictionary<int, DigitalService>? digitalServiceById)) {
					foreach (DigitalService digitalService in digitalServiceById.Values) {
						if (digitalService.Attributes.Website.Contains(keyword, StringComparison.OrdinalIgnoreCase)) {
							builder.Add((domicile, digitalService));
							if (builder.Count >= take) {
								return builder.ToImmutable();
							}
						}
					}
				}
			}

			// Populate by company name
			foreach (Domicile domicile in Enum.GetValues<Domicile>()) {
				if (_digitalServiceByIdByDomicile.TryGetValue(domicile, out ConcurrentDictionary<int, DigitalService>? digitalServiceById)) {
					foreach (DigitalService digitalService in digitalServiceById.Values) {
						if (digitalService.Attributes.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) {
							builder.Add((domicile, digitalService));
							if (builder.Count >= take) {
								return builder.ToImmutable();
							}
						}
					}
				}
			}

			// Populate by system name
			foreach (Domicile domicile in Enum.GetValues<Domicile>()) {
				if (_digitalServiceByIdByDomicile.TryGetValue(domicile, out ConcurrentDictionary<int, DigitalService>? digitalServiceById)) {
					foreach (DigitalService digitalService in digitalServiceById.Values) {
						if (digitalService.Attributes.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) {
							builder.Add((domicile, digitalService));
							if (builder.Count >= take) {
								return builder.ToImmutable();
							}
						}
					}
				}
			}

			return builder.ToImmutable();
		}
	}
}
