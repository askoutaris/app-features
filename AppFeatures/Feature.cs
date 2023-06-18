namespace AppFeatures
{
	public class Feature
	{
		public string Key { get; }
		public object Value { get; }

		public Feature(string key, object value)
		{
			Key = key;
			Value = value;
		}
	}
}
