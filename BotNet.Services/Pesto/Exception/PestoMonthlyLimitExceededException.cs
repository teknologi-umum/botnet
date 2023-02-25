using System;

namespace BotNet.Services.Pesto.Exception; 

public class PestoMonthlyLimitExceededException : System.Exception {
	public PestoMonthlyLimitExceededException() : base("Monthly limit exceeded for current token") {	}
}
