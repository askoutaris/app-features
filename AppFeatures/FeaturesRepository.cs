using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppFeatures
{
	public interface IFeaturesRepository
	{
		Task<ICollection<Feature>> Get(params string[] keys);
		Task Add(params Feature[] features);
		Task Update(params Feature[] features);
	}

	public class InMemoryRepository : IFeaturesRepository
	{
		private readonly Dictionary<string, Feature> _features;

		public InMemoryRepository()
		{
			_features = new Dictionary<string, Feature>();
		}

		public Task Add(Feature[] features)
		{
			foreach (var feature in features)
				_features[feature.Key] = feature;

			return Task.CompletedTask;
		}

		public Task<ICollection<Feature>> Get(string[] keys)
		{
			var features = _features.Values
				.Where(x => keys.Contains(x.Key))
				.ToArray();

			return Task.FromResult((ICollection<Feature>)features);
		}

		public Task Update(params Feature[] features)
		{
			foreach (var feature in features)
				_features[feature.Key] = feature;

			return Task.CompletedTask;
		}
	}
}
