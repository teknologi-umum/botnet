using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Antutu.Models;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.Antutu {
	public sealed class AntutuAndroidDataSource(
		AntutuScraper antutuScraper,
		ScopedDatabase scopedDatabase
	) : IScopedDataSource {
		private readonly AntutuScraper _antutuScraper = antutuScraper;
		private readonly ScopedDatabase _scopedDatabase = scopedDatabase;

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			_scopedDatabase.ExecuteNonQuery("""
			CREATE TABLE antutu_android (
				device VARCHAR(50),
				cpu INTEGER,
				gpu INTEGER,
				mem INTEGER,
				ux INTEGER,
				total INTEGER
			)
			""");

			ImmutableList<AntutuBenchmarkData> data = await _antutuScraper.GetAndroidRankingAsync(cancellationToken);

			foreach (AntutuBenchmarkData antutuBenchmarkData in data) {
				_scopedDatabase.ExecuteNonQuery($"""
				INSERT INTO antutu_android (
					device,
					cpu,
					gpu,
					mem,
					ux,
					total
				) VALUES (
					@Device,
					@Cpu,
					@Gpu,
					@Mem,
					@Ux,
					@Total
				)
				""",
					[
						("@Device", antutuBenchmarkData.Device),
						("@Cpu", antutuBenchmarkData.Cpu),
						("@Gpu", antutuBenchmarkData.Gpu),
						("@Mem", antutuBenchmarkData.Mem),
						("@Ux", antutuBenchmarkData.Ux),
						("@Total", antutuBenchmarkData.Total)
					]
				);
			}
		}
	}
}
