using System.Threading.Tasks;
using IronOcr;

namespace BotNet.Services.OCR {
	public static class Reader {
		public static async Task<string> ReadImageAsync(byte[] originalImage) {
			IronTesseract ironTesseract = new();
			ironTesseract.AddSecondaryLanguage(OcrLanguage.Japanese);
			using OcrInput ocrInput = new(originalImage);
			OcrResult result = await ironTesseract.ReadAsync(ocrInput);
			return result.Text;
		}
	}
}
