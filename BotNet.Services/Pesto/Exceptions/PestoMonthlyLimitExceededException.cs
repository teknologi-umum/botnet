namespace BotNet.Services.Pesto.Exceptions; 

public class PestoMonthlyLimitExceededException : System.Exception {
	public PestoMonthlyLimitExceededException() : base("Monthly limit exceeded for current token") {	}
}
