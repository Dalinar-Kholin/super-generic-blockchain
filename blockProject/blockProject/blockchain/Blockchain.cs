using blockProject.nodeCommunicatio;
using Newtonsoft.Json;

namespace blockProject.blockchain;

public class Blockchain
{
    private static Blockchain? _instance;
    private readonly IBlockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();

    private readonly Mutex _mutex = new();

    private readonly IValidator validator = new Validator();
    private List<BlockType> chain = new();

    // newest block to moduify when 3 records in it commited to blockchain
    public BlockType newestBlock = new();

    private Blockchain() { }

    // return count of records in blockchain that let us know to create block or not 
    public BlockType? AddRecord(byte[] Record)
    {
        var rec = newestBlock.AddRecord(Record);

        if (rec >= 3)
        {
            var newBlock = newestBlock;
            newBlock.header.PreviousHash = chain.Count == 0 ? "0" : chain[chain.Count - 1].header.Hash;
            newBlock.header.DataHash = validator.calcDataHash(newBlock);
            newBlock.header.Hash = validator.calcHash(newBlock);
            chain.Add(newestBlock);
            newestBlock = new BlockType();
            return newBlock;
        }

        return null;
    }

    public void CreateBlock(BlockType block)
    {
        _mutex.WaitOne();

        block.header.Hash = validator.calcHash(block);
        block.header.DataHash = validator.calcDataHash(block);

        chain.Add(block);
        _mutex.ReleaseMutex();
    }

    public void AddBlock(BlockType block)
    {
        _mutex.WaitOne();
        var err = validator.validate(block);
        if (err != null) // if not valid block we reject to include
        {
            Console.WriteLine($"{err.Message}");
            _mutex.ReleaseMutex();
            return;
        }

        chain.Add(block);
        _mutex.ReleaseMutex();
    }

    public List<BlockType> GetChain()
    {
        return chain;
    }

    public void SetChain(List<BlockType> blockchain)
    {
        chain = blockchain;
    }

    public string GetParsedBlockchain()
    {
        return JsonConvert.SerializeObject(chain);
    }

    public List<BlockType> GetBlockchain()
    {
        return chain;
    }


    public static Blockchain GetInstance()
    {
        return _instance ??= new Blockchain();
    }

    public static Blockchain GetTestInstance()
    {
        return new Blockchain();
    }

	// method is available only in test environment
	public static void Reset()
    {
        _instance = null;
    }
}

////klasa do obsługi blockchainu
//public class Blockchain : IBlockchain
//{

//    // private IValidate validator ?? pojawiennie się jakiejś klasy validatora?

//    private static Blockchain? _instance;
//    private Mutex _mut;
//    private NoneBlock _blockchain = new NoneBlock();
//    private Blockchain()
//    {
//        _mut = new Mutex();
//    }

//    public static Blockchain GetInstance()
//    {
//        return _instance ??= new Blockchain();
//    }

//