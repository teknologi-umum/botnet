// ReSharper disable InconsistentNaming
namespace BotNet.Services.Weather.Models {
	public class Current {
		public string? Last_Updated { get; set; }
		public double Temp_C { get; set; }
		public double Wind_Kph { get; set; }
		public Condition? Condition { get; set; }
	}
}
