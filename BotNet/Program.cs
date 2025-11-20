using System;
using System.Threading;
using BotNet.Bot;
using BotNet.CommandHandlers;
using BotNet.CommandHandlers.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Services.BMKG;
using BotNet.Services.BotProfile;
using BotNet.Services.Brainfuck;
using BotNet.Services.BubbleWrap;
using BotNet.Services.ChineseCalendar;
using BotNet.Services.ClearScript;
using BotNet.Services.ColorCard;
using BotNet.Services.Craiyon;
using BotNet.Services.DynamicExpresso;
using BotNet.Services.Gemini;
using BotNet.Services.GoogleMap;
using BotNet.Services.GoogleSheets;
using BotNet.Services.GoogleSheets.Options;
using BotNet.Services.Hosting;
using BotNet.Services.ImageConverter;
using BotNet.Services.KokizzuVPSBenchmark;
using BotNet.Services.NoAsAService;
using BotNet.Services.OMDb;
using BotNet.Services.OpenAI;
using BotNet.Services.Pemilu2024;
using BotNet.Services.Pesto;
using BotNet.Services.Piston;
using BotNet.Services.Plot;
using BotNet.Services.Preview;
using BotNet.Services.Primbon;
using BotNet.Services.ProgrammerHumor;
using BotNet.Services.QrCode;
using BotNet.Services.Soundtrack;
using BotNet.Services.Sqlite;
using BotNet.Services.Stability;
using BotNet.Services.StatusPage;
using BotNet.Services.ThisXDoesNotExist;
using BotNet.Services.Tiktok;
using BotNet.Services.Tokopedia;
using BotNet.Services.Typography;
using BotNet.Services.TimeZone;
using BotNet.Services.Weather;
using BotNet.Views.Clock;
using BotNet.Views.DecimalClock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TimeZoneConverter;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

if (Environment.GetEnvironmentVariable("SENTRY_DSN") is string sentryDsn) {
	builder.WebHost.UseSentry(
		dsn: sentryDsn
	);
}

// DI Services
builder.Services.Configure<HostingOptions>(builder.Configuration.GetSection("HostingOptions"));
builder.Services.Configure<V8Options>(builder.Configuration.GetSection("V8Options"));
builder.Services.Configure<PistonOptions>(builder.Configuration.GetSection("PistonOptions"));
builder.Services.Configure<PestoOptions>(builder.Configuration.GetSection("PestoOptions"));
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAIOptions"));
builder.Services.Configure<StabilityOptions>(builder.Configuration.GetSection("StabilityOptions"));
builder.Services.Configure<GoogleMapOptions>(builder.Configuration.GetSection("GoogleMapOptions"));
builder.Services.Configure<CommandPrioritizationOptions>(builder.Configuration.GetSection("CommandPrioritizationOptions"));
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("GeminiOptions"));
builder.Services.Configure<GoogleSheetsOptions>(builder.Configuration.GetSection("GoogleSheetsOptions"));
builder.Services.Configure<OmdbOptions>(builder.Configuration.GetSection("OmdbOptions"));
builder.Services.AddHttpClient();
builder.Services.AddFontService();
builder.Services.AddColorCardRenderer();
builder.Services.AddImageConverter();
builder.Services.AddBrainfuckTranspiler();
builder.Services.AddV8Evaluator();
builder.Services.AddPistonClient();
builder.Services.AddPestoClient();
builder.Services.AddOpenAiClient();
builder.Services.AddProgrammerHumorScraper();
builder.Services.AddTiktokServices();
builder.Services.AddCSharpEvaluator();
builder.Services.AddThisXDoesNotExist();
builder.Services.AddCraiyonClient();
builder.Services.AddStabilityClient();
builder.Services.AddTokopediaServices();
builder.Services.AddGoogleMaps();
builder.Services.AddWeatherService();
builder.Services.AddBmkg();
builder.Services.AddPreviewServices();
builder.Services.AddBubbleWrapKeyboardGenerator();
builder.Services.AddPrimbonScraper();
builder.Services.AddChineseCalendarScraper();
builder.Services.AddNoAsAServiceClient();
builder.Services.AddCommandHandlers();
builder.Services.AddCommandPriorityCategorizer();
builder.Services.AddBotProfileAccessor();
builder.Services.AddGeminiClient();
builder.Services.AddSqliteDatabases();
builder.Services.AddPemilu2024();
builder.Services.AddGoogleSheets();
builder.Services.AddKokizzuVpsBenchmarkDataSource();
builder.Services.AddSoundtrackProvider();
builder.Services.AddTimeZoneService();
builder.Services.AddQrCodeGenerator();
builder.Services.AddMathPlotRenderer();
builder.Services.AddOmdbClient();
builder.Services.AddStatusPageClient();

// MediatR
builder.Services.AddMediatR(config => {
	config.Lifetime = ServiceLifetime.Transient;
	config.AutoRegisterRequestProcessors = true;
	config.RegisterServicesFromAssemblies(
		typeof(SlashCommandHandler).Assembly
	);
});

// Hosted Services
builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotOptions"));
builder.Services.AddSingleton<BotService>();
builder.Services.AddHostedService<BotService>();
builder.Services.AddHostedService<CommandConsumer>();

// Telegram Bot
builder.Services.AddTelegramBot(botToken: builder.Configuration["BotOptions:AccessToken"]!);

// Memory Cache
builder.Services.AddMemoryCache();

WebApplication app = builder.Build();

// Prometheus metrics
app.UseHttpMetrics();

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

// Clock renderer
app.MapGet(
	"/clock/svg", (
		string? iana
	) => Results.Content(
		content: ClockSvgBuilder.GenerateSvg(iana ?? "Asia/Jakarta"),
		contentType: "image/svg+xml"
	)
);

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

app.MapMetrics();

app.Run();
