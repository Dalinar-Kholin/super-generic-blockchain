using System.Text.Json;

namespace blockProject.nodeCommunicatio;

public class SimpleTextCm : ICommunicationMaster
{
    private const string BlockchainFile = "../../../data.json";
    private List<string> _nodes = new List<string>();


    public void AddToBlockchain(string data)
    {
        throw new NotImplementedException("Method not implemented yet.");
    }

    public void GetNodes()
    {
        throw new NotImplementedException("Method not implemented yet.");
    }

    public string GetBlockchain()
    {
        Console.WriteLine("Getting blockchain data...");
        var blockchain = GetBlockchainList();
        return JsonSerializer.Serialize(blockchain);
    }

    private List<string> GetBlockchainList()
    {
        try
        {
            var json = File.ReadAllText(BlockchainFile);
            Console.WriteLine($"{Directory.GetCurrentDirectory()}");
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading blockchain: {ex.Message}");
            return new List<string>();
        }
    }
}