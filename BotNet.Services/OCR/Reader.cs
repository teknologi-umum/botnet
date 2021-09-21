using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MemoryPressureCoordinator;
using IronOcr;

namespace BotNet.Services.OCR {
	public class Reader : IPressurable {
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private readonly MemoryPressureSemaphore _memoryPressureSemaphore;

		public Reader(
			MemoryPressureSemaphore memoryPressureSemaphore
		) {
			_memoryPressureSemaphore = memoryPressureSemaphore;
			_memoryPressureSemaphore.Register(this);
		}

		public async Task<string> ReadImageAsync(byte[] originalImage, CancellationToken cancellationToken) {
			await _semaphore.WaitAsync(cancellationToken);
			await _memoryPressureSemaphore.WaitAsync(this);
			try {
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
