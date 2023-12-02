using System;
using System.Threading;
using BotNet.Bot;
using BotNet.Services.BMKG;
using BotNet.Services.Brainfuck;
using BotNet.Services.BubbleWrap;
using BotNet.Services.ClearScript;
using BotNet.Services.ColorCard;
using BotNet.Services.Craiyon;
using BotNet.Services.DynamicExpresso;
using BotNet.Services.GoogleMap;
using BotNet.Services.Hosting;
using BotNet.Services.ImageConverter;
using BotNet.Services.Meme;
using BotNet.Services.OpenAI;
using BotNet.Services.OpenGraph;
using BotNet.Services.Pesto;
using BotNet.Services.Piston;
using BotNet.Services.Preview;
using BotNet.Services.ProgrammerHumor;
using BotNet.Services.Stability;
using BotNet.Services.Tenor;
using BotNet.Services.ThisXDoesNotExist;
using BotNet.Services.Tiktok;
using BotNet.Services.Tokopedia;
using BotNet.Services.Typography;
using BotNet.Services.Weather;
using BotNet.Views.DecimalClock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

if (Environment.GetEnvironmentVariable("SENTRY_DSN") is string sentryDsn) {
	builder.WebHost.UseSentry(
		dsn: sentryDsn
	);
}

// DI Services
builder.Services.Configure<HostingOptions>(builder.Configuration.GetSection("HostingOptions"));
builder.Services.Configure<TenorOptions>(builder.Configuration.GetSection("TenorOptions"));
builder.Services.Configure<V8Options>(builder.Configuration.GetSection("V8Options"));
builder.Services.Configure<PistonOptions>(builder.Configuration.GetSection("PistonOptions"));
builder.Services.Configure<PestoOptions>(builder.Configuration.GetSection("PestoOptions"));
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAIOptions"));
builder.Services.Configure<StabilityOptions>(builder.Configuration.GetSection("StabilityOptions"));
builder.Services.Configure<GoogleMapOptions>(builder.Configuration.GetSection("GoogleMapOptions"));
builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection("WeatherOptions"));
builder.Services.AddHttpClient();
builder.Services.AddTenorClient();
builder.Services.AddFontService();
builder.Services.AddColorCardRenderer();
builder.Services.AddOpenGraph();
builder.Services.AddImageConverter();
builder.Services.AddBrainfuckTranspiler();
builder.Services.AddV8Evaluator();
builder.Services.AddPistonClient();
builder.Services.AddPestoClient();
builder.Services.AddOpenAIClient();
builder.Services.AddProgrammerHumorScraper();
builder.Services.AddTiktokServices();
builder.Services.AddCSharpEvaluator();
builder.Services.AddThisXDoesNotExist();
builder.Services.AddCraiyonClient();
builder.Services.AddStabilityClient();
builder.Services.AddTokopediaServices();
builder.Services.AddGoogleMaps();
builder.Services.AddWeatherService();
builder.Services.AddBMKG();
builder.Services.AddPreviewServices();
builder.Services.AddMemeGenerator();
builder.Services.AddBubbleWrapKeyboardGenerator();

// Hosted Services
builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotOptions"));
builder.Services.AddSingleton<BotService>();
builder.Services.AddHostedService<BotService>();

// Telegram Bot
builder.Services.AddTelegramBot(botToken: builder.Configuration["BotOptions:AccessToken"]!);

// Memory Cache
builder.Services.AddMemoryCache();

// Localhost Orleans
builder.Host.UseOrleans((hostBuilderContext, siloBuilder) => {
	siloBuilder.UseLocalhostClustering();
});

WebApplication app = builder.Build();

// Healthcheck endpoint
app.MapGet("/", () => "https://t.me/teknologi_umum_v2");

// Webhook
app.MapPost("/webhook/{secretPath}", async (
	string secretPath,
	[FromBody] Update update,
	[FromServices] ITelegramBotClient telegramBotClient,
	[FromServices] UpdateHandler updateHandler,
	[FromServices] IOptions<BotOptions> botOptionsAccessor,
	CancellationToken cancellationToken
) => {
	if (secretPath != botOptionsAccessor.Value.AccessToken!.Split(':')[1]) return Results.NotFound();
	await updateHandler.HandleUpdateAsync(telegramBotClient, update, cancellationToken);
	return Results.Ok();
});

// Decimal clock renderer
app.MapGet("/decimalclock/svg", () => Results.Content(
	content: DecimalClockSvgBuilder.GenerateSvg(),
	contentType: "image/svg+xml"
));

// Color card renderer
app.MapGet("/renderer/color", (
	string name,
	[FromServices] ColorCardRenderer colorCardRenderer
) => {
	try {
		return Results.File(
			fileContents: colorCardRenderer.RenderColorCard(name),
			contentType: "image/png",
			enableRangeProcessing: true
		);
	} catch {
		return Results.NotFound();
	}
});

// Meme generator test
#if DEBUG
app.MapGet("/meme", ([FromServices] MemeGenerator memeGenerator) => Results.File(
	fileContents: memeGenerator.CaptionRamad("Melakukan TDD, meski situasi sulit"),
	contentType: "image/png",
	enableRangeProcessing: true
));
#endif

app.Run();
