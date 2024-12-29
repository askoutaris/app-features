namespace AppFeatures
{
	public interface IFeature<T>
	{
		T Value { get; }
	}

	public class Feature<T> : IFeature<T>
	{
		public T Value { get; }

		public Feature(IFeaturesStore store)
		{
			Value = (T)store.Get(typeof(T).ToFeatureKey()).Value;
		}
	}
}
