using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class AssistantBot {
		private readonly OpenAIClient _openAIClient;

		public AssistantBot(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public Task<string> AskSomethingAsync(string name, string question, CancellationToken cancellationToken) {
			string prompt = $"Berikut ini adalah sebuah percakapan antara seorang manusia bernama {name} dengan sebuah bot asisten. "
				+ "Bot ini sangat ramah, membantu, kreatif, dan cerdas.\n\n"
				+ "Manusia: Halo, apa kabar?\n"
				+ "TeknumBot: Saya bot yang diciptakan oleh TEKNUM. Apakah ada yang bisa saya bantu?\n\n"
				+ $"Manusia: {question}\n"
				+ "TeknumBot: ";
			return _openAIClient.DavinciCodexAutocompleteAsync(
				source: prompt,
				stop: new[] { "Manusia:" },
				maxRecursion: 0,
				cancellationToken: cancellationToken
			);
		}
	}
}
