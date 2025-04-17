using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using Newtonsoft.Json;
using Record = blockProject.blockchain.Record;

namespace TestProject.testBlockchainDataHandler;
[Collection("SequentialTests")]
public class testDataHandler
{
    private static string getRandomString(int dlugosc)
    {
        const string znaki = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var wynik = new char[dlugosc];
        for (var i = 0; i < dlugosc; i++) wynik[i] = znaki[random.Next(znaki.Length)];
        return new string(wynik);
    }
    private List<Block> generateBlockchain()
    {
        var block = new Block("0x0");
        block.AddRecord(new Record("esssa", "w chuj"));
        block.AddRecord(new Record("456", "123"));
        var blockchain = Blockchain.GetInstance();
        blockchain.CreateBlock(block);
        var block1 = new Block(block.Hash);
        block.AddRecord(new Record("fajny", "pieniąź"));
        blockchain.CreateBlock(block1);
        block1 = new Block(block1.Hash);
        block1.AddRecord(new Record(getRandomString(5), getRandomString(5)));
        blockchain.CreateBlock(block1);
        block1 = new Block(block1.Hash);
        block1.AddRecord(new Record(getRandomString(5), getRandomString(5)));
        blockchain.CreateBlock(block1);
        block1 = new Block(block1.Hash);
        block1.AddRecord(new Record(getRandomString(5), getRandomString(5)));
        blockchain.CreateBlock(block1);
        return blockchain.chain;
    }
    
    
    // TODO: dodać testowanie usuwania, modyfikacji i dodawania tylko jednego bloku
    [Fact]
    [Trait("cat", "dataHandler")]
    public void properBlockchain()
    {
        var path = "/tmp/testBlockchainFile.json";
        var dh = singleFileBlockchainDataHandler.GetInstance();
        dh._filePath = path;
        
        // tworzymy blockchain
        var blockChain = generateBlockchain();
        var json = JsonConvert.SerializeObject(blockChain);
        Blockchain.Reset(); // wymazujemy z pamięci stary blockchain
        // zapisujemy blockchian
        Assert.Null(dh.writeBlockchain(blockChain));
        // porównujemy zapisany plik z tym co powinno być
        var file = File.ReadAllText(path);
        
        Assert.Equal(json, file); // to co w plik powinno być równe temu do cczego parsuje się JSON
        // czytamy blockchain
        var (res, err) = dh.readBlockchain();
        Assert.Null(err);
        // sprawdzamy czy się zgadza
        for (int i = 0; i < res.Count; i++)
            if (res[i].Hash != blockChain[i].Hash) Assert.Fail("wczytane blockchainy nie są takie same");
        
        Blockchain.Reset();
    }
    
    
    
}