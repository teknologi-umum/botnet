using System;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Graphics;

namespace BotNet.Services.Typography {
	public class FontStyle {
		public string Id { get; }
		public string Name { get; }
		public string FullName { get; }
		public int Weight { get; }
		public FontStyleType StyleType { get; }

		private readonly WeakReference<FontFamily> _fontFamilyRef;
		public FontFamily FontFamily => _fontFamilyRef.TryGetTarget(out FontFamily? fontFamily) ? fontFamily : throw new NullReferenceException("Referenced FontFamily no longer exists.");

		private readonly Assembly _resourceAssembly;
		private readonly string _resourceName;

		public FontStyle(
			string id,
			string name,
			string fullName,
			int weight,
			FontStyleType styleType,
			FontFamily fontFamily,
			Assembly resourceAssembly,
			string resourceName
		) {
			Id = id;
			Name = name;
			FullName = fullName;
			Weight = weight;
			StyleType = styleType;
			_fontFamilyRef = new WeakReference<FontFamily>(fontFamily);
			_resourceAssembly = resourceAssembly;
			_resourceName = resourceName;
		}

		public int CompareTo(FontStyle? other) => Id.CompareTo(other?.Id ?? throw new ArgumentNullException(nameof(other)));

		public Stream OpenStream() => _resourceAssembly.GetManifestResourceStream(_resourceName) ?? throw new InvalidOperationException("Embedded resource could not be loaded.");
	}
}
