using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AppFeatures
{
	public interface IFeaturesBuilder
	{
		void AddFeature(object feature);
		IReadOnlyDictionary<string, Feature> BuildFeatures();
		Feature CreateFeature(string key, object value);
		string GetFeatureKey(Type type);
	}

	public class FeaturesBuilder : IFeaturesBuilder
	{
		private readonly Dictionary<string, object> _features;

		public FeaturesBuilder()
		{
			_features = new Dictionary<string, object>();
		}

		public void AddFeature(object feature)
		{
			var key = GetFeatureKey(feature.GetType());

			if (_features.ContainsKey(key))
				throw new Exception($"Feature with {key} already exists");

			_features[key] = feature;
		}

		public IReadOnlyDictionary<string, Feature> BuildFeatures()
		{
			var features = _features.ToDictionary(x => x.Key, x => CreateFeature(x.Key,x.Value));

			return new ReadOnlyDictionary<string, Feature>(features);
		}

		public Feature CreateFeature(string key, object value)
		{
			return new Feature(key, value);
		}

		public string GetFeatureKey(Type type)
		{
			return type.FullName;
		}
	}
}
