using BotNet;
using BotNet.Bot;
using BotNet.Services.Tenor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

Host.CreateDefaultBuilder(args)
	.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
	.ConfigureServices((hostBuilderContext, services) => {
		// DI Services
		services.Configure<TenorOptions>(hostBuilderContext.Configuration.GetSection("TenorOptions"));
		services.AddHttpClient();
		services.AddTenorClient();

		// Hosted Services
		services.Configure<BotOptions>(hostBuilderContext.Configuration.GetSection("BotOptions"));
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
