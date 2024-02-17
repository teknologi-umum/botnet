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
				ganjar INTEGER
			)
			""");

			IList<Wilayah> listProvinsi = await _sirekapClient.GetPronvisiListAsync(cancellationToken);
			Dictionary<string, Wilayah> provinsiByKode = listProvinsi.ToDictionary(
				keySelector: provinsi => provinsi.Kode
			);

			ReportPilpres report = await _sirekapClient.GetReportPilpresAsync(cancellationToken);

			foreach ((string kodeWilayah, ReportPilpres.Row row) in report.RowByKodeWilayah.OrderBy(pair => pair.Key)) {
				_scopedDatabase.ExecuteNonQuery("""
				INSERT INTO pilpres (provinsi, progress, anies, prabowo, ganjar)
				VALUES (@provinsi, @progress, @anies, @prabowo, @ganjar)
				""",
					[
						( "@provinsi", provinsiByKode[kodeWilayah].Nama ),
						( "@progress", row.Persen ),
						( "@anies", row.VotesByKodeCalon!.TryGetValue(ANIES, out int anies) ? anies : null),
						( "@prabowo", row.VotesByKodeCalon!.TryGetValue(PRABOWO, out int prabowo) ? prabowo : null),
						( "@ganjar", row.VotesByKodeCalon!.TryGetValue(GANJAR, out int ganjar) ? ganjar : null)
					]
				);
			}
		}
	}
}
