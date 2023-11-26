using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BotNet {
	[ExcludeFromCodeCoverage]
	public class Startup(IConfiguration configuration, IWebHostEnvironment environment) {
		public IConfiguration Configuration { get; } = configuration;
		public IWebHostEnvironment Environment { get; } = environment;

		public void ConfigureServices(IServiceCollection services) {
			IMvcBuilder mvcBuilder = services.AddControllersWithViews();
			if (Environment.IsDevelopment()) {
				mvcBuilder.AddRazorRuntimeCompilation();
			}

			// Yes. Those f***ers still use NewtonsoftJson
			services.AddControllers()
				.AddNewtonsoftJson();

			services.AddResponseCaching();
			services.AddResponseCompression();
		}

		public void Configure(IApplicationBuilder app) {
			if (Environment.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			} else {
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseResponseCaching();
			app.UseResponseCompression();

			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}
	}
}
