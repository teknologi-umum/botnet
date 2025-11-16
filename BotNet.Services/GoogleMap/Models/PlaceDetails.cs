namespace BotNet.Services.GoogleMap.Models {
	/// <summary>
	/// Detailed information about a place from Google Places API
	/// </summary>
	public class PlaceDetails {
		/// <summary>
		/// The human-readable name of the place
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// The human-readable address
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public string? Formatted_Address { get; set; }

		/// <summary>
		/// The place's rating from 1.0 to 5.0
		/// </summary>
		public decimal? Rating { get; set; }

		/// <summary>
		/// Total number of user ratings
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public int? User_Ratings_Total { get; set; }

		/// <summary>
		/// Phone number in local format
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public string? Formatted_Phone_Number { get; set; }

		/// <summary>
		/// Phone number in international format
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public string? International_Phone_Number { get; set; }

		/// <summary>
		/// The authoritative website for this place
		/// </summary>
		public string? Website { get; set; }

		/// <summary>
		/// Opening hours information
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public OpeningHours? Opening_Hours { get; set; }

		/// <summary>
		/// Editorial summary of the place
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public EditorialSummary? Editorial_Summary { get; set; }

		/// <summary>
		/// Price level from 0 (free) to 4 (very expensive)
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public int? Price_Level { get; set; }

		/// <summary>
		/// URL of the official Google page for this place
		/// </summary>
		public string? Url { get; set; }

		/// <summary>
		/// Indicates if the business is operational, closed temporarily, or closed permanently
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public string? Business_Status { get; set; }
	}

	/// <summary>
	/// Opening hours information for a place
	/// </summary>
	public class OpeningHours {
		/// <summary>
		/// Whether the place is currently open
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public bool? Open_Now { get; set; }

		/// <summary>
		/// Human-readable opening hours text for each day
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public string[]? Weekday_Text { get; set; }
	}

	/// <summary>
	/// Editorial summary of the place
	/// </summary>
	public class EditorialSummary {
		/// <summary>
		/// Textual overview of the place
		/// </summary>
		public string? Overview { get; set; }

		/// <summary>
		/// Language code of the summary
		/// </summary>
		public string? Language { get; set; }
	}

	/// <summary>
	/// Response from Google Places API
	/// </summary>
	public class PlaceDetailsResponse {
		/// <summary>
		/// The place details
		/// </summary>
		public PlaceDetails? Result { get; set; }

		/// <summary>
		/// Status of the API response
		/// </summary>
		public string? Status { get; set; }

		/// <summary>
		/// HTML attributions that must be displayed
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public string[]? Html_Attributions { get; set; }
	}
}
