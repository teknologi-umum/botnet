using System;
using System.Collections.Generic;
using System.Linq;

namespace BotNet.Services.Soundtrack {
	public class SoundtrackProvider {
	private static readonly List<SoundtrackSite> Sites = new() {
		new("musicForProgramming()", "https://musicforprogramming.net/latest"),
		new("Poolsuite FM", "https://poolsuite.net/"),
		new("Lofi Cafe", "https://lofi.cafe/"),
		new("Radio Garden", "https://radio.garden/"),
		new("Chillhop", "https://app.chillhop.com/"),
		new("DevTunes FM", "https://radio.madza.dev/"),
		new("Nightride FM", "https://nightride.fm/"),
		new("Code Radio", "https://coderadio.freecodecamp.org/"),
		new("FilterMusic", "https://filtermusic.net/")
	};

	public (SoundtrackSite First, SoundtrackSite Second) GetHourlyPicks() {
			DateTime now = DateTime.UtcNow;
			// Seed based on year + day + hour for hourly rotation
			int seed = now.Year * 10000 + now.DayOfYear * 100 + now.Hour;
			Random rng = new(seed);

			// Shuffle and take first 2
			List<SoundtrackSite> shuffled = Sites.OrderBy(_ => rng.Next()).ToList();
			return (shuffled[0], shuffled[1]);
		}
	}

	public record SoundtrackSite(string Name, string Url);
}
