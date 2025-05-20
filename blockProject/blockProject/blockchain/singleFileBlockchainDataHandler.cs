using blockProject.randomSrc;
using Newtonsoft.Json;

namespace blockProject.nodeCommunicatio;

public interface IBlockchainDataHandler
{
    Error? editBlock(string blockHash, BlockType block);
    Error? writeBlockchain(List<BlockType> blocks);
    Error? writeBlock(BlockType blocks);
    (List<BlockType>, Error?) readBlockchain();
    (bool, Error?) isBlockInBlockchain(string blockHash);
    Error? deleteBlock(string blockHash);
}

public class singleFileBlockchainDataHandler : IBlockchainDataHandler
{
    private static singleFileBlockchainDataHandler? _instance;
    public string _filePath = "data.json";
    private readonly Mutex _mutex = new();

    private singleFileBlockchainDataHandler()
    {
    }


    // this function doesn't make sense if we don't modify the last block, because we will destroy the hash chain
    public Error? editBlock(string blockHash, BlockType block)
    {
        var (res, err) = readBlockchain();
        if (err != null) return err;

        var index = res.FindIndex(b => b.header.Hash == blockHash);
        if (index == -1) return new Error("Block hash not found in blockchain");
        res[index] = block;

        err = writeBlockchain(res);
        return err;
    }

    public Error? writeBlockchain(List<BlockType> blocks)
    {
        _mutex.WaitOne();

        var parsedJson = JsonConvert.SerializeObject(blocks);
        try
        {
            File.WriteAllText(_filePath, parsedJson);
        }
        catch (Exception e)
        {
            return new Error($"Write failed: {e.Message}");
        }

        finally
        {
            _mutex.ReleaseMutex();
        }

        return null;
    }

    // just adding a block to the blockchain instead of saving the entire blockchain// or at least that's how it should be
    public Error? writeBlock(BlockType blocks)
    {
        var (blockchain, err) = readBlockchain();
        if (err != null) return err;

        blockchain.Add(blocks);
        writeBlockchain(blockchain);
        return null;
    }

    public (List<BlockType>, Error?) readBlockchain()
    {
        _mutex.WaitOne();
        try
        {
            if (!File.Exists(_filePath))
            {
                return (new List<BlockType>(), new Error("Blockchain file not found"));
            }

            var json = File.ReadAllText(_filePath);
            var chain = JsonConvert.DeserializeObject<List<BlockType>>(json) ?? new List<BlockType>();
            return (chain, null);
        }
        catch (Exception e)
        {
            return (new List<BlockType>(), new Error($"Read failed: {e.Message}"));
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public (bool, Error?) isBlockInBlockchain(string blockHash)
    {
        var (blockchain, err) = readBlockchain();
        if (err != null) return (false, err);

        return (blockchain.Exists(block => block.header.Hash == blockHash), null);
    }

    public Error? deleteBlock(string blockHash)
    {
        var (blockchain, err) = readBlockchain();

        var index = blockchain.FindIndex(block => block.header.Hash == blockHash);
        if (index == -1) return new Error("Block not found");
        blockchain.RemoveAt(index);

        err = writeBlockchain(blockchain);
        return err;
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