using blockProject.nodeCommunicatio;
using Newtonsoft.Json;

namespace blockProject.blockchain;

public class Blockchain : IBlockchain<BlockType, messageRecord>
{
    private static Blockchain? _instance;
    private readonly IblockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();

    private readonly Mutex _mutex = new();

    private readonly IValidator validator = new Validator();
    private List<BlockType> chain = new();

    // najowszy blok gdzie będziemy przekazywać dane rekordu
    // a następnie przy spełnieniu warunków jest commitowany do blockchainu
    public BlockType newestBlock = new();

    private Blockchain()
    {
        // taka głupotka, ponieważ zakłada że mamy już stworzony plik z blockchainem, utrudnia testowanie 
        /*var (storedChain, error) = _blockchainDataHandler.readBlockchain();
        if (error != null) { // jeżeli nie udało się załadować blockchainu spadamy z rowerka
            Console.WriteLine($"nie udało się załadować blockchainu z powodu {error.Message}");
            Environment.Exit(1);
        }

        this.chain = storedChain;*/
    }

    // zwraca nam ilość elementów w recordzie
    public BlockType? AddRecord(messageRecord Record)
    {
        var rec = newestBlock.Records;
        rec.Add(Record);

        if (rec.Count >= 3)
        {
            var newBlock = newestBlock;
            newBlock.Hash = validator.calcHash(newBlock);
            newBlock.DataHash = validator.calcDataHash(newBlock);
            newestBlock = new BlockType();
            return newBlock;
        }

        return null;
    }

    public void CreateBlock(BlockType block)
    {
        _mutex.WaitOne();

        block.Hash = validator.calcHash(block);
        block.DataHash = validator.calcDataHash(block);

        chain.Add(block);
        _mutex.ReleaseMutex();
    }

    public void AddBlock(BlockType block)
    {
        _mutex.WaitOne();
        var err = validator.validate(block);
        if (err != null) // jeżeli blok nie ma poprawnie zdefiniowanego hasha chcemy go odrzucić
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

    // Metoda dostępna tylko w środowisku testowym
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