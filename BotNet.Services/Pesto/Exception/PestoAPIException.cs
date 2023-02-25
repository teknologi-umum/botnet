namespace BotNet.Services.Pesto.Exception; 

public class PestoAPIException : System.Exception {
	public PestoAPIException(string? message) : base(message) { }
	public PestoAPIException() : base("Unhandled exception with empty message") { }
}
