using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Pemilu2024 {
	public sealed class PilegDPRDapilDataSource(
		ScopedDatabase scopedDatabase,
		SirekapClient sirekapClient
	) : IScopedDataSource {
		private const string PKB = "1";
		private const string GERINDRA = "2";
		private const string PDIP = "3";
		private const string GOLKAR = "4";
		private const string NASDEM = "5";
		private const string PARTAI_BURUH = "6";
		private const string GELORA = "7";
		private const string PKS = "8";
		private const string PKN = "9";
		private const string HANURA = "10";
		private const string GARUDA = "11";
		private const string PAN = "12";
		private const string PBB = "13";
		private const string DEMOKRAT = "14";
		private const string PSI = "15";
		private const string PERINDO = "16";
		private const string PPP = "17";
		private const string PNA = "18";
		private const string GABTHAT = "19";
		private const string PDA = "20";
		private const string PARTAI_ACEH = "21";
		private const string PAS_ACEH = "22";
		private const string PARTAI_SIRA = "23";
		private const string PARTAI_UMMAT = "24";
		private readonly ScopedDatabase _scopedDatabase = scopedDatabase;
		private readonly SirekapClient _sirekapClient = sirekapClient;

		public string? KodeDapil { get; set; }

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			if (KodeDapil is null) throw new InvalidProgramException("KodeDapil is not set");

			_scopedDatabase.ExecuteNonQuery($$"""
			CREATE TABLE pileg_dpr_{{KodeDapil}} (
				partai VARCHAR(50),
				kode_caleg VARCHAR(10),
				nomor_urut INTEGER,
				nama VARCHAR(100),
				jenis_kelamin VARCHAR(1),
				tempat_tinggal VARCHAR(100),
				jumlah_suara INTEGER
			)
			""");

			IDictionary<string, IDictionary<string, Caleg>> calegByKodeByKodePartai = await _sirekapClient.GetCalegByKodeByKodePartaiAsync(KodeDapil, cancellationToken);
			ReportCalegDPR report = await _sirekapClient.GetReportCalegDPRAsync(KodeDapil, cancellationToken);

			foreach ((string kodePartai, IDictionary<string, int> votesByKodeCaleg) in report.VotesByKodeCalegByKodePartai.OrderBy(pair => pair.Key)) {
				string partai = kodePartai switch {
					PKB => "PKB",
					GERINDRA => "Gerindra",
					PDIP => "PDIP",
					GOLKAR => "Golkar",
					NASDEM => "Nasdem",
					PARTAI_BURUH => "Partai Buruh",
					GELORA => "Gelora",
					PKS => "PKS",
					PKN => "PKN",
					HANURA => "Hanura",
					GARUDA => "Garuda",
					PAN => "PAN",
					PBB => "PBB",
					DEMOKRAT => "Demokrat",
					PSI => "PSI",
					PERINDO => "Perindo",
					PPP => "PPP",
					PNA => "Partai Nanggroe Aceh",
					GABTHAT => "Partai Generasi Atjeh Beusaboh Tha'at Dan Taqwa",
					PDA => "Partai Darul Aceh",
					PARTAI_ACEH => "Partai Aceh",
					PAS_ACEH => "Partai Adil Sejahtera Aceh",
					PARTAI_SIRA => "Partai SIRA",
					PARTAI_UMMAT => "Partai Ummat",
					_ => throw new InvalidProgramException("Unknown partai")
				};

				foreach ((string kodeCaleg, int votes) in votesByKodeCaleg!.OrderBy(pair => pair.Key)) {
					if (!int.TryParse(kodeCaleg, out _)) continue;
					Caleg caleg = calegByKodeByKodePartai[kodePartai][kodeCaleg];
					_scopedDatabase.ExecuteNonQuery($$"""
					INSERT INTO pileg_dpr_{{KodeDapil}} (partai, kode_caleg, nomor_urut, nama, jenis_kelamin, tempat_tinggal, jumlah_suara)
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

				_scopedDatabase.ExecuteNonQuery($$"""
				INSERT INTO pileg_dpr_{{KodeDapil}} (partai, kode_caleg, nomor_urut, nama, jenis_kelamin, tempat_tinggal, jumlah_suara)
				VALUES (@partai, null, null, 'Jumlah Suara Total', null, null, @jumlah_suara)
				""",
					[
						( "@partai", partai ),
						( "@jumlah_suara", votesByKodeCaleg["jml_suara_total"] )
					]
				);

				_scopedDatabase.ExecuteNonQuery($$"""
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
