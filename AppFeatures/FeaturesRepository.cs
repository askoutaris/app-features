using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppFeatures
{
	public interface IFeaturesRepository
	{
		Task<ICollection<FeatureData>> GetAll();
		Task Add(FeatureData feature);
		Task Update(FeatureData feature);
	}

	public class InMemoryRepository : IFeaturesRepository
	{
		private readonly Dictionary<string, FeatureData> _features;

		public InMemoryRepository()
		{
			_features = [];
		}

		public Task Add(FeatureData feature)
		{
			_features[feature.Key] = feature;

			return Task.CompletedTask;
		}

		public Task<ICollection<FeatureData>> GetAll()
		{
			var features = _features.Values.ToArray();

			return Task.FromResult((ICollection<FeatureData>)features);
		}

		public Task Update(FeatureData feature)
		{
			_features[feature.Key] = feature;

			return Task.CompletedTask;
		}
	}
}
