using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppFeatures.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAppFeatures(this IServiceCollection services, TimeSpan syncInterval)
		{
			services.AddSingleton<IFeaturesStore, FeaturesStore>();

			services.AddSingleton<IFeaturesManager>(ctx =>
			{
				var serviceProvider = ctx.GetRequiredService<IServiceProvider>();
				var featureBuilders = ctx.GetRequiredService<IEnumerable<IFeatureBuilder>>();
				var store = ctx.GetRequiredService<IFeaturesStore>();
				var repository = ctx.GetRequiredService<IFeaturesRepository>();
				var logger = ctx.GetRequiredService<ILogger<FeaturesManager>>();

				return new FeaturesManager(
					services: serviceProvider,
					featureBuilders: featureBuilders,
					store: store,
					repository: repository,
					syncInterval: syncInterval,
					logger: logger);
			});

			services.AddHostedService<AppFeaturesHostedService>();

			return services;
		}

		public static IServiceCollection AddFeature<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : notnull
		{
			services.AddSingleton<IFeatureBuilder>(new FeatureBuilder<T>(factory));

			services.AddSingleton<IFeature<T>, Feature<T>>();

			services.AddSingleton<IFeatureSnapshot<T>, FeatureSnapshot<T>>();

			return services;
		}

		public static IServiceCollection AddAppFeaturesProviderInMemory(this IServiceCollection services)
		{
			services.AddSingleton<IFeaturesRepository, InMemoryRepository>();

			return services;
		}
	}
}
