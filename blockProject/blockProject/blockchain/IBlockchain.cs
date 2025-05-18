global using BlockType = blockProject.blockchain.Block; // alias na typ aktualnie używanego bloku
namespace blockProject.blockchain;



// uselless 
// jakie metody powinnien udostępniać nasz blockchain
public interface IBlockchain<T, Z>
{
    string GetParsedBlockchain(); // zwraca jako json

    T? AddRecord(Z Record);
    void CreateBlock(T block); // gdy MY tworzymy blok
    void AddBlock(T block); // gdy dostajemy blok

    List<BlockType> GetChain();
    void SetChain(List<BlockType> blockchain);
}