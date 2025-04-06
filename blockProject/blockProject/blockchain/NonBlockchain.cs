using System.Text.Json;

namespace blockProject.blockchain;

// zawartość blockchainu
internal struct NoneBlock
{
    private List<string> _block = new List<string>();

    public NoneBlock() { }

    public void AddBlock(string message)
    {
        _block.Add(message);
    }

    public List<string> GetBlock()
    {
        return _block;
    }
}


//klasa do obsługi blockchainu
public class NonBlockChain : IBlockchain
{

    // private IValidate validator ?? pojawiennie się jakiejś klasy validatora?

    private static NonBlockChain? _instance;
    private Mutex _mut;
    private NoneBlock _blockchain = new NoneBlock();
    private NonBlockChain()
    {
        _mut = new Mutex();
    }

    public static NonBlockChain GetInstance()
    {
        return _instance ??= new NonBlockChain();
    }

    public string GetBlockchain() // zakładam że domyślnym typem komunikacji między węzłami jest JSON m
    {
        return JsonSerializer.Serialize(_blockchain.GetBlock());
    }
}