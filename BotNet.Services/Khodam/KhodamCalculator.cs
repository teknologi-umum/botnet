using System;

namespace BotNet.Services.Khodam {
	public static class KhodamCalculator {
		private static readonly string[] Animals = [
			"Anjing",
			"Ayam",
			"Bebek",
			"Beruang",
			"Buaya",
			"Elang",
			"Gajah",
			"Harimau",
			"Ikan Lohan",
			"Kadal",
			"Kalajengking",
			"Kambing",
			"Katak",
			"Kucing",
			"Kuda",
			"Lumba-Lumba",
			"Monyet",
			"Naga",
			"Serigala",
			"Singa",
			"Ular",
		];

		private static readonly string[] Adjectives = [
			"Birahi",
			"Hitam",
			"Hutan",
			"Jawa",
			"Kalimantan",
			"Ngawi",
			"Nolep",
			"Pelangi",
			"Pemalas",
			"Pemarah",
			"Putih",
			"Sakti",
			"Sumatera",
			"Sunda",
		];

		private static readonly string[] Rares = [
			"Ayam Geprek",
			"Ban Serep",
			"Bintang Laut",
			"Bubur Ayam",
			"Dewi Bulan",
			"Es Cendol",
			"Gado-Gado",
			"Gitar Spanyol",
			"Gule Kambing",
			"Ikan Bakar",
			"Kambing Guling",
			"Kang Parkir Indomaret",
			"Klepon",
			"Kopi Hitam",
			"Kulit Pisang",
			"Lontong Balap",
			"Mie Ayam",
			"Nasi Padang",
			"Pempek Palembang",
			"Penjaga Hutan",
			"Pisang Goreng",
			"Raja Jin",
			"Ratu Pantai Selatan",
			"Rawon",
			"Rengginang",
			"Risoles",
			"Sate Ayam",
			"Sate Kambing",
			"Sendal Jepit",
			"Sop Buntut",
			"Soto Ayam",
			"Supra Geter",
			"Susu Jahe",
			"Tahu Bulat",
			"Tiang Listrik",
			"Ulat Keket",
			"Wayang Kulit",
		];

		public static string CalculateKhodam(string name, long userId) {
			int hashCode = Math.Abs(HashCode.Combine(
				value1: DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).Date,
				value2: name,
				value3: userId
			));

			// Kosong vs isi
			if (hashCode % 20 == 13) {
				return "Kosong";
			}

			// Rare
			if (hashCode % 631 > 580) {
				return Rares[hashCode % Rares.Length];
			}

			// Animals
			return $"{Animals[hashCode % Animals.Length]} {Adjectives[hashCode % Adjectives.Length]}";
		}
	}
}
