global using recordType = blockProject.blockchain.messageRecord;
using System.Text;

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

    public static recordType getRandomRecord()
    {
        return new recordType(getRandomString(5), getRandomString(5),
            Encoding.ASCII.GetBytes(getRandomString(20)) , getRandomString(5), false);
    }
}