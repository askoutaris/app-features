namespace AppFeatures
{
	public interface IFeatureSnapshot<T>
	{
		T Value { get; }
	}

	public class FeatureSnapshot<T> : IFeatureSnapshot<T>
	{
		private readonly IFeaturesStore _store;

		public T Value => (T)_store.Get(typeof(T).ToFeatureKey()).Value;

		public FeatureSnapshot(IFeaturesStore store)
		{
			_store = store;
		}
	}
}
