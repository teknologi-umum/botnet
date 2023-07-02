using System;
using System.Runtime.Serialization;

namespace BotNet.Services.Pesto.Models;

public enum Language {
	[EnumMember(Value = "Brainfuck")]
	Brainfuck = 0,
	[EnumMember(Value = "C")]
	C = 1,
	[EnumMember(Value = "C++")]
	CPlusPlus = 2,
	[EnumMember(Value = "Common Lisp")]
	CommonLisp = 3,
	[EnumMember(Value = ".NET")]
	DotNet = 4,
	[EnumMember(Value = "Go")]
	Go = 5,
	[EnumMember(Value = "Java")]
	Java = 6,
	[EnumMember(Value = "Javascript")]
	Javascript = 7,
	[EnumMember(Value = "Julia")]
	Julia = 8,
	[EnumMember(Value = "Lua")]
	Lua = 9,
	[EnumMember(Value = "PHP")]
	PHP = 10,
	[EnumMember(Value = "Python")]
	Python = 11,
	[EnumMember(Value = "Ruby")]
	Ruby = 12,
	[EnumMember(Value = "SQLite3")]
	SQLite3 = 13,
	[EnumMember(Value = "V")]
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
			Language.Javascript => "JavaScript",
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
