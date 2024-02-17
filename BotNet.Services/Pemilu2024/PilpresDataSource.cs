using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Pemilu2024 {
	public sealed class PilpresDataSource(
		ScopedDatabase scopedDatabase,
		SirekapClient sirekapClient
	) : IScopedDataSource {
		private const string ANIES = "100025";
		private const string PRABOWO = "100026";
		private const string GANJAR = "100027";
		private readonly ScopedDatabase _scopedDatabase = scopedDatabase;
		private readonly SirekapClient _sirekapClient = sirekapClient;

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			_scopedDatabase.ExecuteNonQuery("""
			CREATE TABLE pilpres (
				provinsi VARCHAR(50) PRIMARY KEY,
				progress REAL,
				anies INTEGER,
				prabowo INTEGER,
				ganjar INTEGER,
				total INTEGER
			)
			""");

			IList<Wilayah> listProvinsi = await _sirekapClient.GetPronvisiListAsync(cancellationToken);
			Dictionary<string, Wilayah> provinsiByKode = listProvinsi.ToDictionary(
				keySelector: provinsi => provinsi.Kode
			);

			ReportPilpres report = await _sirekapClient.GetReportPilpresAsync(cancellationToken);

			foreach ((string kodeWilayah, ReportPilpres.Row row) in report.RowByKodeWilayah.OrderBy(pair => pair.Key)) {
				int? anies = row.VotesByKodeCalon!.TryGetValue(ANIES, out int a) ? a : null;
				int? prabowo = row.VotesByKodeCalon!.TryGetValue(PRABOWO, out int p) ? p : null;
				int? ganjar = row.VotesByKodeCalon!.TryGetValue(GANJAR, out int g) ? g : null;
				int total = (anies ?? 0) + (prabowo ?? 0) + (ganjar ?? 0);

				_scopedDatabase.ExecuteNonQuery("""
				INSERT INTO pilpres (provinsi, progress, anies, prabowo, ganjar, total)
				VALUES (@provinsi, @progress, @anies, @prabowo, @ganjar, @total)
				""",
					[
						( "@provinsi", provinsiByKode[kodeWilayah].Nama ),
						( "@progress", row.Persen ),
						( "@anies", anies),
						( "@prabowo", prabowo),
						( "@ganjar", ganjar),
						( "@total", total)
					]
				);
			}
		}
	}
}
