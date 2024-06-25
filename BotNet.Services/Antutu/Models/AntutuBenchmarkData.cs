namespace BotNet.Services.Antutu.Models {
	public readonly record struct AntutuBenchmarkData(
		string Device,
		int Cpu,
		int Gpu,
		int Mem,
		int Ux,
		int Total
	);
}
