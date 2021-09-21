using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MemoryPressureCoordinator;
using IronOcr;
using Microsoft.Extensions.Options;

namespace BotNet.Services.OCR {
	public class Reader : IPressurable {
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private readonly MemoryPressureSemaphore _memoryPressureSemaphore;
		private readonly string _ironOcrLicenseKey;

		public Reader(
			MemoryPressureSemaphore memoryPressureSemaphore,
			IOptions<IronOcrOptions> ironOcrOptionsAccessor
		) {
			_memoryPressureSemaphore = memoryPressureSemaphore;
			_memoryPressureSemaphore.Register(this);
			_ironOcrLicenseKey = ironOcrOptionsAccessor.Value.LicenseKey ?? throw new InvalidOperationException("IronOcr license key not configured. Please add a .NET secret with key 'IronOcr:LicenseKey' or a Docker secret with key 'IronOcr__LicenseKey'");
		}

		public async Task<string> ReadImageAsync(byte[] originalImage, CancellationToken cancellationToken) {
			await _semaphore.WaitAsync(cancellationToken);
			await _memoryPressureSemaphore.WaitAsync(this);
			try {
				Installation.LicenseKey = _ironOcrLicenseKey;
				IronTesseract ironTesseract = new();
				ironTesseract.Language = OcrLanguage.EnglishFast;
				//ironTesseract.AddSecondaryLanguage(OcrLanguage.Japanese);
				using OcrInput ocrInput = new(originalImage);
				OcrResult result = await ironTesseract.ReadAsync(ocrInput);
				return result.Text;
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
