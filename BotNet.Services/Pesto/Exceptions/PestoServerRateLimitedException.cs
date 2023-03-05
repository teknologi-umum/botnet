namespace BotNet.Services.Pesto.Exceptions; 

public class PestoServerRateLimitedException : System.Exception {
	public PestoServerRateLimitedException() : base("Server rate limited") { }
}
