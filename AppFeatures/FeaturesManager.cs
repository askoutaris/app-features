using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppFeatures
{
	public interface IFeaturesManager
	{
		Task Init();
		Feature GetFeature(string key);
		void UpdateFeature(object feature);
	}

	public class FeaturesManager : IFeaturesManager
	{
		private readonly IReadOnlyDictionary<string, Feature> _defaultFeatures;
		private Dictionary<string, Feature> _features;
		private Dictionary<string, Feature> _pendingUpdatedFeatures;

		private readonly IFeaturesRepository _repository;
		private readonly IFeaturesBuilder _builder;
		private readonly TimeSpan _syncInterval;
		private readonly ILogger<IFeaturesManager> _logger;
		private Timer? _syncTimer = null;

		public FeaturesManager(
			IReadOnlyDictionary<string, Feature> defaultFeatures,
			IFeaturesRepository repository,
			IFeaturesBuilder builder,
			TimeSpan syncInterval,
			ILogger<IFeaturesManager> logger)
		{
			_repository = repository;
			_logger = logger;
			_builder = builder;
			_syncInterval = syncInterval;
			_defaultFeatures = defaultFeatures;
			_features = new Dictionary<string, Feature>();
			_pendingUpdatedFeatures = new Dictionary<string, Feature>();
		}

		public Feature GetFeature(string key)
		{
			return _features[key];
		}

		public async Task Init()
		{
			await SyncFeatures();

			_syncTimer = new Timer(OnSyncTimerTick, null, _syncInterval, _syncInterval);
		}

		public void UpdateFeature(object value)
		{
			var key = _builder.GetFeatureKey(value.GetType());

			if (!_features.ContainsKey(key))
				throw new Exception($"UpdateFeature: Feature {key} has not been registered, hence you cannot update it");

			var feature = _builder.CreateFeature(key, value);

			_features[key] = feature;
			_pendingUpdatedFeatures[feature.Key] = feature;
		}

		private async Task SyncFeatures()
		{
			await LoadFeatures();

			await AddNewFeatures();
		}

		private async Task AddNewFeatures()
		{
			var newFeatures = _defaultFeatures
				.Where(x => !_features.ContainsKey(x.Key))
				.Select(x => x.Value)
				.ToArray();

			await _repository.Add(newFeatures);

			foreach (var feature in newFeatures)
			{
				_features.Add(feature.Key, feature);

				_logger.LogDebug($"AddNewFeatures: Feature added {feature.ToJson()}");
			}
		}

		private async Task LoadFeatures()
		{
			string[] keys = GetAllFeatureKeys();

			var entries = await _repository.Get(keys);

			var features = new Dictionary<string, Feature>(entries.Count);
			foreach (var feature in entries)
			{
				if (!features.ContainsKey(feature.Key))
				{
					features.Add(feature.Key, feature);

					_logger.LogDebug($"LoadFeatures: Feature loaded {feature.ToJson()}");
				}
				else
					_logger.LogWarning($"LoadFeatures: Duplicate feature {feature.Key} skipped");
			}

			_features = features;
		}

		private string[] GetAllFeatureKeys()
		{
			return _defaultFeatures.Keys.ToArray();
		}

		private void OnSyncTimerTick(object state)
		{
			_syncTimer?.Change(Timeout.Infinite, Timeout.Infinite);

			Task.Run(async () =>
			{
				try
				{
					await LoadFeatures();

					await _repository.Update(_pendingUpdatedFeatures.Values.ToArray());

					foreach (var feature in _pendingUpdatedFeatures.Values)
						_logger.LogDebug($"OnSyncTimerTick: Feature updated {feature.ToJson()}");

					_pendingUpdatedFeatures.Clear();

					_logger.LogDebug($"OnSyncTimerTick");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, ex.Message);
				}
			}).GetAwaiter().GetResult();

			_syncTimer?.Change(_syncInterval, _syncInterval);
		}
	}
}
