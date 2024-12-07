using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Pemilu2024 {
	public sealed class PilegDprDapilDataSource(
		ScopedDatabase scopedDatabase,
		SirekapClient sirekapClient
	) : IScopedDataSource {
		private const string Pkb = "1";
		private const string Gerindra = "2";
		private const string Pdip = "3";
		private const string Golkar = "4";
		private const string Nasdem = "5";
		private const string PartaiBuruh = "6";
		private const string Gelora = "7";
		private const string Pks = "8";
		private const string Pkn = "9";
		private const string Hanura = "10";
		private const string Garuda = "11";
		private const string Pan = "12";
		private const string Pbb = "13";
		private const string Demokrat = "14";
		private const string Psi = "15";
		private const string Perindo = "16";
		private const string Ppp = "17";
		private const string Pna = "18";
		private const string Gabthat = "19";
		private const string Pda = "20";
		private const string PartaiAceh = "21";
		private const string PasAceh = "22";
		private const string PartaiSira = "23";
		private const string PartaiUmmat = "24";

		public string? KodeDapil { get; set; }

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			if (KodeDapil is null) throw new InvalidProgramException("KodeDapil is not set");

			scopedDatabase.ExecuteNonQuery($"""
			CREATE TABLE pileg_dpr_{KodeDapil} (
				partai VARCHAR(50),
				kode_caleg VARCHAR(10),
				nomor_urut INTEGER,
				nama VARCHAR(100),
				jenis_kelamin VARCHAR(1),
				tempat_tinggal VARCHAR(100),
				jumlah_suara INTEGER
			)
			""");

			IDictionary<string, IDictionary<string, Caleg>> calegByKodeByKodePartai = await sirekapClient.GetCalegByKodeByKodePartaiAsync(KodeDapil, cancellationToken);
			ReportCalegDpr report = await sirekapClient.GetReportCalegDprAsync(KodeDapil, cancellationToken);

			foreach ((string kodePartai, IDictionary<string, int> votesByKodeCaleg) in report.VotesByKodeCalegByKodePartai.OrderBy(pair => pair.Key)) {
				string partai = kodePartai switch {
					Pkb => "PKB",
					Gerindra => "Gerindra",
					Pdip => "PDIP",
					Golkar => "Golkar",
					Nasdem => "Nasdem",
					PartaiBuruh => "Partai Buruh",
					Gelora => "Gelora",
					Pks => "PKS",
					Pkn => "PKN",
					Hanura => "Hanura",
					Garuda => "Garuda",
					Pan => "PAN",
					Pbb => "PBB",
					Demokrat => "Demokrat",
					Psi => "PSI",
					Perindo => "Perindo",
					Ppp => "PPP",
					Pna => "Partai Nanggroe Aceh",
					Gabthat => "Partai Generasi Atjeh Beusaboh Tha'at Dan Taqwa",
					Pda => "Partai Darul Aceh",
					PartaiAceh => "Partai Aceh",
					PasAceh => "Partai Adil Sejahtera Aceh",
					PartaiSira => "Partai SIRA",
					PartaiUmmat => "Partai Ummat",
					_ => throw new InvalidProgramException("Unknown partai")
				};

				foreach ((string kodeCaleg, int votes) in votesByKodeCaleg.OrderBy(pair => pair.Key)) {
					if (!int.TryParse(kodeCaleg, out _)) continue;
					Caleg caleg = calegByKodeByKodePartai[kodePartai][kodeCaleg];
					scopedDatabase.ExecuteNonQuery($"""
					INSERT INTO pileg_dpr_{KodeDapil} (partai, kode_caleg, nomor_urut, nama, jenis_kelamin, tempat_tinggal, jumlah_suara)
					VALUES (@partai, @kode_caleg, @nomor_urut, @nama, @jenis_kelamin, @tempat_tinggal, @jumlah_suara)
					""",
						[
							( "@partai", partai ),
							( "@kode_caleg", kodeCaleg ),
							( "@nomor_urut", caleg.NomorUrut ),
							( "@nama", caleg.Nama ),
							( "@jenis_kelamin", caleg.JenisKelamin ),
							( "@tempat_tinggal", caleg.TempatTinggal ),
							( "@jumlah_suara", votes )
						]
					);
				}

				scopedDatabase.ExecuteNonQuery($$"""
				INSERT INTO pileg_dpr_{{KodeDapil}} (partai, kode_caleg, nomor_urut, nama, jenis_kelamin, tempat_tinggal, jumlah_suara)
				VALUES (@partai, null, null, 'Jumlah Suara Total', null, null, @jumlah_suara)
				""",
					[
						( "@partai", partai ),
						( "@jumlah_suara", votesByKodeCaleg["jml_suara_total"] )
					]
				);

				scopedDatabase.ExecuteNonQuery($$"""
				INSERT INTO pileg_dpr_{{KodeDapil}} (partai, kode_caleg, nomor_urut, nama, jenis_kelamin, tempat_tinggal, jumlah_suara)
				VALUES (@partai, null, null, 'Jumlah Suara Partai', null, null, @jumlah_suara)
				""",
					[
						( "@partai", partai ),
						( "@jumlah_suara", votesByKodeCaleg["jml_suara_partai"] )
					]
				);
			}
		}
	}
}
