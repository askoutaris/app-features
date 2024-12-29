using System;
using System.Collections.Generic;

namespace AppFeatures
{
	public interface IFeaturesStore
	{
		bool Contains(string key);
		FeatureData Get(string key);
		IReadOnlyCollection<FeatureData> GetAll();
		void Set(FeatureData feature);
	}

	public class FeaturesStore : IFeaturesStore
	{
		private readonly Dictionary<string, FeatureData> _features;

		public FeaturesStore()
		{
			_features = [];
		}

		public FeatureData Get(string key)
		{
			if (_features.TryGetValue(key, out var feature))
				return feature;
			else
				throw new Exception($"Feature has not been registered {key}");
		}

		public void Set(FeatureData feature)
			=> _features[feature.Key] = feature;

		public bool Contains(string key)
			=> _features.ContainsKey(key);

		public IReadOnlyCollection<FeatureData> GetAll()
			=> [.. _features.Values];

	}
}
