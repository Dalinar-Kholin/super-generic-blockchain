using blockProject.randomSrc;
using Newtonsoft.Json;

namespace blockProject.nodeCommunicatio;

public interface IBlockchainDataHandler
{
    Error? editBlock(string blockhash, BlockType block);
    Error? writeBlockchain(List<BlockType> blocks);
    Error? writeBlockc(BlockType blocks);
    (List<BlockType>, Error?) readBlockchain();
    (bool, Error?) isBlockInBlockchain(string blockHash);
    Error? deleteBlock(string blockHash);
}

public class singleFileBlockchainDataHandler : IBlockchainDataHandler
{
    private static singleFileBlockchainDataHandler? _instance;
    public string _filePath = "data.json";
    private readonly Mutex mut = new();

    private singleFileBlockchainDataHandler()
    {
    }


    // ta funckcja nie ma sensu jeżeli nie modyfikujemy ostatniego bloku, ponieważ będziemy niszczyć ciąg hashy
    public Error? editBlock(string blockhash, BlockType block)
    {
        var (res, err) = readBlockchain();
        if (err != null) return err;

        var index = res.FindIndex(b => b.header.Hash == blockhash);
        if (index == -1) return new Error("there is no selected hash in blockchain");
        res[index] = block;
        err = writeBlockchain(res);
        return err;
    }

    public Error? writeBlockchain(List<BlockType> blocks)
    {
        mut.WaitOne();

        var parsedJson = JsonConvert.SerializeObject(blocks);
        try
        {
            File.WriteAllText(_filePath, parsedJson);
        }
        catch (Exception e)
        {
            return new Error(e.Message);
        }

        mut.ReleaseMutex();
        return null;
    }

    // tylko dodanie bloku do blockchainnu zamiast zapisywać cały blockchain// a przynajmnie j tak powinno być ale na razie jest XD
    public Error? writeBlockc(BlockType blocks)
    {
        var (blockchain, err) = readBlockchain();
        if (err != null) return err;
        blockchain.Add(blocks);
        writeBlockchain(blockchain);
        return null;
    }

    public (List<BlockType>, Error?) readBlockchain()
    {
        mut.WaitOne();
        Console.WriteLine($"path {_filePath}");
        if (!File.Exists(_filePath))
        {
            mut.ReleaseMutex();
            return (new List<BlockType>(), new Error("there is no selected file "));
        }

        var json = File.ReadAllText(_filePath);
        var chain = JsonConvert.DeserializeObject<List<BlockType>>(json) ?? new List<BlockType>();
        mut.ReleaseMutex();

        return (chain, null);
    }

    public (bool, Error?) isBlockInBlockchain(string blockHash)
    {
        var (blockchain, err) = readBlockchain();
        return err != null ? (false, err) : (blockchain.FindIndex(block => block.header.Hash == blockHash) != -1, null);
    }

    public Error? deleteBlock(string blockHash)
    {
        var (blockchain, err) = readBlockchain();
        if (err != null) return err;
        blockchain.RemoveAt(blockchain.FindIndex(block => block.header.Hash == blockHash));
        err = writeBlockchain(blockchain);
        return err ?? null;
    }

    public static singleFileBlockchainDataHandler GetInstance()
    {
        return _instance ??= new singleFileBlockchainDataHandler();
    }

    public static singleFileBlockchainDataHandler GetTestInstance()
    {
        return new singleFileBlockchainDataHandler();
    }
}