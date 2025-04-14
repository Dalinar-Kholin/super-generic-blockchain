namespace blockProject.blockchain;

// jakie metody powinnien udostępniać nasz blockchain
public interface IBlockchain<T>
{
    string GetParsedBlockchain(); // zwraca jako json

    void CreateBlock(T block); // gdy MY tworzymy blok
    void AddBlock(T block); // gdy dostajemy blok

}