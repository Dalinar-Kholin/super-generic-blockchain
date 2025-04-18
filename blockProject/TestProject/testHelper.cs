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
}