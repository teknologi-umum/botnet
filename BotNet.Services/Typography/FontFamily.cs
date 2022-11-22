using System;
using System.Collections.Generic;
using System.Linq;

namespace BotNet.Services.Typography {
	public class FontFamily {
		public string Name { get; }
		private readonly FontStyle[] _fontStyles;

		internal FontFamily(string name, Func<FontFamily, IEnumerable<FontStyle>> stylesSetup) {
			Name = name;
			_fontStyles = stylesSetup.Invoke(this).ToArray();
		}

		public FontStyle[] GetFontStyles() => _fontStyles;
	}
}
