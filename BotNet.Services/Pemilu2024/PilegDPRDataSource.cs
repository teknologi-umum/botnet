using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Pemilu2024 {
	public sealed class PilegDPRDataSource(
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
			CREATE TABLE pileg_dpr (
				provinsi VARCHAR(50) PRIMARY KEY,
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
				partai_ummat INTEGER
			)
			""");

			IList<Wilayah> listProvinsi = await _sirekapClient.GetPronvisiListAsync(cancellationToken);
			Dictionary<string, Wilayah> provinsiByKode = listProvinsi.ToDictionary(
				keySelector: provinsi => provinsi.Kode
			);

			ReportPilegDPR report = await _sirekapClient.GetReportPilegDPRAsync(cancellationToken);

			foreach ((string kodeWilayah, ReportPilegDPR.Row row) in report.RowByKodeWilayah.OrderBy(pair => pair.Key)) {
				_scopedDatabase.ExecuteNonQuery("""
				INSERT INTO pileg_dpr (provinsi, progress, pkb, gerindra, pdip, golkar, nasdem, partai_buruh, gelora, pks, pkn, hanura, garuda, pan, pbb, demokrat, psi, perindo, ppp, pna, gabthat, pda, partai_aceh, pas_aceh, partai_sira, partai_ummat)
				VALUES (@provinsi, @progress, @pkb, @gerindra, @pdip, @golkar, @nasdem, @partai_buruh, @gelora, @pks, @pkn, @hanura, @garuda, @pan, @pbb, @demokrat, @psi, @perindo, @ppp, @pna, @gabthat, @pda, @partai_aceh, @pas_aceh, @partai_sira, @partai_ummat)
				""",
					[
						( "@provinsi", provinsiByKode[kodeWilayah].Nama ),
						( "@progress", row.Persen ),
						( "@pkb", row.VotesByKodePartai!.TryGetValue(PKB, out int pkb) ? pkb : null),
						( "@gerindra", row.VotesByKodePartai!.TryGetValue(GERINDRA, out int gerindra) ? gerindra : null),
						( "@pdip", row.VotesByKodePartai!.TryGetValue(PDIP, out int pdip) ? pdip : null),
						( "@golkar", row.VotesByKodePartai!.TryGetValue(GOLKAR, out int golkar) ? golkar : null),
						( "@nasdem", row.VotesByKodePartai!.TryGetValue(NASDEM, out int nasdem) ? nasdem : null),
						( "@partai_buruh", row.VotesByKodePartai!.TryGetValue(PARTAI_BURUH, out int partai_buruh) ? partai_buruh : null),
						( "@gelora", row.VotesByKodePartai!.TryGetValue(GELORA, out int gelora) ? gelora : null),
						( "@pks", row.VotesByKodePartai!.TryGetValue(PKS, out int pks) ? pks : null),
						( "@pkn", row.VotesByKodePartai!.TryGetValue(PKN, out int pkn) ? pkn : null),
						( "@hanura", row.VotesByKodePartai!.TryGetValue(HANURA, out int hanura) ? hanura : null),
						( "@garuda", row.VotesByKodePartai!.TryGetValue(GARUDA, out int garuda) ? garuda : null),
						( "@pan", row.VotesByKodePartai!.TryGetValue(PAN, out int pan) ? pan : null),
						( "@pbb", row.VotesByKodePartai!.TryGetValue(PBB, out int pbb) ? pbb : null),
						( "@demokrat", row.VotesByKodePartai!.TryGetValue(DEMOKRAT, out int demokrat) ? demokrat : null),
						( "@psi", row.VotesByKodePartai!.TryGetValue(PSI, out int psi) ? psi : null),
						( "@perindo", row.VotesByKodePartai!.TryGetValue(PERINDO, out int perindo) ? perindo : null),
						( "@ppp", row.VotesByKodePartai!.TryGetValue(PPP, out int ppp) ? ppp : null),
						( "@pna", row.VotesByKodePartai!.TryGetValue(PNA, out int pna) ? pna : null),
						( "@gabthat", row.VotesByKodePartai!.TryGetValue(GABTHAT, out int gabthat) ? gabthat : null),
						( "@pda", row.VotesByKodePartai!.TryGetValue(PDA, out int pda) ? pda : null),
						( "@partai_aceh", row.VotesByKodePartai!.TryGetValue(PARTAI_ACEH, out int partai_aceh) ? partai_aceh : null),
						( "@pas_aceh", row.VotesByKodePartai!.TryGetValue(PAS_ACEH, out int pas_aceh) ? pas_aceh : null),
						( "@partai_sira", row.VotesByKodePartai!.TryGetValue(PARTAI_SIRA, out int partai_sira) ? partai_sira : null),
						( "@partai_ummat", row.VotesByKodePartai!.TryGetValue(PARTAI_UMMAT, out int partai_ummat) ? partai_ummat : null)
					]
				);
			}
		}
	}
}
