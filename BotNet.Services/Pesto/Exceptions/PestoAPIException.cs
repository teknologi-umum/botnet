namespace BotNet.Services.Pesto.Exceptions; 

public class PestoApiException(
	string? message
) : System.Exception(message) {
	public PestoApiException() : this("Unhandled exception with empty message") { }
}
