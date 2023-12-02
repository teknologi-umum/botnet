using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Brainfuck;
using BotNet.Services.CopyPasta;
using BotNet.Services.FancyText;
using BotNet.Services.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using RG.Ninja;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Bot {
	public class InlineQueryHandler {
		private readonly IServiceProvider _serviceProvider;
		private readonly IGrainFactory _grainFactory;

		public InlineQueryHandler(
			IServiceProvider serviceProvider,
			IGrainFactory grainFactory
		) {
			_serviceProvider = serviceProvider;
			_grainFactory = grainFactory;
		}

		public async Task<ImmutableList<InlineQueryResult>> GetResultsAsync(string query, long userId, GrainCancellationToken grainCancellationToken) {
			List<Task<ImmutableList<InlineQueryResult>>> resultTasks = new();

			if (query.ToLowerInvariant().Trim() is string pastaKey
				&& CopyPastaLookup.TryGetAutoText(pastaKey, out ImmutableList<string>? pastas)) {
				resultTasks.Add(Task.FromResult(pastas.Select(pasta => new InlineQueryResultArticle(
					id: Guid.NewGuid().ToString("N"),
					title: pasta,
					inputMessageContent: new InputTextMessageContent(pasta)
				)).ToImmutableList<InlineQueryResult>()));
			}

			if (query.Length > 0) {
				string[] fancyTexts = await Task.WhenAll(
					Enum.GetValues<FancyTextStyle>()
						.Select(style => FancyTextGenerator.GenerateAsync(query, style, grainCancellationToken.CancellationToken))
				);
				resultTasks.Add(Task.FromResult(fancyTexts.Select(fancyText => new InlineQueryResultArticle(
					id: Guid.NewGuid().ToString("N"),
					title: fancyText,
					inputMessageContent: new InputTextMessageContent(fancyText)
				)).ToImmutableList<InlineQueryResult>()));
			}

			if (query.Length > 0) {
				string brainfuck = _serviceProvider
					.GetRequiredService<BrainfuckTranspiler>()
					.TranspileBrainfuck(query);
				resultTasks.Add(
					Task.FromResult(
						ImmutableList.Create<InlineQueryResult>(
							new InlineQueryResultArticle(
								id: Guid.NewGuid().ToString("N"),
								title: brainfuck,
								inputMessageContent: new InputTextMessageContent(brainfuck)
							)
						)
					)
				);
			}

			if (query.Length is 4 or 7 && query[0] == '#' && query[1..].All(c => c is >= 'a' and <= 'f' || c is >= 'A' and <= 'F' || char.IsDigit(c))) {
				HostingOptions hostingOptions = _serviceProvider.GetRequiredService<IOptions<HostingOptions>>().Value;
				string url = $"https://{hostingOptions.HostName}/renderer/color?name={WebUtility.UrlEncode(query)}";
				resultTasks.Add(Task.FromResult(ImmutableList.Create<InlineQueryResult>(
					new InlineQueryResultPhoto($"color{query[1..]}", url, url) {
						Title = query,
						Description = query,
						PhotoWidth = 200,
						PhotoHeight = 200
					}
				)));
			}

			if (query.StartsWith("gif ", StringComparison.InvariantCultureIgnoreCase, out string? gifQuery)) {
				resultTasks.Add(
					_grainFactory
						.GetGrain<ITenorGrain>(gifQuery)
						.SearchGifsAsync(grainCancellationToken)
						.ContinueWith(task => task.Result.Select(gif => new InlineQueryResultGif(gif.Id, gif.Url, gif.PreviewUrl)).ToImmutableList<InlineQueryResult>())
				);
			}

			return (await Task.WhenAll(resultTasks)).SelectMany(results => results).ToImmutableList();
		}
	}
}
