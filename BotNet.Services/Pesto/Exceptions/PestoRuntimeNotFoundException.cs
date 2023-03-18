namespace BotNet.Services.Pesto.Exceptions; 

public class PestoRuntimeNotFoundException : System.Exception {
	public PestoRuntimeNotFoundException(string? runtime) 
		: base($"Runtime not found for {runtime ?? "current request"}") {	}
}
