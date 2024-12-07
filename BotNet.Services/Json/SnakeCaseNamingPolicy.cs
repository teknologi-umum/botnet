using System;
using System.Linq;
using System.Text.Json;

namespace BotNet.Services.Json {
	public class SnakeCaseNamingPolicy : JsonNamingPolicy {
		public override string ConvertName(string name) {
			if (name == "") return "";
			Span<char> nameSpan = stackalloc char[name.Length + name.Skip(1).Count(char.IsUpper)];
			int i = 0;
			nameSpan[i++] = char.ToLower(name[0]);
			foreach (char c in name.Skip(1)) {
				if (char.IsUpper(c)) {
					nameSpan[i++] = '_';
					nameSpan[i++] = char.ToLower(c);
				} else {
					nameSpan[i++] = c;
				}
			}

			return new string(nameSpan);
		}
	}
}
