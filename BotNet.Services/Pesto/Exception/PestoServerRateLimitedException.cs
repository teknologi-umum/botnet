namespace BotNet.Services.Pesto.Exception; 

public class PestoServerRateLimitedException : System.Exception {
	public PestoServerRateLimitedException() : base("Server rate limited") { }
}
