using System.Security.Cryptography;
using blockProject.httpServer;

namespace TestProject.testHttpServer;


[Collection("SequentialTests")]
public class testKeyMaster
{

    [Trait("cat", "keyMaster")]
    [Fact]
    public void testStoring()
    {
        using var receiver = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        byte[] privateKey = receiver.ExportECPrivateKey();
        byte[] publicKey = receiver.ExportSubjectPublicKeyInfo();

        var username = testHelper.getRandomString(64);
        var password = testHelper.getRandomString(64);

        var keyMaster = new JsonKeyMaster();
        JsonKeyMaster.path = "../../../.KeyFile";
        var error = keyMaster.deepStore(new Keys(privateKey, publicKey), username, password);
        Assert.Null(error);
        var (uuid, err) = keyMaster.loadKeys(username, password);
        Assert.Null(err);
        var (_, err1) = keyMaster.loadKeys(username, "hehXD");
        Assert.NotNull(err1);

        var keys = keyMaster.getKeys(uuid);
        Assert.Null(keys.err);
        Assert.True(privateKey.SequenceEqual(keys.keys.PrivateKey));
    }
}