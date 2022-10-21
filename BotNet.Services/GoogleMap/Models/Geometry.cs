namespace BotNet.Services.GoogleMap.Models {
	public class Geometry {

		public Coordinate? Location{ get; set; }

		public string? Location_Type { get; set; }

		public class Coordinate {
			public double Lat { get; set; }
			public double Lng { get; set; }
		}
	}
}
