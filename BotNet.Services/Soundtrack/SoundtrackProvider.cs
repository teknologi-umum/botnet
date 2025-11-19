using System;
using System.Collections.Generic;
using System.Linq;

namespace BotNet.Services.Soundtrack {
	public class SoundtrackProvider {
		private static readonly List<SoundtrackSite> Sites = new() {
			new("musicForProgramming()", "https://musicforprogramming.net/latest"),
			new("Poolsuite FM", "https://poolsuite.net/"),
			new("Lofi Cafe", "https://lofi.cafe/"),
			new("LofiCafe", "https://loficafe.net/"),
			new("Radio Garden", "https://radio.garden/"),
			new("Chillhop", "https://app.chillhop.com/"),
			new("DevTunes FM", "https://radio.madza.dev/"),
			new("Nightride FM", "https://nightride.fm/"),
			new("Code Radio", "https://coderadio.freecodecamp.org/"),
			new("FilterMusic", "https://filtermusic.net/"),
			new("Lofi Girl - Hiphop Radio to Relax/Study to", "https://www.youtube.com/watch?v=jfKfPfyJRdk"),
			new("Lofi Girl - Synthwave Radio to Game/Chill to", "https://www.youtube.com/watch?v=4xDzrJKXOOY"),
			new("Lofi Girl - Hiphop Radio to Sleep/Chill to", "https://www.youtube.com/watch?v=28KRPhVzCus"),
			new("Lofi Girl - Jazz Radio to Chill/Study to", "https://www.youtube.com/watch?v=HuFYqnbVbzY"),
			new("Lofi Girl - Asian Radio to Relax/Study to", "https://www.youtube.com/watch?v=Na0w3Mz46GA"),
			new("Lofi Girl - Peaceful Piano Radio to Focus/Study to", "https://www.youtube.com/watch?v=TtkFsfOP9QI"),
			new("Lofi Girl - Dark Ambient Radio to Escape/Dream to", "https://www.youtube.com/watch?v=S_MOd40zlYU"),
			new("Lofi Girl - Sad Radio for Rainy Days", "https://www.youtube.com/watch?v=P6Segk8cr-c"),
			new("Lofi Girl - Medieval Radio to Scribe Manuscripts to", "https://www.youtube.com/watch?v=IxPANmjPaek")
		};

		public (SoundtrackSite First, SoundtrackSite Second) GetRandomPicks() {
			Random rng = Random.Shared;

			// Shuffle and take first 2
			List<SoundtrackSite> shuffled = Sites.OrderBy(_ => rng.Next()).ToList();
			return (shuffled[0], shuffled[1]);
		}
	}

	public record SoundtrackSite(string Name, string Url);
}
