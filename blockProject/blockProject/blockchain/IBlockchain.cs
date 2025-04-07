namespace blockProject.blockchain;


// jakie metody powinnien udostępniać nasz blockchain
public interface IBlockchain
{
	string GetBlockchain(); // zwraca jako json
	void AddBlock(Block block);
	void LoadData(); // wczytuje z pliku
	void BroadcastBlock(Block block); // rozsyłanie bloku 
}


