using System;
using System.Collections.Generic;
using System.Linq;
using TimeZoneConverter;

namespace BotNet.Services.TimeZone {
	public class TimeZoneService {
		private static readonly Dictionary<string, string> AbbreviationToIanaMap = new(StringComparer.OrdinalIgnoreCase) {
			// Common US timezone abbreviations
			["CST"] = "America/Chicago", // Central Standard Time
			["ET"] = "America/New_York", // Eastern Time
			["EST"] = "America/New_York", // Eastern Standard Time
			["PST"] = "America/Los_Angeles", // Pacific Standard Time
			["PT"] = "America/Los_Angeles", // Pacific Time
			["MST"] = "America/Denver", // Mountain Standard Time
			["MT"] = "America/Denver", // Mountain Time
			
			// Indonesian timezone abbreviations
			["WIB"] = "Asia/Jakarta", // Waktu Indonesia Barat (Western Indonesian Time) - UTC+7
			["WITA"] = "Asia/Makassar", // Waktu Indonesia Tengah (Central Indonesian Time) - UTC+8
			["WIT"] = "Asia/Jayapura" // Waktu Indonesia Timur (Eastern Indonesian Time) - UTC+9
		};

		public TimeInfo GetTimeInfo(string cityOrTimeZone) {
			try {
				// Check if input is GMT+N or UTC+N format
				string? ianaCode = TryParseUtcOffset(cityOrTimeZone);
				
				if (ianaCode == null) {
					// Try abbreviation mapping
					if (AbbreviationToIanaMap.TryGetValue(cityOrTimeZone, out string? mappedIana)) {
						ianaCode = mappedIana;
					} else {
						// Try to find city name in IANA timezone IDs
						ianaCode = FindIanaByCity(cityOrTimeZone) ?? cityOrTimeZone;
					}
				}

				TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(ianaCode);
				DateTimeOffset localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZoneInfo);

				return new TimeInfo(
					Success: true,
					LocalTime: localTime,
					TimeZoneName: timeZoneInfo.DisplayName,
					TimeZoneId: timeZoneInfo.Id,
					UtcOffset: localTime.Offset,
					ErrorMessage: null
				);
			} catch (TimeZoneNotFoundException) {
				return new TimeInfo(
					Success: false,
					LocalTime: default,
					TimeZoneName: null,
					TimeZoneId: null,
					UtcOffset: default,
					ErrorMessage: $"Could not find time zone for '{cityOrTimeZone}'. Try using IANA format like 'Asia/Jakarta' or supported city names."
				);
			} catch (Exception ex) {
				return new TimeInfo(
					Success: false,
					LocalTime: default,
					TimeZoneName: null,
					TimeZoneId: null,
					UtcOffset: default,
					ErrorMessage: $"Error: {ex.Message}"
				);
			}
		}

