namespace blockProject.blockchain
{
	public class Block
	{
		public int Index { get; set; }
		public DateTime Timestamp { get; set; }
		public List<Record> Records { get; set; } = new();
		public string PreviousHash { get; set; } = "";
		public string Hash { get; set; } = "";

		public Block(int index, string previousHash)
		{
			Index = index;
			PreviousHash = previousHash;
			Timestamp = DateTime.UtcNow;
		}

		public void AddRecord(Record record)
		{
			Records.Add(record);
		}

		public void ComputeHash()
		{
			// todo: ogarnąć logikę tej funkcji (nonce itp.)
			var raw = $"{Index}-{PreviousHash}-{Timestamp}-{string.Join(",", Records.Select(r => r.ToString()))}";
			Hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)));
		}
	}
}
