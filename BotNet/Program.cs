using BotNet.Bot;
using Orleans;
using Orleans.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostBuilderContext, services) => {
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
	.Build();

await host.StartAsync();

Console.WriteLine("Press any key to exit.");
Console.ReadKey();
await host.StopAsync();

return 0;
