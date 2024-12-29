using System;

namespace AppFeatures
{
	public interface IFeatureBuilder
	{
		FeatureData BuildFeature(IServiceProvider services);
	}

	public class FeatureBuilder<T> : IFeatureBuilder where T : notnull
	{
		private readonly Func<IServiceProvider, T> _factory;

		public FeatureBuilder(Func<IServiceProvider, T> factory)
		{
			_factory = factory;
		}

		public FeatureData BuildFeature(IServiceProvider services)
		{
			var value = _factory(services);

			var key = typeof(T).ToFeatureKey();

			return new FeatureData(key, value);
		}
	}
}
