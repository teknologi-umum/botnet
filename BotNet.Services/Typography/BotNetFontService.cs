using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Graphics;

namespace BotNet.Services.Typography {
	public class BotNetFontService : IFontService {
		private static readonly IFontFamily JETBRAINS_MONO_NL = new FontFamily(
			name: "JetBrainsMonoNL",
			stylesSetup: EnumerateJetBrainsMonoMLStyles);

		private static IEnumerable<IFontStyle> EnumerateJetBrainsMonoMLStyles(IFontFamily fontFamily) {
			Assembly resourceAssembly = Assembly.GetAssembly(typeof(BotNetFontService))!;
			string resourceNamespace = "BotNet.Services.Typography.Assets";

			FontStyle CreateFontStyle(string name, int weight, FontStyleType styleType) {
				return new FontStyle(
					id: name,
					name: name,
					fullName: name,
					weight: weight,
					styleType: styleType,
					fontFamily: fontFamily,
					resourceAssembly: resourceAssembly,
					resourceName: $"{resourceNamespace}.{name}.ttf");
			}

			yield return CreateFontStyle("JetBrainsMonoNL-Thin", 100, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-ThinItalic", 100, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-ExtraLight", 200, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-ExtraLightItalic", 200, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-Light", 300, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-LightItalic", 300, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-Regular", 400, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-Italic", 400, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-Medium", 500, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-MediumItalic", 500, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-SemiBold", 600, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-SemiBoldItalic", 600, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-Bold", 700, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-BoldItalic", 700, FontStyleType.Italic);
			yield return CreateFontStyle("JetBrainsMonoNL-ExtraBold", 800, FontStyleType.Normal);
			yield return CreateFontStyle("JetBrainsMonoNL-ExtraBoldItalic", 800, FontStyleType.Italic);
		}

		public IFontStyle GetDefaultFontStyle() => JETBRAINS_MONO_NL.GetFontStyles().Single(style => style is { Weight: 400, StyleType: FontStyleType.Normal });
		public IFontFamily[] GetFontFamilies() => new[] { JETBRAINS_MONO_NL };
		public IFontStyle GetFontStyleById(string id) => JETBRAINS_MONO_NL.GetFontStyles().Single(style => style.Id == id);
	}
}
