using Microsoft.Extensions.DependencyInjection;

namespace AppFeatures.Providers.Redis
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAppFeaturesProviderRedis(this IServiceCollection services)
		{
			services.AddSingleton<IFeaturesRepository, RedisRepository>();

			return services;
		}
	}
}
