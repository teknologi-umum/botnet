namespace BotNet.Services.Meme {
	internal sealed record Template(
		string ImageResourceName
	);

	internal static class Templates {
		public static readonly Template RAMAD = new("BotNet.Services.Meme.Images.Ramad.jpg");
	}
}
