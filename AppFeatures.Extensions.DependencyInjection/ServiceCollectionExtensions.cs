using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppFeatures.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAppFeatures(this IServiceCollection services, TimeSpan syncInterval, Action<FeaturesBuilder> configure)
		{
			var builder = new FeaturesBuilder();

			services.AddSingleton<IFeaturesBuilder>(builder);

			configure(builder);

			var features = builder.BuildFeatures();

			RegisterFeatures(services, features);

			RegisterManager(services, features, syncInterval);

			services.AddHostedService<AppFeaturesHostedService>();

			return services;
		}

		public static IServiceCollection AddAppFeaturesProviderInMemory(this IServiceCollection services)
		{
			services.AddSingleton<IFeaturesRepository, InMemoryRepository>();

			return services;
		}

		private static void RegisterManager(IServiceCollection services, IReadOnlyDictionary<string, Feature> features, TimeSpan syncInterval)
		{
			services.AddSingleton<IFeaturesManager>(serviceProvider =>
			{
				var builder = serviceProvider.GetRequiredService<IFeaturesBuilder>();
				var repository = serviceProvider.GetRequiredService<IFeaturesRepository>();
				var logger = serviceProvider.GetRequiredService<ILogger<IFeaturesManager>>();

				return new FeaturesManager(features, repository, builder, syncInterval, logger);
			});
		}

		private static void RegisterFeatures(IServiceCollection services, IReadOnlyDictionary<string, Feature> features)
		{
			foreach (var feature in features.Values)
			{
				var type = feature.Value.GetType();
				services.AddTransient(type, sericeProvider =>
				{
					var builder = sericeProvider.GetRequiredService<IFeaturesBuilder>();
					var manager = sericeProvider.GetRequiredService<IFeaturesManager>();
					var key = builder.GetFeatureKey(type);
					var featureInstance = manager.GetFeature(key);
					return featureInstance.Value;
				});
			}
		}
	}
}
