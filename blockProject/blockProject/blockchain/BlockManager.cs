using System.Text.Json;

namespace blockProject.blockchain
{
	public static class BlockManager
	{
		private static string filePath = "blockchain_data.json"; // plik z blockchainem

		public static void SaveBlockchain(List<Block> chain)
		{
			// zapisujemy blockchain jako JSON do pliku
			var json = JsonSerializer.Serialize(chain);
			File.WriteAllText(filePath, json);
		}

		public static List<Block> LoadBlockchain()
		{
			// wczytujemy blockchain z pliku 
			if (!File.Exists(filePath)) return new List<Block>();
			var json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<List<Block>>(json) ?? new List<Block>();
		}

		public static string SerializeBlock(Block block)
		{
			return JsonSerializer.Serialize(block);
		}

		public static Block DeserializeBlock(string json)
		{
			return JsonSerializer.Deserialize<Block>(json)!; // użycie ! zakłada że deserializacja się powiedzie
		}
	}
}
