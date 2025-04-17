global using BlockType = blockProject.blockchain.Block; // alias na typ aktualnie używanego bloku
using System.Text.Json;
using blockProject.nodeCommunicatio;

namespace blockProject.blockchain;

public class Blockchain : IBlockchain<BlockType>
{
    private static Blockchain? _instance;
    
    private readonly Mutex _mutex = new();
    
    private readonly IValidator validator = new Validator();
    public List<BlockType> chain = new ();
    private readonly IblockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();
    
    // najowszy blok gdzie będziemy przekazywać dane rekordu
    // a następnie przy spełnieniu warunków jest commitowany do blockchainu
    public BlockType newestBlock = new BlockType(); 

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
        if (!validator.validate(block)) // jeżeli blok nie ma poprawnie zdefiniowanego hasha chcemy go odrzucić
        {
            _mutex.ReleaseMutex();
            return;
        }

        chain.Add(block);
        _mutex.ReleaseMutex();
    }

    public string GetParsedBlockchain()
    {
        return JsonSerializer.Serialize(chain);
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