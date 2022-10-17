using BotNet;
using BotNet.Bot;
using BotNet.PSE;
using BotNet.Services.Brainfuck;
using BotNet.Services.ClearScript;
using BotNet.Services.ColorCard;
using BotNet.Services.Craiyon;
using BotNet.Services.DynamicExpresso;
using BotNet.Services.Hosting;
using BotNet.Services.ImageConverter;
using BotNet.Services.OpenAI;
using BotNet.Services.OpenGraph;
using BotNet.Services.Piston;
using BotNet.Services.ProgrammerHumor;
using BotNet.Services.PSE;
using BotNet.Services.Stability;
using BotNet.Services.Tenor;
using BotNet.Services.ThisXDoesNotExist;
using BotNet.Services.Tiktok;
using BotNet.Services.Typography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

Host.CreateDefaultBuilder(args)
	.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
	.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) => {
		configurationBuilder
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
			.AddKeyPerFile("/run/secrets", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables("ASPNETCORE_")
			.AddUserSecrets<BotService>(optional: true, reloadOnChange: true);
	})
	.ConfigureServices((hostBuilderContext, services) => {
		IConfiguration configuration = hostBuilderContext.Configuration;

		// DI Services
		services.Configure<HostingOptions>(configuration.GetSection("HostingOptions"));
		services.Configure<TenorOptions>(configuration.GetSection("TenorOptions"));
		services.Configure<V8Options>(configuration.GetSection("V8Options"));
		services.Configure<PistonOptions>(configuration.GetSection("PistonOptions"));
		services.Configure<OpenAIOptions>(configuration.GetSection("OpenAIOptions"));
		services.Configure<StabilityOptions>(configuration.GetSection("StabilityOptions"));
		services.AddHttpClient();
		services.AddTenorClient();
		services.AddFontService();
		services.AddColorCardRenderer();
		services.AddOpenGraph();
		services.AddImageConverter();
		services.AddBrainfuckTranspiler();
		services.AddV8Evaluator();
		services.AddPistonClient();
		services.AddOpenAIClient();
		services.AddProgrammerHumorScraper();
		services.AddTiktokServices();
		services.AddCSharpEvaluator();
		services.AddThisXDoesNotExist();
		services.AddPSEClient();
		services.AddCraiyonClient();
		services.AddStabilityClient();

		// Hosted Services
		services.Configure<BotOptions>(configuration.GetSection("BotOptions"));
		services.AddSingleton<BotService>();
		services.AddSingleton<PSEService>();
		services.AddHostedService<BotService>();
		services.AddHostedService<PSEService>();

		// Telegram Bot
		services.AddTelegramBot(botToken: configuration["BotOptions:AccessToken"]);
	})
	.UseOrleans((hostBuilderContext, siloBuilder) => {
		siloBuilder
			.UseLocalhostClustering();
	})
	.Build()
	.Run();
