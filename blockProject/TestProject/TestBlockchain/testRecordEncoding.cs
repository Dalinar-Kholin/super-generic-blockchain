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
    
    [Fact]
    [Trait("cat", "testHashing")]
    public void testSign()
    {
        using var receiver = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        using var sender = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        
        byte[] senderPrivateKey = sender.ExportECPrivateKey();
        byte[] senderPublicKey = sender.ExportSubjectPublicKeyInfo();
        
        byte[] receiverPublicKey = receiver.ExportSubjectPublicKeyInfo();

        var messageRecord = new recordType(Convert.ToBase64String(receiverPublicKey), Encoding.ASCII.GetBytes("pojebaneoasdghfjhasdfjhvasdiofbasldskgfgahdbfoiajds;flkahflu gyeahrljghaoiusdfh"), new Keys(senderPrivateKey, senderPublicKey), false);
        
        Assert.Equal(messageRecord.message, new recordType(messageRecord.toByte()).message);
        
        Assert.Null(messageRecord.validate(Convert.ToBase64String(senderPublicKey)));
        messageRecord.message[9] = (byte)(messageRecord.message[9] ^ -1);
        Assert.NotNull(messageRecord.validate(Convert.ToBase64String(senderPublicKey)));
    }
    
    
}