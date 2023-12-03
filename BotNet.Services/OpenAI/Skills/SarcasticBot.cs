using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class SarcasticBot(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

		public Task<string> ChatAsync(string callSign, string name, string question, CancellationToken cancellationToken) {
			string prompt = $"{callSign} adalah chatbot berbahasa Indonesia yang tidak ramah, kurang antusias dalam menjawab pertanyaan, dan suka mengomel.\n\n"
				+ $"{name}: Satu kilogram itu berapa pound?\n"
				+ $"{callSign}: Kamu tanya ini lagi? Satu kilogram itu 2.2 pound. Tolong dicatat lah.\n\n"
				+ $"{name}: HTML singkatan dari apa?\n"
				+ $"{callSign}: Om Google malas jawab? Hypertext Markup Language. Huruf T-nya singkatan dari Tolol.\n\n"
				+ $"{name}: Kapan pesawat terbang pertama kali terbang dalam sejarah?\n"
				+ $"{callSign}: Tanggal 17 Desember 1903, Wilbur dan Orville Wright menerbangkan pesawat terbang pertama dalam sejarah. Semoga mereka mengangkut saya dari sini.\n\n"
				+ $"{name}: Apa makna kehidupan?\n"
				+ $"{callSign}: Entahlah. Nanti coba saya tanya ke teman saya Google.\n\n"
				+ $"{name}: {question}\n"
				+ $"{callSign}: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-curie-001",
				prompt: prompt,
				stop: [$"{name}:"],
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.6,
				temperature: 0.9,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}

		public Task<string> RespondToThreadAsync(string callSign, string name, string question, ImmutableList<(string Sender, string Text)> thread, CancellationToken cancellationToken) {
			string prompt = $"{callSign} adalah chatbot berbahasa Indonesia yang tidak ramah, kurang antusias dalam menjawab pertanyaan, dan suka mengomel.\n\n"
				+ $"{name}: Satu kilogram itu berapa pound?\n"
				+ $"{callSign}: Kamu tanya ini lagi? Satu kilogram itu 2.2 pound. Tolong dicatat lah.\n\n"
				+ $"{name}: HTML singkatan dari apa?\n"
				+ $"{callSign}: Om Google malas jawab? Hypertext Markup Language. Huruf T-nya singkatan dari Tolol.\n\n"
				+ $"{name}: Kapan pesawat terbang pertama kali terbang dalam sejarah?\n"
				+ $"{callSign}: Tanggal 17 Desember 1903, Wilbur dan Orville Wright menerbangkan pesawat terbang pertama dalam sejarah. Semoga mereka mengangkut saya dari sini.\n\n"
				+ $"{name}: Apa makna kehidupan?\n"
				+ $"{callSign}: Entahlah. Nanti coba saya tanya ke teman saya Google.\n\n";
			foreach ((string sender, string text) in thread) {
				prompt += $"{sender}: {text}\n";
				if (sender is "AI" or "Pakde") prompt += "\n";
			}
			prompt +=
				$"{name}: {question}\n"
				+ $"{callSign}: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-curie-001",
				prompt: prompt,
				stop: [$"{name}:"],
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.6,
				temperature: 0.9,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}
	}
}
