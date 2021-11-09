using System;
using System.Collections.Generic;

namespace BotNet.Services.RateLimit.Internal {
	internal static class CooldownFormatter {
		public static string Format(TimeSpan cooldown) {
			List<string> timeComponents = new();
			double hours = cooldown.TotalHours;
			if (hours >= 1) {
				int h = (int)Math.Floor(hours);
				timeComponents.Add($"{h} jam");
				hours -= h;
			}
			double minutes = hours * 60;
			if (minutes >= 1) {
				int m = (int)Math.Floor(minutes);
				timeComponents.Add($"{m} menit");
				minutes -= m;
			}
			double seconds = minutes * 60;
			if (timeComponents.Count == 0 || seconds >= 0.1) {
				timeComponents.Add($"{seconds:0.0} detik");
			}
			return "dalam " + string.Join(" ", timeComponents);
		}
	}
}
