using System;
using System.Threading.Tasks;
using AppFeatures;
using AppFeatures.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using AppFeatures.Providers.Redis;

namespace Workbench
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var multiplexer = ConnectionMultiplexer.Connect("your-redis-connection-string");

			var syncInterval = TimeSpan.FromSeconds(5);

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
				.AddAppFeatures(syncInterval: syncInterval)
					.AddFeature(services => new CustomerRegistrationFeature(enabled: true, minAge: 18))
					.AddFeature(services => new ArchivingFeature(enabled: true, cutoffDate: DateTime.UtcNow.AddDays(-30)))
				.BuildServiceProvider();

			var manager = serviceProvider.GetService<IFeaturesManager>();

			await manager.Init();

			var customerFeatureValue = serviceProvider.GetRequiredService<IFeature<CustomerRegistrationFeature>>();
			var archivingFeatureValue = serviceProvider.GetRequiredService<IFeatureSnapshot<ArchivingFeature>>();

			var customerFeatureUpdated = new CustomerRegistrationFeature(
				enabled: true,
				minAge: 19);

			manager.UpdateFeature(customerFeatureUpdated);

			await Task.Delay(syncInterval * 2);

			var customerFeatureValueUpdated = serviceProvider.GetRequiredService<IFeature<CustomerRegistrationFeature>>();

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
