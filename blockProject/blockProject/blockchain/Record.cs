namespace blockProject.blockchain
{
	public class Record
	{
		public string Key { get; set; }
		public string Value { get; set; }

		public Record(string key, string value)
		{
			Key = key;
			Value = value;
		}

		public override string ToString()
		{
			return $"{Key}:{Value}";
		}
	}

}


