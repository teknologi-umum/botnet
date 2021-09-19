using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace BotNet.Services.Typography {
	public class FontFamily : IFontFamily {
		public string Name { get; }
		private readonly IFontStyle[] _fontStyles;

		internal FontFamily(string name, Func<FontFamily, IEnumerable<IFontStyle>> stylesSetup) {
			Name = name;
			_fontStyles = stylesSetup.Invoke(this).ToArray();
		}

		public IFontStyle[] GetFontStyles() => _fontStyles;
	}
}
