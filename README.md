# app-features

Application features to control flow and functionalites of an application

### app-features Usage
```csharp
using System;
using System.Threading.Tasks;
using AppFeatures;
using AppFeatures.Extensions.DependencyInjection;
using AppFeatures.Providers.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Workbench
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var multiplexer = ConnectionMultiplexer.Connect("redis-connection-string");

			var customerFeauture = new CustomerRegistrationFeature(
				enabled: true,
				minAge: 18);

			var archivingFeauture = new ArchivingFeature(
				enabled: true,
				cutoffDate: DateTime.UtcNow.AddDays(-30));

			// setup our DI
			var serviceProvider = new ServiceCollection()
				.AddSingleton<IConnectionMultiplexer>(multiplexer)
				.AddLogging(builder =>
				{
					builder.AddConsole();
					builder.SetMinimumLevel(LogLevel.Trace);
				})
				//.AddAppFeaturesProviderInMemory()
				.AddAppFeaturesProviderRedis()
				.AddAppFeatures(
					syncInterval: TimeSpan.FromSeconds(10),
					x =>
					 {
						 x.AddFeature(customerFeauture);
						 x.AddFeature(archivingFeauture);
					 })
				.BuildServiceProvider();

			var manager = serviceProvider.GetService<IFeaturesManager>();

			await manager.Init();

			var customerFeautureValue = serviceProvider.GetRequiredService<CustomerRegistrationFeature>();
			var archivingFeautureValue = serviceProvider.GetRequiredService<ArchivingFeature>();

			var customerFeautureUpdated = new CustomerRegistrationFeature(
				enabled: true,
				minAge: 19);

			manager.UpdateFeature(customerFeautureUpdated);

			var customerFeautureValueUpdated = serviceProvider.GetRequiredService<CustomerRegistrationFeature>();

			Console.ReadLine();
		}
	}

	public class CustomerRegistrationFeature
	{
		public bool Enabled { get; }
		public int MinAge { get; }

		public CustomerRegistrationFeature(bool enabled, int minAge)
		{
			Enabled = enabled;
			MinAge = minAge;
		}
	}

	public class ArchivingFeature
	{
		public bool Enabled { get; }
		public DateTime CutoffDate { get; }

		public ArchivingFeature(bool enabled, DateTime cutoffDate)
		{
			Enabled = enabled;
			CutoffDate = cutoffDate;
		}
	}
}
```