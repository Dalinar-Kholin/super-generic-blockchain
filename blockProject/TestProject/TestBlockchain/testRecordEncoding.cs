using System.Security.Cryptography;
using System.Text;
using blockProject.httpServer;

namespace TestProject.TestBlockchain;

public class testRecordEncoding
{
    
    [Fact]
    [Trait("cat", "testHashing")]
    public void encodingTest()
    {
        using var receiver = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        byte[] receiverPrivateKey = receiver.ExportECPrivateKey();
        byte[] receiverPublicKey = receiver.ExportSubjectPublicKeyInfo();
        var message = "chujdupa";

        var keys = new Keys(receiverPrivateKey, receiverPublicKey);
        var record = new recordType(Convert.ToBase64String(receiverPublicKey), Encoding.ASCII.GetBytes(message),keys);

        var decryptedMessage  = record.decrypt(keys);
        Assert.Equal(message, decryptedMessage.message);
    }

    [Fact]
    [Trait("cat", "testHashing")]
    public void testParsing()
    {
        var xd = testHelper.getRandomDummyRecord();
        Assert.Equal(xd.message, new recordType(xd.toByte()).message);
    }
    
    
}