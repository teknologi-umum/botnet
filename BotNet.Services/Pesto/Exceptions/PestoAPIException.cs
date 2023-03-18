namespace BotNet.Services.Pesto.Exceptions; 

public class PestoAPIException : System.Exception {
	public PestoAPIException(string? message) : base(message) { }
	public PestoAPIException() : base("Unhandled exception with empty message") { }
}
