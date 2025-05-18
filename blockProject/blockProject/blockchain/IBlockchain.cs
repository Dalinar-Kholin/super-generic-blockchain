global using BlockType = blockProject.blockchain.Block; 
namespace blockProject.blockchain;

public interface IBlockchain<T, Z>
{
    string GetParsedBlockchain(); // returns json

    T? AddRecord(Z Record);
    void CreateBlock(T block); 
    void AddBlock(T block); 

    List<BlockType> GetChain();
    void SetChain(List<BlockType> blockchain);
}