using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class Translator(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

		public async Task<string> TranslateAsync(string sentence, string languagePair, CancellationToken cancellationToken) {
			switch (languagePair) {
				case "eniden":
					return await TranslateAsync(
						sentence: await TranslateAsync(sentence, "enid", cancellationToken),
						languagePair: "iden",
						cancellationToken: cancellationToken
					);
				case "idenid":
					return await TranslateAsync(
						sentence: await TranslateAsync(sentence, "iden", cancellationToken),
						languagePair: "enid",
						cancellationToken: cancellationToken
					);
			}
			string? prompt = languagePair switch {
				"enid" => "English: I do not speak Indonesian.\n"
					+ "Indonesian: Saya tidak bisa berbicara bahasa Indonesia.\n\n"

					+ "English: See you later!\n"
					+ "Indonesian: Sampai jumpa lagi!\n\n"

					+ "English: Where is a good restaurant?\n"
					+ "Indonesian: Di mana restoran yang bagus?\n\n"

					+ "English: What rooms do you have available?\n"
					+ "Indonesian: Kamar yang mana yang tersedia?\n\n"

					+ $"English: {sentence}\n"
					+ "Indonesian:",
				"iden" => "Indonesian: Saya tidak bisa berbicara bahasa Indonesia.\n"
					+ "English: I do not speak Indonesian.\n\n"

					+ "Indonesian: Sampai jumpa lagi!\n"
					+ "English: See you later!\n\n"

					+ "Indonesian: Di mana restoran yang bagus?\n"
					+ "English: Where is a good restaurant?\n\n"

					+ "Indonesian: Kamar yang mana yang tersedia?\n"
					+ "English: What rooms do you have available?\n\n"

					+ $"Indonesian: {sentence}\n"
					+ "English:",
				_ => throw new NotImplementedException()
			};
			return await _openAIClient.AutocompleteAsync(
				engine: "text-davinci-002",
				prompt: prompt,
				stop: ["\n"],
				maxTokens: 128,
				frequencyPenalty: 0.0,
				presencePenalty: 0.0,
				temperature: 0.0,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}
	}
}
