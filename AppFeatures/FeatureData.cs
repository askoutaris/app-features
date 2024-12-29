namespace AppFeatures
{
	public class FeatureData
	{
		public string Key { get; }
		public object Value { get; }

		public FeatureData(string key, object value)
		{
			Key = key;
			Value = value;
		}
	}
}
