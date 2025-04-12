using System.Text.Json;
using blockProject.blockchain;

namespace blockProject.nodeCommunicatio;

public class SimpleTextCm : ICommunicationMaster
{
    private const string BlockchainFile = "../../../data.json";
    private List<string> _nodes = new();

    private readonly DataSender sender;

    public SimpleTextCm(DataSender sender)
    {
        this.sender = sender;
    }


    // bardzo podstawowa metoda do przesylanai bloku dalej
    // trzeba tutaj zaimplementowac algorytm plotki
    public void SendFurther(BlockType block)
    {
        var ips = sender.GetIps();
        foreach (var ip in ips)
        {
            var error = sender.SendBlock(block).Result;
            if (error != null)
            {
                Console.WriteLine($"Error sending block to {ip}: {error.Message}");
            }
        }
    }

    public void AddToBlockchain(BlockType block)
    {
        Console.WriteLine($"Adding block to blockchain: {block}");
        return; // TODO: implementacja tej metody
    }


    public void GetNodes()
    {
        throw new NotImplementedException("Method not implemented yet.");
    }

    public List<BlockType> GetBlockchain()
    {
        return Blockchain.GetInstance().GetBlockchain();
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