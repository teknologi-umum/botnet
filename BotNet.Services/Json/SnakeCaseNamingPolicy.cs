using System.Linq;
using System.Text.Json;

namespace BotNet.Services.Json {
	public class SnakeCaseNamingPolicy : JsonNamingPolicy {
		public override string ConvertName(string name) {
			return string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())).ToLower();
		}
	}
}
