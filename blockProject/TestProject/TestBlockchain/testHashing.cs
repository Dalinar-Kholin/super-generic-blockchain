using blockProject.blockchain;
using Record = blockProject.blockchain.Record;

namespace TestProject.TestBlockchain;

public class testHashing
{
    /*[Trait("cat", "blockchainTest")]
    [Fact]
    public void generateBlockchain()
    {
        var block = new Block("0x0");
        block.AddRecord(new Record("esssa", "w chuj"));
        block.AddRecord(new Record("456", "123"));
        var blockchain = Blockchain.GetInstance();
        blockchain.CreateBlock(block);
        Assert.Equal(
            "0003e880acb04805d6520e7f602bf5b2e44cb211fe8c3e004c6d0299e97808cac17a194fce19ce686f4d13ac3acaaabdac9df412ec8fb45482d65ec4ef4ae7e1",
            block.Hash);
        Assert.Equal(
            "18ec8a3ac375e00134e744d1e3379bb697c94dd7ba4b6245b4ea05bb99fd22fa10ae12dc2fcbb2d62dd4c00e3bf3fdfe94087a1b005654d99bb2cde27e8db814",
            block.DataHash);
        Assert.Equal(15516, block.Nonce);
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
        Console.WriteLine($"chain Data := {blockchain.GetParsedBlockchain()}");
        Blockchain.Reset();
    }*/
    

    [Trait("cat", "blockchainTest")]
    [Fact]
    public void Test1()
    {
        var block = new Block("0x0");
        block.AddRecord(new Record("esssa", "w chuj"));
        block.AddRecord(new Record("456", "123"));
        var blockchain = Blockchain.GetInstance();
        blockchain.CreateBlock(block);
        Assert.Equal(
            "0003e880acb04805d6520e7f602bf5b2e44cb211fe8c3e004c6d0299e97808cac17a194fce19ce686f4d13ac3acaaabdac9df412ec8fb45482d65ec4ef4ae7e1",
            block.Hash);
        Assert.Equal(
            "18ec8a3ac375e00134e744d1e3379bb697c94dd7ba4b6245b4ea05bb99fd22fa10ae12dc2fcbb2d62dd4c00e3bf3fdfe94087a1b005654d99bb2cde27e8db814",
            block.DataHash);
        Assert.Equal(15516, block.Nonce);

        Blockchain.Reset();
        //Console.WriteLine(block.ToString());
    }


    public static string getRandomString(int dlugosc)
    {
        const string znaki = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var wynik = new char[dlugosc];
        for (var i = 0; i < dlugosc; i++) wynik[i] = znaki[random.Next(znaki.Length)];

        return new string(wynik);
    }

    [Trait("cat", "blockchainTest")]
    [Fact]
    public void Test2()
    {
        var block = new Block("0x0");
        var random = getRandomString(10);
        block.AddRecord(new Record(random, random));
        var blockchain = Blockchain.GetInstance();
        blockchain.CreateBlock(block);
        Assert.True(block.Hash.Substring(0, 3) == "000");
        Blockchain.Reset();
    }
}