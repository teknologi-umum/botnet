using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Pemilu2024 {
	public sealed class PilegDPRPerDapilDataSource(
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

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			_scopedDatabase.ExecuteNonQuery("""
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

			IList<Wilayah> listDapilDPR = await _sirekapClient.GetDapilDPRListAsync(cancellationToken);
			Dictionary<string, Wilayah> dapilByKode = listDapilDPR.ToDictionary(
				keySelector: dapil => dapil.Kode
			);

			ReportPilegDPRByDapil report = await _sirekapClient.GetReportPilegDPRByDapilAsync(cancellationToken);

			foreach ((string kodeDapil, ReportPilegDPRByDapil.Row? row) in report.RowByKodeDapil.OrderBy(pair => pair.Key)) {
				if (row == null) {
					_scopedDatabase.ExecuteNonQuery("""
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

				int? pkb = row.VotesByKodePartai!.TryGetValue(PKB, out int p) ? p : null;
				int? gerindra = row.VotesByKodePartai!.TryGetValue(GERINDRA, out int g) ? g : null;
				int? pdip = row.VotesByKodePartai!.TryGetValue(PDIP, out int pd) ? pd : null;
				int? golkar = row.VotesByKodePartai!.TryGetValue(GOLKAR, out int go) ? go : null;
				int? nasdem = row.VotesByKodePartai!.TryGetValue(NASDEM, out int n) ? n : null;
				int? partai_buruh = row.VotesByKodePartai!.TryGetValue(PARTAI_BURUH, out int pb) ? pb : null;
				int? gelora = row.VotesByKodePartai!.TryGetValue(GELORA, out int ge) ? ge : null;
				int? pks = row.VotesByKodePartai!.TryGetValue(PKS, out int pk) ? pk : null;
				int? pkn = row.VotesByKodePartai!.TryGetValue(PKN, out int pn) ? pn : null;
				int? hanura = row.VotesByKodePartai!.TryGetValue(HANURA, out int h) ? h : null;
				int? garuda = row.VotesByKodePartai!.TryGetValue(GARUDA, out int ga) ? ga : null;
				int? pan = row.VotesByKodePartai!.TryGetValue(PAN, out int pa) ? pa : null;
				int? pbb = row.VotesByKodePartai!.TryGetValue(PBB, out int pb2) ? pb2 : null;
				int? demokrat = row.VotesByKodePartai!.TryGetValue(DEMOKRAT, out int d) ? d : null;
				int? psi = row.VotesByKodePartai!.TryGetValue(PSI, out int ps) ? ps : null;
				int? perindo = row.VotesByKodePartai!.TryGetValue(PERINDO, out int pe) ? pe : null;
				int? ppp = row.VotesByKodePartai!.TryGetValue(PPP, out int pp) ? pp : null;
				int? pna = row.VotesByKodePartai!.TryGetValue(PNA, out int pn2) ? pn2 : null;
				int? gabthat = row.VotesByKodePartai!.TryGetValue(GABTHAT, out int gab) ? gab : null;
				int? pda = row.VotesByKodePartai!.TryGetValue(PDA, out int pd2) ? pd2 : null;
				int? partai_aceh = row.VotesByKodePartai!.TryGetValue(PARTAI_ACEH, out int pa2) ? pa2 : null;
				int? pas_aceh = row.VotesByKodePartai!.TryGetValue(PAS_ACEH, out int pas) ? pas : null;
				int? partai_sira = row.VotesByKodePartai!.TryGetValue(PARTAI_SIRA, out int s) ? s : null;
				int? partai_ummat = row.VotesByKodePartai!.TryGetValue(PARTAI_UMMAT, out int u) ? u : null;
				int total = (pkb ?? 0) + (gerindra ?? 0) + (pdip ?? 0) + (golkar ?? 0) + (nasdem ?? 0) + (partai_buruh ?? 0) + (gelora ?? 0) + (pks ?? 0) + (pkn ?? 0) + (hanura ?? 0) + (garuda ?? 0) + (pan ?? 0) + (pbb ?? 0) + (demokrat ?? 0) + (psi ?? 0) + (perindo ?? 0) + (ppp ?? 0) + (pna ?? 0) + (gabthat ?? 0) + (pda ?? 0) + (partai_aceh ?? 0) + (pas_aceh ?? 0) + (partai_sira ?? 0) + (partai_ummat ?? 0);
				_scopedDatabase.ExecuteNonQuery("""
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
						( "@partai_buruh", partai_buruh),
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
						( "@partai_aceh", partai_aceh),
						( "@pas_aceh", pas_aceh),
						( "@partai_sira", partai_sira),
						( "@partai_ummat", partai_ummat),
						( "@total", total)
					]
				);
			}
		}
	}
}
