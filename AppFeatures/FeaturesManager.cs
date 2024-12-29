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
		void Stop();
		void UpdateFeature(object feature);
	}

	public class FeaturesManager : IFeaturesManager
	{
		private readonly object _sync = new object();
		private readonly IServiceProvider _services;
		private readonly IEnumerable<IFeatureBuilder> _featureBuilders;
		private readonly IFeaturesStore _store;
		private readonly Dictionary<string, FeatureData> _updatedFeatures;

		private readonly IFeaturesRepository _repository;
		private readonly TimeSpan _syncInterval;
		private readonly ILogger<IFeaturesManager> _logger;
		private Timer? _syncTimer = null;

		public FeaturesManager(
			IServiceProvider services,
			IEnumerable<IFeatureBuilder> featureBuilders,
			IFeaturesStore store,
			IFeaturesRepository repository,
			TimeSpan syncInterval,
			ILogger<IFeaturesManager> logger)
		{
			_repository = repository;
			_logger = logger;
			_syncInterval = syncInterval;
			_services = services;
			_featureBuilders = featureBuilders;
			_store = store;
			_updatedFeatures = [];
		}

		public async Task Init()
		{
			await SyncFeatures();

			_syncTimer = new Timer(OnSyncTimerTick, null, _syncInterval, _syncInterval);
		}

		public void Stop()
		{
			_syncTimer?.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public void UpdateFeature(object value)
		{
			lock (_sync)
			{
				var key = value.GetType().ToFeatureKey();

				if (!_store.Contains(key))
					throw new Exception($"No registered feature for {value.GetType().FullName}");

				var feature = new FeatureData(key, value);

				_updatedFeatures[feature.Key] = feature;
			}
		}

		private async Task SyncFeatures()
		{
			await LoadFeatures();

			await AddNewFeatures();
		}

		private async Task AddNewFeatures()
		{
			var features = _featureBuilders
				.Select(x => x.BuildFeature(_services))
				.ToArray();

			foreach (var feature in features)
			{
				if (!_store.Contains(feature.Key))
				{
					await _repository.Add(feature);

					_store.Set(feature);

					_logger.LogInformation($"Feature added {feature.ToJson()}");
				}
			}
		}

		private async Task LoadFeatures()
		{
			var entries = await _repository.GetAll();

			foreach (var feature in entries)
			{
				_store.Set(feature);

				_logger.LogInformation($"Feature loaded {feature.ToJson()}");
			}
		}

		private void OnSyncTimerTick(object state)
		{
			_syncTimer?.Change(Timeout.Infinite, Timeout.Infinite);

			try
			{
				EnsureFeatures();

				UpdateFeatures();

				LoadFeatures().GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}

			_syncTimer?.Change(_syncInterval, _syncInterval);
		}

		private void EnsureFeatures()
		{
			var entries = _repository.GetAll().GetAwaiter().GetResult();

			var features = _store.GetAll();

			foreach (var feature in features)
			{
				if (!entries.Any(x => x.Key == feature.Key))
				{
					_repository.Add(feature).GetAwaiter().GetResult();

					_logger.LogWarning($"Missing feature detected and will be readded {feature.ToJson()}");
				}
			}
		}

		private void UpdateFeatures()
		{
			lock (_sync)
			{
				foreach (var feature in _updatedFeatures.Values.ToArray())
				{
					try
					{
						_repository.Update(feature).GetAwaiter().GetResult();

						_updatedFeatures.Remove(feature.Key);

						_logger.LogInformation($"Feature updated {feature.ToJson()}");
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, feature.ToJson());
					}
				}
			}
		}
	}
}
