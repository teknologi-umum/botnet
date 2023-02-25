namespace BotNet.Services.Pesto.Exception; 

public class PestoEmptyCodeException : System.Exception{
	public PestoEmptyCodeException() : base ("Code parameter is empty") { }
}
