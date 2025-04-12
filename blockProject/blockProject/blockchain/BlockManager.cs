using System.Text.Json;

namespace blockProject.blockchain;

public static class BlockManager
{
    private static readonly string filePath = "../../../data.json"; // plik z blockchainem

    public static void SaveBlockchain(List<BlockType> chain)
    {
        // zapisujemy blockchain jako JSON do pliku
        var json = JsonSerializer.Serialize(chain);
        File.WriteAllText(filePath, json);
    }

    public static List<BlockType> LoadBlockchain()
    {
        // wczytujemy blockchain z pliku 
        if (!File.Exists(filePath)) return new List<BlockType>();
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<BlockType>>(json) ?? new List<BlockType>();
    }

    public static string SerializeBlock(BlockType block)
    {
        return JsonSerializer.Serialize(block);
    }

    public static BlockType DeserializeBlock(string json)
    {
        return JsonSerializer.Deserialize<BlockType>(json)!; // użycie ! zakłada że deserializacja się powiedzie
    }
}