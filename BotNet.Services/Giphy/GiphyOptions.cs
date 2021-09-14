namespace BotNet.Services.Giphy {
	public class GiphyOptions {
		public string? ApiKey { get; set; }
		public int ReadsPerHour { get; set; } = 42;
		public int CallsPerDay { get; set; } = 1000;
	}
}
