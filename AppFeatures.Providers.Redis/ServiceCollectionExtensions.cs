using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AppFeatures.Providers.Redis
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAppFeaturesProviderRedis(this IServiceCollection services)
		{
			services.AddSingleton<IFeaturesRepository, RedisRepository>();

			return services;
		}

		public static IServiceCollection AddAppFeaturesProviderRedis(this IServiceCollection services, string redisConnectionString)
		{
			var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

			services.AddSingleton<IFeaturesRepository, RedisRepository>(services =>
			{
				var logger = services.GetRequiredService<ILogger<RedisRepository>>();

				return new RedisRepository(
					multiplexer: multiplexer,
					logger: logger);
			});

			return services;
		}
	}
}
