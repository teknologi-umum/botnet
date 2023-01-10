namespace BotNet.Services.BMKG {
	public record EarthQuake {
		public QuakeInfo InfoGempa { get; set; } = new QuakeInfo();

		public record QuakeInfo {
			public Quake Gempa { get; set; } = new Quake();

			public record Quake {
				public string Tanggal { get; set; } = string.Empty;
				public string Jam { get; set; } = string.Empty;
				public string Coordinates { get; set; } = string.Empty;
				public string Magnitude { get; set; } = string.Empty;
				public string Kedalaman { get; set; } = string.Empty;
				public string Wilayah { get; set; } = string.Empty;
				public string Potensi { get; set; } = string.Empty;
				public string Dirasakan { get; set; } = string.Empty;
				public string Shakemap{ get; set; } = string.Empty;
				public string ShakemapUrl {
					get => $"https://data.bmkg.go.id/DataMKG/TEWS/{Shakemap}";
					set { }
				}
			}
		}

	}
}
