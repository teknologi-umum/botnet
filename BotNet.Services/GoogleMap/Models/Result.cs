namespace BotNet.Services.GoogleMap.Models {

	public class Result {
		// ReSharper disable once InconsistentNaming
		public string? Formatted_Address { get; set; }

		public Geometry? Geometry { get; set; }

		// ReSharper disable once InconsistentNaming
		public string[]? Types { get; set; }

		// ReSharper disable once InconsistentNaming
		public string? Place_Id { get; set; }
	}
}
