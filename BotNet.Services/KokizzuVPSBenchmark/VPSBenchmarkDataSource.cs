using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.GoogleSheets;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;

namespace BotNet.Services.KokizzuVPSBenchmark {
	public sealed class VPSBenchmarkDataSource(
		GoogleSheetsClient googleSheetsClient,
		ScopedDatabase scopedDatabase
	) : IScopedDataSource {
		private readonly GoogleSheetsClient _googleSheetsClient = googleSheetsClient;
		private readonly ScopedDatabase _scopedDatabase = scopedDatabase;

		public async Task LoadTableAsync(CancellationToken cancellationToken) {
			_scopedDatabase.ExecuteNonQuery("""
			CREATE TABLE vps (
				Provider TEXT,
				Location VARCHAR(2),
				BenchmarkDate VARCHAR(10),
				VerdictCons TEXT,
				IdrMo REAL,
				Core INTEGER,
				SsdGb INTEGER,
				RamMb INTEGER,
				IoMbs INTEGER,
				ToCacheFlyMbs REAL,
				ToHkCnMbs REAL,
				ToLinodeJpMbs REAL,
				ToLinodeSgMbs REAL,
				ToLinodeUkMbs REAL,
				ToLinodeCaMbs REAL,
				BzipSec REAL,
				DlMbs REAL,
				AvgMbs REAL
			)
			""");

			ImmutableList<VPSBenchmark> data = await _googleSheetsClient.GetDataAsync<VPSBenchmark>(
				// Source: https://docs.google.com/spreadsheets/d/14nAIFzIzkQuSxiayhc5tSFWFCWFncrV-GCA3Q5BbS4g/edit#gid=0
				spreadsheetId: "14nAIFzIzkQuSxiayhc5tSFWFCWFncrV-GCA3Q5BbS4g",
				range: "'Result'!A3:T",
				firstColumn: "A",
				cancellationToken: cancellationToken
			);

			foreach (VPSBenchmark vpsBenchmark in data) {
				_scopedDatabase.ExecuteNonQuery($"""
				INSERT INTO vps (
					Provider,
					Location,
					BenchmarkDate,
					VerdictCons,
					IdrMo,
					Core,
					SsdGb,
					RamMb,
					IoMbs,
					ToCacheFlyMbs,
					ToHkCnMbs,
					ToLinodeJpMbs,
					ToLinodeSgMbs,
					ToLinodeUkMbs,
					ToLinodeCaMbs,
					BzipSec,
					DlMbs,
					AvgMbs
				) VALUES (
					@Provider,
					@Location,
					@BenchmarkDate,
					@VerdictCons,
					@IdrMo,
					@Core,
					@SsdGb,
					@RamMb,
					@IoMbs,
					@ToCacheFlyMbs,
					@ToHkCnMbs,
					@ToLinodeJpMbs,
					@ToLinodeSgMbs,
					@ToLinodeUkMbs,
					@ToLinodeCaMbs,
					@BzipSec,
					@DlMbs,
					@AvgMbs
				)
				""",
					[
						("@Provider", vpsBenchmark.Provider),
						("@Location", vpsBenchmark.Location),
						("@BenchmarkDate", vpsBenchmark.BenchmarkDate),
						("@VerdictCons", vpsBenchmark.VerdictCons),
						("@IdrMo", vpsBenchmark.IdrMo),
						("@Core", vpsBenchmark.Core),
						("@SsdGb", vpsBenchmark.SsdGb),
						("@RamMb", vpsBenchmark.RamMb),
						("@IoMbs", vpsBenchmark.IoMbs),
						("@ToCacheFlyMbs", vpsBenchmark.ToCacheFlyMbs),
						("@ToHkCnMbs", vpsBenchmark.ToHkCnMbs),
						("@ToLinodeJpMbs", vpsBenchmark.ToLinodeJpMbs),
						("@ToLinodeSgMbs", vpsBenchmark.ToLinodeSgMbs),
						("@ToLinodeUkMbs", vpsBenchmark.ToLinodeUkMbs),
						("@ToLinodeCaMbs", vpsBenchmark.ToLinodeCaMbs),
						("@BzipSec", vpsBenchmark.BzipSec),
						("@DlMbs", vpsBenchmark.DlMbs),
						("@AvgMbs", vpsBenchmark.AvgMbs)
					]
				);
			}
		}
	}
}
