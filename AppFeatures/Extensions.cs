using System;
using System.Text.Json;

namespace AppFeatures
{
	static class Extensions
	{
		public static string ToJson(this object obj)
			=> JsonSerializer.Serialize(obj);

		public static string ToFeatureKey(this Type type)
			=> type.FullName;
	}
}
