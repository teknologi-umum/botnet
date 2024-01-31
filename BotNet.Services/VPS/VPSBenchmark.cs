using System;
using BotNet.Services.GoogleSheets;

namespace BotNet.Services.VPS {
	public sealed record VPSBenchmark(
		[property: FromColumn("A")] string Provider,
		[property: FromColumn("B")] string Location,
		[property: FromColumn("C")] DateOnly BenchmarkDate,
		[property: FromColumn("E")] string? VerdictCons,
		[property: FromColumn("F")] decimal IdrMo,
		[property: FromColumn("G")] int Core,
		[property: FromColumn("H")] int SsdGb,
		[property: FromColumn("I")] int RamMb,
		[property: FromColumn("J")] int IoMbs,
		[property: FromColumn("K")] double? ToCacheFlyMbs,
		[property: FromColumn("L")] double? ToHkCnMbs,
		[property: FromColumn("M")] double? ToLinodeJpMbs,
		[property: FromColumn("N")] double? ToLinodeSgMbs,
		[property: FromColumn("O")] double? ToLinodeUkMbs,
		[property: FromColumn("P")] double? ToLinodeCaMbs,
		[property: FromColumn("R")] double? BzipSec,
		[property: FromColumn("S")] double? DlMbs,
		[property: FromColumn("T")] double? AvgMbs
	);
}
