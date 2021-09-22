using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Hosting;
using BotNet.Services.MemoryPressureCoordinator;
using IronOcr;
using Microsoft.Extensions.Options;

namespace BotNet.Services.OCR {
	public class Reader : IPressurable {
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private readonly MemoryPressureSemaphore _memoryPressureSemaphore;
		private readonly string _ironOcrLicenseKey;
		private readonly TesseractConfiguration _tesseractConfiguration;
		private readonly OcrLanguage _primaryLanguage;
		private readonly ImmutableArray<OcrLanguage> _secondaryLanguages;

		public Reader(
			MemoryPressureSemaphore memoryPressureSemaphore,
			IOptions<IronOcrOptions> ironOcrOptionsAccessor,
			IOptions<HostingOptions> hostingOptionsAccessor
		) {
			_memoryPressureSemaphore = memoryPressureSemaphore;
			_memoryPressureSemaphore.Register(this);
			_ironOcrLicenseKey = ironOcrOptionsAccessor.Value.LicenseKey ?? throw new InvalidOperationException("IronOcr license key not configured. Please add a .NET secret with key 'IronOcr:LicenseKey' or a Docker secret with key 'IronOcr__LicenseKey'");

			long memory = hostingOptionsAccessor.Value.Memory;
			switch (memory) {
				case < 500_000_000L:
					_tesseractConfiguration = new TesseractConfiguration {
						EngineMode = TesseractEngineMode.LstmOnly,
						PageSegmentationMode = TesseractPageSegmentationMode.AutoOnly
					};
					_primaryLanguage = OcrLanguage.EnglishFast;
					_secondaryLanguages = ImmutableArray<OcrLanguage>.Empty;
					break;
				case < 1_500_000_000L:
					_tesseractConfiguration = new TesseractConfiguration {
						EngineMode = TesseractEngineMode.Default,
						PageSegmentationMode = TesseractPageSegmentationMode.AutoOsd
					};
					_primaryLanguage = OcrLanguage.English;
					_secondaryLanguages = ImmutableArray.Create(
						OcrLanguage.Japanese
					);
					break;
				default:
					_tesseractConfiguration = new TesseractConfiguration {
						EngineMode = TesseractEngineMode.Default,
						PageSegmentationMode = TesseractPageSegmentationMode.AutoOsd
					};
					_primaryLanguage = OcrLanguage.English;
					_secondaryLanguages = ImmutableArray.Create(
						OcrLanguage.Japanese
					);
					break;
			}
		}

		public async Task<string> ReadImageAsync(byte[] originalImage, CancellationToken cancellationToken) {
			await _semaphore.WaitAsync(cancellationToken);
			await _memoryPressureSemaphore.WaitAsync(this);
			try {
				Installation.LicenseKey = _ironOcrLicenseKey;
				IronTesseract ironTesseract = new(_tesseractConfiguration);
				ironTesseract.Language = _primaryLanguage;
				foreach (OcrLanguage secondaryLanguage in _secondaryLanguages) {
					ironTesseract.AddSecondaryLanguage(secondaryLanguage);
				}
				using OcrInput ocrInput = new(originalImage);
				OcrResult result = await ironTesseract.ReadAsync(ocrInput);
				return result.Text;
			} catch (OperationCanceledException) {
				throw;
			} catch {
				return "";
			} finally {
				_memoryPressureSemaphore.Release(this);
				_semaphore.Release();
			}
		}

		public Task ApplyPressureAsync() {
			return _semaphore.WaitAsync();
		}

		public void ReleasePressure() {
			_semaphore.Release();
		}
	}
}
