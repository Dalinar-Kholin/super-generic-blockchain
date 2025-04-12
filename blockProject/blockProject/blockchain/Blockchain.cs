global using BlockType = blockProject.blockchain.Block; // alias na typ aktualnie używanego bloku
using System.Text.Json;

namespace blockProject.blockchain;

public class Blockchain : IBlockchain<BlockType>
{
    private static Blockchain? _instance;
    private readonly Mutex _mutex = new();
    private readonly IValidator validator = new Validator();
    private List<BlockType> _chain = new();


    private Blockchain()
    {
        LoadData();
    }

    public void setChain(List<BlockType> chain)
    {
        _chain = chain;
    }
    
    public void CreateBlock(BlockType block)
    {
        _mutex.WaitOne();

        block.Hash = validator.calcHash(block);
        block.DataHash = validator.calcDataHash(block);

        _chain.Add(block);

        //BlockManager.SaveBlockchain(_chain); // robi straszny syf podczas testów, wyekstrachowałbym to do osobnej funkcji

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

        _chain.Add(block);
        //BlockManager.SaveBlockchain(_chain);
        _mutex.ReleaseMutex();
    }

    public string GetParsedBlockchain()
    {
        return JsonSerializer.Serialize(_chain);
    }
    
    public List<BlockType> GetBlockchain()
    {
        return _chain;
    }

    public void LoadData()
    {
        _chain = BlockManager.LoadBlockchain();
    }

    public void BroadcastBlock(BlockType block)
    {
        // todo: dodać prawdziwą logikę wysyłania do innych węzłów
        // to nie jest zadanie blockchainu tylko sendera
        Console.WriteLine("Broadcasting block: " + BlockManager.SerializeBlock(block));
    }

    public static Blockchain GetInstance()
    {
        return _instance ??= new Blockchain();
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