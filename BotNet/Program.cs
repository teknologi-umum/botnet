using BotNet;
using BotNet.Bot;
using BotNet.Services.ColorCard;
using BotNet.Services.DuckDuckGo;
using BotNet.Services.Hosting;
using BotNet.Services.ImageConverter;
using BotNet.Services.MemoryPressureCoordinator;
using BotNet.Services.OCR;
using BotNet.Services.OpenGraph;
using BotNet.Services.SafeSearch;
using BotNet.Services.Tenor;
using BotNet.Services.Typography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

Host.CreateDefaultBuilder(args)
	.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
	.ConfigureServices((hostBuilderContext, services) => {
		IConfiguration configuration = hostBuilderContext.Configuration;

		// DI Services
		services.Configure<HostingOptions>(configuration.GetSection("HostingOptions"));
		services.Configure<TenorOptions>(configuration.GetSection("TenorOptions"));
		services.AddHttpClient();
		services.AddTenorClient();
		services.AddFontService();
		services.AddColorCardRenderer();
		services.AddSafeSearch();
		services.AddDuckDuckGo();
		services.AddOpenGraph();
		services.AddImageConverter();
		services.AddOCR();
		services.AddMemoryPressureCoordinator();

		// Telemetry
		services.AddApplicationInsightsTelemetry(configuration.GetConnectionString("AppInsights"));

		// Hosted Services
		services.Configure<BotOptions>(configuration.GetSection("BotOptions"));
		services.AddSingleton<BotService>();
		services.AddHostedService<BotService>();
	})
	.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) => {
		configurationBuilder
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
			.AddKeyPerFile("/run/secrets", optional: true, reloadOnChange: true)
			.AddUserSecrets<BotService>(optional: true, reloadOnChange: true);
	})
	.UseOrleans((hostBuilderContext, siloBuilder) => {
		siloBuilder
			.UseLocalhostClustering();
	})
	.Build()
	.Run();
