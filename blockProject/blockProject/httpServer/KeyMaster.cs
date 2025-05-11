using System.Security.Cryptography;
using System.Text;
using blockProject.randomSrc;
using Konscious.Security.Cryptography;
using Newtonsoft.Json;

namespace blockProject.httpServer;

// szyfrowanie rekordów jest prawie gotowe
public record Keys(byte[] PrivateKey, byte[] PublicKey)
{
    public static List<Keys>? LoadFromFile(string v)
    {
        throw new NotImplementedException();
    }
}

public interface IKeyMaster
{
    (string uuid, Error?) loadKeys(string username, string password); // załadowanie kluczy do szybkiej pamięcii danie im uuid
    (Keys keys, Error? err) getKeys(string uuid); // pobranie kluczy
    Error? deepStore(Keys keys, string username, string password); // storowanie kluczy w jakiej instancji trwałej pamięci
}


public class DatabaseRecord(byte[] privateKey, byte[] publicKey, string username, byte[] iv, byte[] salt, byte[] tag)
{
    public byte[] privateKey = privateKey;
    public byte[] publicKey = publicKey;
    public byte[] iv = iv;
    public byte[] salt = salt;
    public string username = username;
    public byte[] tag = tag;
}



// zarządca kluczy trzymający klucze w pliku .json
// potem mozna to zmienic na jakiego redisa czy inne tego typu rzeczy
public class JsonKeyMaster : IKeyMaster
{
    public static string path = "../../../../blockProject/.KeyFile"; // ".KeyFile"; // ścieżka do pliku z kluczami na razie JSON
    private static Mutex _mut = new();

    // cahce naszych kluczy 
    private static Dictionary<string, Keys> hashMap = new();

    public (string uuid, Error?) loadKeys(string username, string password)
    {
        _mut.WaitOne();
        var json = File.ReadAllText(path);
        _mut.ReleaseMutex();
        var records = JsonConvert.DeserializeObject<List<DatabaseRecord>>(json) ?? new List<DatabaseRecord>();
        var userData = records.Find(u => u.username == username);
        if (userData == null)
        {
            return ("", new Error("there is no user"));
        }

        byte[] salt = userData.salt;
        byte[] nonce = userData.iv;
        byte[] tag = userData.tag;
        byte[] ciphertext = userData.privateKey;

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            MemorySize = 65536,
            Iterations = 3
        };
        byte[] key = argon2.GetBytes(32);

        byte[] decryptedBytes = new byte[ciphertext.Length];

        try
        {
            using (var aes = new AesGcm(key))
            {
                aes.Decrypt(nonce, ciphertext, tag, decryptedBytes);
            }
        }
        catch (Exception)
        {
            return ("", new Error("bad password"));
        }


        //sprawdznie czy klucz jest poprawny i spójny
        using var ecdsaPrivate = ECDsa.Create();
        ecdsaPrivate.ImportECPrivateKey(decryptedBytes, out _);

        using var ecdsaPublic = ECDsa.Create();
        ecdsaPublic.ImportSubjectPublicKeyInfo(userData.publicKey, out _);

        // Przykładowa wiadomość
        var messageBytes = RandomNumberGenerator.GetBytes(64);

        byte[] signature = ecdsaPrivate.SignData(messageBytes, HashAlgorithmName.SHA256);
        bool isValid = ecdsaPublic.VerifyData(messageBytes, signature, HashAlgorithmName.SHA256);

        if (!isValid) return ("", new Error("bad password"));

        // jeżeli klucz jest poprawny, dodajemy go do cacha i 
        var uuid = Guid.NewGuid().ToString();
        hashMap.Add(uuid, new Keys(decryptedBytes, userData.publicKey));

        return (uuid, null);
    }

    public (Keys keys, Error? err) getKeys(string uuid)
    {
        var res = hashMap.GetValueOrDefault(uuid);
        return res != null ? (res, null) : (new Keys(new byte[] { }, new byte[] { }), new Error("bad uuid"));
    }


    //wywoływane przy rejestracji konta, powoduje trwałe zapamiętanie kluczy dla danego konta
    public Error? deepStore(Keys keys, string username, string password)
    {

        if (!File.Exists(path))
        {
            return new Error("there is no selected file ");
        }

        _mut.WaitOne();
        var json = File.ReadAllText(path);
        _mut.ReleaseMutex();
        var records = JsonConvert.DeserializeObject<List<DatabaseRecord>>(json) ?? new List<DatabaseRecord>();

        if (records.FindIndex(e => e.username == username) != -1)
        {
            return new Error("user already exists");
        }

        byte[] salt = RandomNumberGenerator.GetBytes(16); // 16B salt

        // Derive key with Argon2id
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4, // threads
            MemorySize = 65536,      // KB (64 MB)
            Iterations = 3           // iterations
        };
        byte[] key = argon2.GetBytes(32); // 32B = 256-bit key

        // Encrypt something with AES-GCM
        byte[] iv = RandomNumberGenerator.GetBytes(12); // 12B nonce for AES-GCM

        using var aes = new AesGcm(key);
        byte[] plaintextBytes = keys.PrivateKey;
        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] tag = new byte[16]; // 16B tag

        aes.Encrypt(iv, plaintextBytes, ciphertext, tag);
        // szyfrujemy tylko prywatny bo publiczny i tak nie ma znaczenia 

        // zapisujemy nowe dane do pliku
        records.Add(new DatabaseRecord(ciphertext, keys.PublicKey, username, iv, salt, tag));
        _mut.WaitOne();
        var parsedJson = JsonConvert.SerializeObject(records);
        try
        {
            File.WriteAllText(path, parsedJson);
        }
        catch (Exception e)
        {
            return new Error(e.Message);
        }

        _mut.ReleaseMutex();
        return null;
    }
}