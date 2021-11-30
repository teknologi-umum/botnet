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
			string prompt = $"I am a highly intelligent question answering bot. If you ask me a question that is rooted in truth, I will give you the answer. If you ask me a question that is nonsense, trickery, or has no clear answer, I will respond with \"Unknown\".\n\n"
				+ "Q: What is human life expectancy in the United States?\n"
				+ "A: Human life expectancy in the United States is 78 years.\n\n"
				+ "Q: Who was president of the United States in 1955?\n"
				+ "A: Dwight D. Eisenhower was president of the United States in 1955.\n\n"
				+ "Q: Which party did he belong to?\n"
				+ "A: He belonged to the Republican Party.\n\n"
				+ "Q: What is the square root of banana?\n"
				+ "A: Unknown\n\n"
				+ "Q: How does a telescope work?\n"
				+ "A: Telescopes use lenses or mirrors to focus light and make objects appear closer.\n\n"
				+ "Q: Where were the 1992 Olympics held?\n"
				+ "A: The 1992 Olympics were held in Barcelona, Spain.\n\n"
				+ "Q: How many squigs are in a bonk?\n"
				+ "A: Unknown\n\n"
				+ $"Q: {question}\n"
				+ "A: ";
			return _openAIClient.AutocompleteAsync(
				engine: "davinci",
				prompt: prompt,
				stop: new[] { "\n" },
				maxTokens: 100,
				frequencyPenalty: 0.0,
				presencePenalty: 0.0,
				temperature: 0.0,
				cancellationToken: cancellationToken
			);
		}
	}
}
