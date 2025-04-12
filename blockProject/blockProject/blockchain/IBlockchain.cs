namespace blockProject.blockchain;

// jakie metody powinnien udostępniać nasz blockchain
public interface IBlockchain<T>
{
    string GetParsedBlockchain(); // zwraca jako json

    void CreateBlock(T block); // gdy MY tworzymy blok
    void AddBlock(T block); // gdy dostajemy blok
    void LoadData(); // wczytuje z pliku

    void
        BroadcastBlock(T block); // rozsyłanie bloku -- to nie jest zadanie blockchainu, wolałbym uniknąć tworzeniu God objectu 
}