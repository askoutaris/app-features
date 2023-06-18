using System.Text.Json;

namespace AppFeatures
{
	static class ObjectExtensions
	{
		public static string ToJson(this object obj)
			=> JsonSerializer.Serialize(obj);
	}
}
