namespace BotNet.Services.Pesto.Exception; 

public class PestoRuntimeNotFoundException : System.Exception {
	public PestoRuntimeNotFoundException(string? runtime) 
		: base($"Runtime not found for {runtime ?? "current request"}") {	}
}
