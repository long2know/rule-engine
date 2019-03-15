using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RuleEngine.Interfaces;
using RuleEngine.Domain.ExpressionParser;
using RuleEngine.Data;
using Microsoft.EntityFrameworkCore;

namespace RuleEngine
{
	public static class ServiceProviderFactory
	{
		public static IServiceProvider ServiceProvider { get; set; }
	}
	public class Startup
	{
		public static IConfigurationRoot Configuration { get; set; }
		public static void Main(string[] args)
		{
			string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			string launch = Environment.GetEnvironmentVariable("LAUNCH_PROFILE");

			if (string.IsNullOrWhiteSpace(env))
			{
				env = "Development";
			}

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				//.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				//.AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
				.AddEnvironmentVariables();

			//if (env == "Development")
			//{
			//	builder.AddUserSecrets<Startup>();
			//}

			Configuration = builder.Build();

			// Create a service collection and configure our depdencies
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			// Build the our IServiceProvider and set our static reference to it
			ServiceProviderFactory.ServiceProvider = serviceCollection.BuildServiceProvider();


			// Enter the applicaiton.. (run!)
			ServiceProviderFactory.ServiceProvider.GetService<Application>().Run(args);
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// Make configuration settings available
			//services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
			services.AddSingleton<IConfiguration>(Configuration);

			services.AddTransient<IParser, ParserService>();
			
			// Add caching
			services.AddMemoryCache();

			// Add EF context
			services.AddDbContext<BusinessRuleContext>(options =>
			{
				options.UseInMemoryDatabase(databaseName: "default-in-memory");
			}, ServiceLifetime.Scoped);
			services.AddDbContext<BusinessRuleContext>(ServiceLifetime.Scoped);

			// Add logging            
			services.AddLogging(builder =>
			{
				builder
					.AddConsole()
					.AddDebug();
			}).Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

			// Add Application 
			services.AddTransient<Application>();
		}
	}
}