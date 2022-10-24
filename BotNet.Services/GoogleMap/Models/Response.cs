using System.Collections.Generic;

namespace BotNet.Services.GoogleMap.Models {
	
	/// <summary>
	/// Response for google map api geocoding
	/// </summary>
	public class Response {
		public List<Result>? Results { get; set; }
		public string? Status { get; set; }
	}
}
