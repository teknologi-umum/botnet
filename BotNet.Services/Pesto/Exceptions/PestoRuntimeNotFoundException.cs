namespace BotNet.Services.Pesto.Exceptions; 

public class PestoRuntimeNotFoundException(
	string? runtime
) : System.Exception($"Runtime not found for {runtime ?? "current request"}");
