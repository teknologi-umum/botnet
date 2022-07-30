using System;

namespace BotNet.Services.PSE.Models {
	public enum Domicile {
		Domestic,
		Foreign
	}

	public static class DomicileConverter {
		public static string ToPSEDomicile(this Domicile domicile) => domicile switch {
			Domicile.Domestic => "LOKAL",
			Domicile.Foreign => "ASING",
			_ => throw new ArgumentOutOfRangeException(nameof(domicile))
		};

		public static Domicile ToDomicileEnum(this string pseDomicile) => pseDomicile switch {
			"LOKAL" => Domicile.Domestic,
			"ASING" => Domicile.Foreign,
			_ => throw new ArgumentOutOfRangeException(nameof(pseDomicile))
		};
	}
}