		private static string? FindIanaByCity(string cityName) {
			// Get all IANA timezone IDs from TimeZoneConverter
			IEnumerable<string> allTimeZoneIds = TZConvert.KnownIanaTimeZoneNames;

			// Try exact match in the last part of IANA ID (e.g., "Asia/Jakarta" matches "Jakarta")
			string? exactMatch = allTimeZoneIds.FirstOrDefault(id => {
				string[] parts = id.Split('/');
				return parts.Length > 1 && parts[^1].Equals(cityName, StringComparison.OrdinalIgnoreCase);
			});

			if (exactMatch != null) return exactMatch;

			// Try partial match with underscore replacement (e.g., "New York" -> "New_York")
			string cityWithUnderscore = cityName.Replace(' ', '_');
			string? underscoreMatch = allTimeZoneIds.FirstOrDefault(id => {
				string[] parts = id.Split('/');
				return parts.Length > 1 && parts[^1].Equals(cityWithUnderscore, StringComparison.OrdinalIgnoreCase);
			});

			if (underscoreMatch != null) return underscoreMatch;

			// Try matching with hyphen to underscore conversion (e.g., "Porto-Novo" -> "Porto-Novo")
			string cityWithHyphen = cityName.Replace(' ', '-');
			string? hyphenMatch = allTimeZoneIds.FirstOrDefault(id => {
				string[] parts = id.Split('/');
				return parts.Length > 1 && parts[^1].Equals(cityWithHyphen, StringComparison.OrdinalIgnoreCase);
			});

			if (hyphenMatch != null) return hyphenMatch;

			// Try matching with space removed (e.g., "Comod Rivadavia" -> "ComodRivadavia")
			string cityNoSpace = cityName.Replace(" ", "");
			string? noSpaceMatch = allTimeZoneIds.FirstOrDefault(id => {
				string[] parts = id.Split('/');
				return parts.Length > 1 && parts[^1].Equals(cityNoSpace, StringComparison.OrdinalIgnoreCase);
			});

			if (noSpaceMatch != null) return noSpaceMatch;

			// Try matching anywhere in the path for multi-part names (e.g., "North Dakota" -> "America/North_Dakota/Center")
			// But only if all matches have the same UTC offset
			string cityForPathMatch = cityName.Replace(' ', '_');
			List<string> pathMatches = allTimeZoneIds
				.Where(id => id.Contains(cityForPathMatch, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (pathMatches.Count > 0) {
				// Check if all matches have the same UTC offset
				try {
					TimeZoneInfo firstTimeZone = TZConvert.GetTimeZoneInfo(pathMatches[0]);
					TimeSpan firstOffset = firstTimeZone.GetUtcOffset(DateTimeOffset.UtcNow);

					bool allSameOffset = pathMatches.All(id => {
						try {
							TimeZoneInfo tz = TZConvert.GetTimeZoneInfo(id);
							return tz.GetUtcOffset(DateTimeOffset.UtcNow) == firstOffset;
						} catch {
							return false;
						}
					});

					if (allSameOffset) {
						return pathMatches[0];
					}
				} catch {
					// If we can't verify offsets, don't return a match
				}
			}

			return null;
		}

		private static string? TryParseUtcOffset(string input) {
			// Match patterns like "GMT+7", "UTC+7", "GMT-5", "UTC-5", "+7", "-5", "+7:00", "-5:00"
			string trimmed = input.Trim();
			if (trimmed.Length < 2) return null;
			
			int hours = 0;
			
			// Check for GMT/UTC prefix
			if (trimmed.Length >= 5) {
				string prefix = trimmed.Substring(0, 3).ToUpperInvariant();
				if (prefix == "GMT" || prefix == "UTC") {
					string offsetPart = trimmed.Substring(3);
					if (string.IsNullOrWhiteSpace(offsetPart)) return null;
					
					// Parse offset (e.g., "+7" or "+7:00")
					if (TryParseOffset(offsetPart, out hours)) {
						return MapHoursToIana(hours);
					}
					return null;
				}
			}
			
			// Check for shorthand format like "+7" or "+7:00"
			if (trimmed[0] == '+' || trimmed[0] == '-') {
				if (TryParseOffset(trimmed, out hours)) {
					return MapHoursToIana(hours);
				}
			}
			
			return null;
		}

		private static bool TryParseOffset(string offset, out int hours) {
			hours = 0;
			
			// Remove any whitespace
			offset = offset.Trim();
			if (offset.Length == 0) return false;
			
			// Check for HH:MM format (e.g., "+7:00" or "-5:30")
			if (offset.Contains(':')) {
				string[] parts = offset.Split(':');
				if (parts.Length != 2) return false;
				
				if (int.TryParse(parts[0], out hours)) {
					// We only support full hour offsets for now
					if (int.TryParse(parts[1], out int minutes) && minutes == 0) {
						return true;
					}
				}
				return false;
			}
			
			// Simple integer format (e.g., "+7" or "-5")
			return int.TryParse(offset, out hours);
		}

		private static string? MapHoursToIana(int hours) {
			return hours switch {
				-12 => "Etc/GMT+12",
				-11 => "Pacific/Midway",
				-10 => "Pacific/Honolulu",
				-9 => "America/Anchorage",
				-8 => "America/Los_Angeles",
				-7 => "America/Denver",
				-6 => "America/Chicago",
				-5 => "America/New_York",
				-4 => "America/Halifax",
				-3 => "America/Sao_Paulo",
				-2 => "Atlantic/South_Georgia",
				-1 => "Atlantic/Azores",
				0 => "UTC",
				1 => "Europe/Paris",
				2 => "Europe/Athens",
				3 => "Europe/Moscow",
				4 => "Asia/Dubai",
				5 => "Asia/Karachi",
				6 => "Asia/Dhaka",
				7 => "Asia/Jakarta",
				8 => "Asia/Singapore",
				9 => "Asia/Tokyo",
				10 => "Australia/Sydney",
				11 => "Pacific/Noumea",
				12 => "Pacific/Fiji",
				_ => null
			};
		}
	}
	public record TimeInfo(
		bool Success,
		DateTimeOffset LocalTime,
		string? TimeZoneName,
		string? TimeZoneId,
		TimeSpan UtcOffset,
		string? ErrorMessage
	);
}
