namespace BotNet.Services.Pesto.Exceptions; 

public class PestoEmptyCodeException : System.Exception{
	public PestoEmptyCodeException() : base ("Code parameter is empty") { }
}
