namespace blockProject.blockchain;

public class Blockchain : IBlockchain
{
	private List<Block> _chain = new();
	private Mutex _mutex = new();
	private static Blockchain? _instance;

	public Blockchain()
	{
		LoadData();
	}

	public void AddBlock(Block block)
	{
		_mutex.WaitOne();
		block.ComputeHash();
		_chain.Add(block);
		BlockManager.SaveBlockchain(_chain);
		_mutex.ReleaseMutex();
	}

	public static Blockchain GetInstance()
	{
		return _instance ??= new Blockchain();
	}

	public string GetBlockchain()
	{
		return System.Text.Json.JsonSerializer.Serialize(_chain);
	}

	public void LoadData()
	{
		_chain = BlockManager.LoadBlockchain();
	}

	public void BroadcastBlock(Block block)
	{
		// todo: dodać prawdziwą logikę wysyłania do innych węzłów
		Console.WriteLine("Broadcasting block: " + BlockManager.SerializeBlock(block));
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