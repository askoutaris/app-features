using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AppFeatures.Providers.Redis
{
	public class RedisRepository : IFeaturesRepository
	{
		private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
		private readonly IConnectionMultiplexer _multiplexer;
		private IDatabase _db => _multiplexer.GetDatabase();

		public RedisRepository(IConnectionMultiplexer multiplexer)
		{
			_multiplexer = multiplexer;
		}

		public async Task Add(Feature[] features)
		{
			foreach (var feature in features)
				await SetFeature(feature);
		}

		public async Task<ICollection<Feature>> Get(string[] keys)
		{
			var features = new List<Feature>(keys.Length);
			foreach (var key in keys)
			{
				var redisKey = GetRedisKey(key);
				var redisValue = await _db.StringGetAsync(redisKey);

				if (redisValue.HasValue)
				{
					var json = redisValue.ToString();
					var feature = Deserialize(json);
					features.Add(feature);
				}
			}

			return features;
		}

		public async Task Remove(string[] keys)
		{
			foreach (var key in keys)
			{
				var redisKey = GetRedisKey(key);
				await _db.KeyDeleteAsync(redisKey);
			}
		}

		public async Task Update(params Feature[] features)
		{
			foreach (var feature in features)
				await SetFeature(feature);
		}

		private async Task SetFeature(Feature feature)
		{
			var redisKey = GetRedisKey(feature.Key);
			var json = Serialize(feature);
			await _db.StringSetAsync(redisKey, json);
		}

		private string Serialize(Feature feature)
			=> JsonConvert.SerializeObject(feature, _serializerSettings);

		private Feature Deserialize(string json)
			=> JsonConvert.DeserializeObject<Feature>(json, _serializerSettings) ?? throw new Exception($"Invalid json: {json}");

		private string GetRedisKey(string key)
			=> $"AppFeatures.{key}";
	}
}
