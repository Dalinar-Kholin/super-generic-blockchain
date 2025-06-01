using System.Security.Cryptography;
using System.Text;
using blockProject.blockchain;
using blockProject.blockchain.genericBlockchain;
using blockProject.httpServer;

namespace TestProject.TestBlockchain;

[Collection("SequentialTests")]
public class testHashing
{
    [Trait("cat", "genBlock")]
    //[Fact]
    internal void generateBlockchain()
    {
        string privateKey = File.ReadAllText("/home/dalinarkholin/Uczelnia/nokiaProject/super-generic-blockchain/blockProject/blockProject/p521_key.pem");
        var ecdsaPrivate = ECDsa.Create();
        
        ecdsaPrivate.ImportFromPem(privateKey);
        
        var messageBytes = RandomNumberGenerator.GetBytes(64);

        byte[] signature = ecdsaPrivate.SignData(messageBytes, HashAlgorithmName.SHA256);
        bool isValid = ecdsaPrivate.VerifyData(messageBytes, signature, HashAlgorithmName.SHA256);
        if (!isValid)
        {
            throw new Exception("bad keys");
        }
        
        
        JsonKeyMaster.loadServerKeys(new Keys(ecdsaPrivate.ExportECPrivateKey(), ecdsaPrivate.ExportSubjectPublicKeyInfo()));
        
        
        using var sender = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var keys = new Keys(sender.ExportECPrivateKey(), sender.ExportSubjectPublicKeyInfo());
        
        using var sender2 = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var keys2 = new Keys(sender2.ExportECPrivateKey(), sender2.ExportSubjectPublicKeyInfo());
        
        
        using var sender3 = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var keys3 = new Keys(sender3.ExportECPrivateKey(), sender3.ExportSubjectPublicKeyInfo());
        
        using var sender4 = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var keys4 = new Keys(sender4.ExportECPrivateKey(), sender4.ExportSubjectPublicKeyInfo());
        
        
        
        var block = Blockchain.GetInstance();
        block.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block.AddRecord(testHelper.getRandomDummyRecord().toByte());
        /*Assert.Equal(
            "0003e880acb04805d6520e7f602bf5b2e44cb211fe8c3e004c6d0299e97808cac17a194fce19ce686f4d13ac3acaaabdac9df412ec8fb45482d65ec4ef4ae7e1",
            block.Hash);
        Assert.Equal(
            "18ec8a3ac375e00134e744d1e3379bb697c94dd7ba4b6245b4ea05bb99fd22fa10ae12dc2fcbb2d62dd4c00e3bf3fdfe94087a1b005654d99bb2cde27e8db814",
            block.DataHash);
        Assert.Equal(15516, block.Nonce);*/
        var block1 = Blockchain.GetInstance();
        block.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(new messageRecord( Convert.ToBase64String(keys2.PublicKey), Encoding.ASCII.GetBytes("ale mamm esse"), keys,0.10f).toByte());
        block1.AddRecord(new messageRecord( Convert.ToBase64String(keys3.PublicKey), Encoding.ASCII.GetBytes("juz nie mam essy"), keys,0.10f).toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(new messageRecord( Convert.ToBase64String(keys4.PublicKey), Encoding.ASCII.GetBytes("ale mam essa znowu"), keys2,0.10f).toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        for (int i = 0; i < 4; i++)
        {
            block1.AddRecord(testHelper.getRandomDummyRecord().toByte());
        }
        
        Console.WriteLine($"blockchain := {block1.GetParsedBlockchain()}");
        
        Blockchain.Reset();
    }

    [Trait("cat", "blockchainTest")]
    [Fact]
    public void Test1()
    {
        var block = new Block("0x0");
        block.AddRecord(testHelper.getRandomDummyRecord().toByte());
        block.AddRecord(testHelper.getRandomDummyRecord().toByte());
        var blockchain = Blockchain.GetTestInstance();
        blockchain.CreateBlock(block);
        /*Assert.Equal(
            "0003e880acb04805d6520e7f602bf5b2e44cb211fe8c3e004c6d0299e97808cac17a194fce19ce686f4d13ac3acaaabdac9df412ec8fb45482d65ec4ef4ae7e1", // ostatni znak powinien być 1
            block.Hash);
        Assert.Equal(
            "18ec8a3ac375e00134e744d1e3379bb697c94dd7ba4b6245b4ea05bb99fd22fa10ae12dc2fcbb2d62dd4c00e3bf3fdfe94087a1b005654d99bb2cde27e8db814",
            block.DataHash);
        Assert.Equal(15516, block.Nonce);*/
        // mając losowe GUIDy nie mogę oczekiwać jakiś konkretnych hashy


        //Console.WriteLine(block.ToString());
    }


    [Trait("cat", "blockchainTest")]
    [Fact]
    public void Test2()
    {
        var block = new Block("0x0");
        block.AddRecord(testHelper.getRandomDummyRecord().toByte());
        var blockchain = Blockchain.GetTestInstance();
        blockchain.CreateBlock(block);
        Assert.True(block.header.Hash.Substring(0, 3) == "000");
    }
}