using DynamicExpresso;

namespace BotNet.Services.DynamicExpresso {
	public class CSharpEvaluator {
		private readonly Interpreter _interpreter;

		public CSharpEvaluator(
			Interpreter interpreter
		) {
			_interpreter = interpreter;
		}

		public object Evaluate(string expression) {
			return _interpreter.Eval(expression);
		}
	}
}
