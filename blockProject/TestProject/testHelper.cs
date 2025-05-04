global using recordType = blockProject.blockchain.messageRecord;
using System.Security.Cryptography;
using System.Text;
using blockProject.httpServer;

namespace TestProject;

public class testHelper
{
    public static string getRandomString(int dlugosc)
    {
        const string znaki = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var wynik = new char[dlugosc];
        for (var i = 0; i < dlugosc; i++) wynik[i] = znaki[random.Next(znaki.Length)];

        return new string(wynik);
    }

    public static recordType getRandomDummyRecord()
    {
        using var receiver = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        byte[] receiverPrivateKey = receiver.ExportECPrivateKey();
        byte[] receiverPublicKey = receiver.ExportSubjectPublicKeyInfo();
        
        return new recordType("0x0", Encoding.ASCII.GetBytes("pojebaneoasdghfjhasdfjhvasdiofbasldskgfgahdbfoiajds;flkahflu gyeahrljghaoiusdfh"), new Keys(receiverPrivateKey, receiverPublicKey), false);
    }
}