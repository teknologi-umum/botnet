using System;

namespace BotNet.Services.Pesto.Models;

public enum Language {
	Brainfuck = 0,
	C = 1,
	CPlusPlus = 2,
	CommonLisp = 3,
	DotNet = 4,
	Go = 5,
	Java = 6,
	JavaScript = 7,
	Julia = 8,
	Lua = 9,
	PHP = 10,
	Python = 11,
	Ruby = 12,
	SQLite3 = 13,
	V = 14
}

public static class LanguageExtensions {
	public static string ToString(this Language language) =>
		language switch {
			Language.Brainfuck => "Brainfuck",
			Language.C => "C",
			Language.CPlusPlus => "C++",
			Language.CommonLisp => "Common Lisp",
			Language.DotNet => ".NET",
			Language.Go => "Go",
			Language.Java => "Java",
			Language.JavaScript => "JavaScript",
			Language.Julia => "Julia",
			Language.Lua => "Lua",
			Language.PHP => "PHP",
			Language.Python => "Python",
			Language.Ruby => "Ruby",
			Language.SQLite3 => "SQLite3",
			Language.V => "V",
			_ => throw new ArgumentOutOfRangeException(nameof(language))
		};
}
