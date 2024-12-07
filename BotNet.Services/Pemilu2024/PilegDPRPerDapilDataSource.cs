using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Pemilu2024 {
	public sealed class PilegDprPerDapilDataSource(
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

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			scopedDatabase.ExecuteNonQuery("""
			CREATE TABLE pileg_dpr_dapil (
				kode_dapil VARCHAR(5) PRIMARY KEY,
				dapil VARCHAR(50),
				progress REAL,
				pkb INTEGER,
				gerindra INTEGER,
				pdip INTEGER,
				golkar INTEGER,
				nasdem INTEGER,
				partai_buruh INTEGER,
				gelora INTEGER,
				pks INTEGER,
				pkn INTEGER,
				hanura INTEGER,
				garuda INTEGER,
				pan INTEGER,
				pbb INTEGER,
				demokrat INTEGER,
				psi INTEGER,
				perindo INTEGER,
				ppp INTEGER,
				pna INTEGER,
				gabthat INTEGER,
				pda INTEGER,
				partai_aceh INTEGER,
				pas_aceh INTEGER,
				partai_sira INTEGER,
				partai_ummat INTEGER,
				total INTEGER
			)
			""");

			IList<Wilayah> listDapilDpr = await sirekapClient.GetDapilDprListAsync(cancellationToken);
			Dictionary<string, Wilayah> dapilByKode = listDapilDpr.ToDictionary(
				keySelector: dapil => dapil.Kode
			);

			ReportPilegDprByDapil report = await sirekapClient.GetReportPilegDprByDapilAsync(cancellationToken);

			foreach ((string kodeDapil, ReportPilegDprByDapil.Row? row) in report.RowByKodeDapil.OrderBy(pair => pair.Key)) {
				if (row == null) {
					scopedDatabase.ExecuteNonQuery("""
					INSERT INTO pileg_dpr_dapil (kode_dapil, dapil, progress, pkb, gerindra, pdip, golkar, nasdem, partai_buruh, gelora, pks, pkn, hanura, garuda, pan, pbb, demokrat, psi, perindo, ppp, pna, gabthat, pda, partai_aceh, pas_aceh, partai_sira, partai_ummat, total)
					VALUES (@kode_dapil, @dapil, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)
					""",
						[
							( "@kode_dapil", kodeDapil ),
							( "@dapil", dapilByKode[kodeDapil].Nama )
						]
					);
					continue;
				}

				int? pkb = row.VotesByKodePartai!.TryGetValue(Pkb, out int p) ? p : null;
				int? gerindra = row.VotesByKodePartai!.TryGetValue(Gerindra, out int g) ? g : null;
				int? pdip = row.VotesByKodePartai!.TryGetValue(Pdip, out int pd) ? pd : null;
				int? golkar = row.VotesByKodePartai!.TryGetValue(Golkar, out int go) ? go : null;
				int? nasdem = row.VotesByKodePartai!.TryGetValue(Nasdem, out int n) ? n : null;
				int? partaiBuruh = row.VotesByKodePartai!.TryGetValue(PartaiBuruh, out int pb) ? pb : null;
				int? gelora = row.VotesByKodePartai!.TryGetValue(Gelora, out int ge) ? ge : null;
				int? pks = row.VotesByKodePartai!.TryGetValue(Pks, out int pk) ? pk : null;
				int? pkn = row.VotesByKodePartai!.TryGetValue(Pkn, out int pn) ? pn : null;
				int? hanura = row.VotesByKodePartai!.TryGetValue(Hanura, out int h) ? h : null;
				int? garuda = row.VotesByKodePartai!.TryGetValue(Garuda, out int ga) ? ga : null;
				int? pan = row.VotesByKodePartai!.TryGetValue(Pan, out int pa) ? pa : null;
				int? pbb = row.VotesByKodePartai!.TryGetValue(Pbb, out int pb2) ? pb2 : null;
				int? demokrat = row.VotesByKodePartai!.TryGetValue(Demokrat, out int d) ? d : null;
				int? psi = row.VotesByKodePartai!.TryGetValue(Psi, out int ps) ? ps : null;
				int? perindo = row.VotesByKodePartai!.TryGetValue(Perindo, out int pe) ? pe : null;
				int? ppp = row.VotesByKodePartai!.TryGetValue(Ppp, out int pp) ? pp : null;
				int? pna = row.VotesByKodePartai!.TryGetValue(Pna, out int pn2) ? pn2 : null;
				int? gabthat = row.VotesByKodePartai!.TryGetValue(Gabthat, out int gab) ? gab : null;
				int? pda = row.VotesByKodePartai!.TryGetValue(Pda, out int pd2) ? pd2 : null;
				int? partaiAceh = row.VotesByKodePartai!.TryGetValue(PartaiAceh, out int pa2) ? pa2 : null;
				int? pasAceh = row.VotesByKodePartai!.TryGetValue(PasAceh, out int pas) ? pas : null;
				int? partaiSira = row.VotesByKodePartai!.TryGetValue(PartaiSira, out int s) ? s : null;
				int? partaiUmmat = row.VotesByKodePartai!.TryGetValue(PartaiUmmat, out int u) ? u : null;
				int total = (pkb ?? 0) + (gerindra ?? 0) + (pdip ?? 0) + (golkar ?? 0) + (nasdem ?? 0) + (partaiBuruh ?? 0) + (gelora ?? 0) + (pks ?? 0) + (pkn ?? 0) + (hanura ?? 0) + (garuda ?? 0) + (pan ?? 0) + (pbb ?? 0) + (demokrat ?? 0) + (psi ?? 0) + (perindo ?? 0) + (ppp ?? 0) + (pna ?? 0) + (gabthat ?? 0) + (pda ?? 0) + (partaiAceh ?? 0) + (pasAceh ?? 0) + (partaiSira ?? 0) + (partaiUmmat ?? 0);
				scopedDatabase.ExecuteNonQuery("""
				INSERT INTO pileg_dpr_dapil (kode_dapil, dapil, progress, pkb, gerindra, pdip, golkar, nasdem, partai_buruh, gelora, pks, pkn, hanura, garuda, pan, pbb, demokrat, psi, perindo, ppp, pna, gabthat, pda, partai_aceh, pas_aceh, partai_sira, partai_ummat, total)
				VALUES (@kode_dapil, @dapil, @progress, @pkb, @gerindra, @pdip, @golkar, @nasdem, @partai_buruh, @gelora, @pks, @pkn, @hanura, @garuda, @pan, @pbb, @demokrat, @psi, @perindo, @ppp, @pna, @gabthat, @pda, @partai_aceh, @pas_aceh, @partai_sira, @partai_ummat, @total)
				""",
					[
						( "@kode_dapil", kodeDapil ),
						( "@dapil", dapilByKode[kodeDapil].Nama ),
						( "@progress", row.Persen ),
						( "@pkb", pkb),
						( "@gerindra", gerindra),
						( "@pdip", pdip),
						( "@golkar", golkar),
						( "@nasdem", nasdem),
						( "@partai_buruh", partaiBuruh),
						( "@gelora", gelora),
						( "@pks", pks),
						( "@pkn", pkn),
						( "@hanura", hanura),
						( "@garuda", garuda),
						( "@pan", pan),
						( "@pbb", pbb),
						( "@demokrat", demokrat),
						( "@psi", psi),
						( "@perindo", perindo),
						( "@ppp", ppp),
						( "@pna", pna),
						( "@gabthat", gabthat),
						( "@pda", pda),
						( "@partai_aceh", partaiAceh),
						( "@pas_aceh", pasAceh),
						( "@partai_sira", partaiSira),
						( "@partai_ummat", partaiUmmat),
						( "@total", total)
					]
				);
			}
		}
	}
}
