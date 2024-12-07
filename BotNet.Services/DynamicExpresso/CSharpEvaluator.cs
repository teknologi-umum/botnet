using DynamicExpresso;

namespace BotNet.Services.DynamicExpresso {
	public class CSharpEvaluator(
		Interpreter interpreter
	) {
		public object Evaluate(string expression) {
			return interpreter.Eval(expression);
		}
	}
}
