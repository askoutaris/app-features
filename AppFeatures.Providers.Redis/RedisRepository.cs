using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AppFeatures.Providers.Redis
{
	public class RedisRepository : IFeaturesRepository
	{
		private const string _hashsetKey = "AppFeatures";
		private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
		private readonly IConnectionMultiplexer _multiplexer;
		private readonly ILogger<RedisRepository> _logger;

		private IDatabase _db => _multiplexer.GetDatabase();

		public RedisRepository(IConnectionMultiplexer multiplexer, ILogger<RedisRepository> logger)
		{
			_multiplexer = multiplexer;
			_logger = logger;
		}

		public async Task<ICollection<FeatureData>> GetAll()
		{
			var fields = await _db.HashGetAllAsync(_hashsetKey);

			var features = new List<FeatureData>(fields.Length);

			foreach (var field in fields)
			{
				try
				{
					if (field.Value.IsNull)
						throw new Exception("Null value detected");

					var feature = Deserialize(field.Value!);

					features.Add(feature);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Deserialization error - feature will be deleted from redis key:{field.Name} value:{field.Value}");

					await _db.HashDeleteAsync(_hashsetKey, field.Name);
				}
			}

			return features;
		}

		public async Task Add(FeatureData feature)
		{
			await SetFeature(feature);
		}

		public async Task Update(FeatureData feature)
		{
			await SetFeature(feature);
		}

		private async Task SetFeature(FeatureData feature)
		{
			var json = Serialize(feature);

			await _db.HashSetAsync(_hashsetKey, feature.Key, json);
		}

		private string Serialize(FeatureData feature)
			=> JsonConvert.SerializeObject(feature, _serializerSettings);

		private FeatureData Deserialize(string json)
			=> JsonConvert.DeserializeObject<FeatureData>(json, _serializerSettings) ?? throw new Exception($"Invalid json: {json}");
	}
}
