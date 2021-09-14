using BotNet.Bot;
using BotNet.Services.Giphy;
using Orleans;
using Orleans.Hosting;

Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostBuilderContext, services) => {
		// DI Services
		services.Configure<GiphyOptions>(hostBuilderContext.Configuration.GetSection("GiphyOptions"));
		services.AddHttpClient();
		services.AddGiphyClient();

		// Hosted Services
		services.Configure<BotOptions>(hostBuilderContext.Configuration.GetSection("BotOptions"));
		services.AddSingleton<BotService>();
		services.AddHostedService<BotService>();
	})
	.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) => {
		configurationBuilder
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<BotService>(optional: true, reloadOnChange: true);
	})
	.UseOrleans((hostBuilderContext, siloBuilder) => {
		siloBuilder
			.UseLocalhostClustering();
	})
	.Build()
	.Run();
