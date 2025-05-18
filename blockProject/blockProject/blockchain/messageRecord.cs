global using recordType = blockProject.blockchain.messageRecord;
using System.Security.Cryptography;
using System.Text;
using blockProject.httpServer;
using Newtonsoft.Json;

namespace blockProject.blockchain;




public class messageRecord
{
    public Guid id;

    public string from { get; set; } = "";

    public string to { get; set; }= "";


    // sama wiadomość będzie zaszyfrowana za pomocą AES-a a klucz do AES-a będzie zaszyforwany za pomocą kluczy asymetrycznych
    public byte[] tag { get; set; } = [];
    public byte[] iv { get; set; } = [];
    public byte[] message { get; set; } = { }; // jako iż może to być zaszyfrowane to string nie średnio się do tego nadaje
    public byte[] sign { get; set; } ={ };// podpis
    public bool isEncoded { get; set; }

    [JsonConstructor]
    public messageRecord(Guid id, string from, string to, byte[] tag, byte[] iv, byte[] message, byte[] sign,
        bool isEncoded)
    {
        this.id = id;
        this.from = from;
        this.to = to;
        this.tag = tag;
        this.iv = iv;
        this.message = message;
        this.sign = sign;
        this.isEncoded = isEncoded;
    }

    public messageRecord(string t, byte[] m, Keys keys, bool iE = true)
    {
        id = Guid.NewGuid();
        from = Convert.ToBase64String(keys.PublicKey);
        to = t; // w base 64

        iv = RandomNumberGenerator.GetBytes(12); // 12B nonce for AES-GCM

        byte[] senderPrivateKey = keys.PrivateKey;

        using var senderStatic = ECDiffieHellman.Create();
        senderStatic.ImportECPrivateKey(senderPrivateKey, out _);

        if (iE && t != "0x0")
        {
            byte[] ciphertext = new byte[m.Length];
            tag = new byte[16]; // 16B tag
            byte[] receiverPublicKey = Convert.FromBase64String(t);
            using var receiverPub = ECDiffieHellman.Create();
            receiverPub.ImportSubjectPublicKeyInfo(receiverPublicKey, out _);
            byte[] key = senderStatic.DeriveKeyMaterial(receiverPub.PublicKey);
            using var aes = new AesGcm(key);
            aes.Encrypt(iv, m, ciphertext, tag);
            message = ciphertext;
        }
        else
        {
            message = m;
        }

        using var ecdsaPrivate = ECDsa.Create();
        ecdsaPrivate.ImportECPrivateKey(senderPrivateKey, out _);

        sign = ecdsaPrivate.SignData(message, HashAlgorithmName.SHA256);
        ;
        isEncoded = iE;

    }


    public simpleMessage decrypt(Keys keys)
    {
        if (!isEncoded)
        {
            return new simpleMessage(from, to, Encoding.ASCII.GetString(message));
        }


        byte[] decryptedBytes = new byte[message.Length];


        byte[] senderPrivateKey = keys.PrivateKey;

        using var senderPublic = ECDiffieHellman.Create();
        senderPublic.ImportSubjectPublicKeyInfo(Convert.FromBase64String(from), out _);

        using var reciverPrivate = ECDiffieHellman.Create();
        reciverPrivate.ImportECPrivateKey(keys.PrivateKey, out _);

        byte[] key = reciverPrivate.DeriveKeyMaterial(senderPublic.PublicKey);

        try
        {
            using (var aes = new AesGcm(key))
            {
                aes.Decrypt(iv, message, tag, decryptedBytes);
            }
        }
        catch (Exception)
        {
            return new simpleMessage("", "", "");
        }

        return new simpleMessage(from, to, Encoding.ASCII.GetString(decryptedBytes));
    }

    public messageRecord() { }

    public messageRecord(byte[] data)
    {
        var len = data.Length;
        var tail = data.Skip(len-22).ToArray();
        // Tablica setterów — każda przypisuje dane do jednego pola newRecord
        Action<byte[]>[] tab = new Action<byte[]>[]
        {
            part => id = new Guid(Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(part)))),
            part => from = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(part))),
            part => to = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(part))),
            part => tag = Convert.FromBase64String(Encoding.UTF8.GetString(part)),
            part => iv = Convert.FromBase64String(Encoding.UTF8.GetString(part)),
            part => message = Convert.FromBase64String(Encoding.UTF8.GetString(part)),
            part => sign = Convert.FromBase64String(Encoding.UTF8.GetString(part)),
            part => isEncoded = part[0] == 0x01
        };

        var i = 0;
        foreach (var setter in tab)
        {
            // Szukamy indeksu separatora (0x00)
            int zeroIndex = Array.FindIndex(data, x => x== 0x0);
            if (zeroIndex == -1)
                throw new Exception($"Nieprawidłowy format danych wejściowych — brakuje separatora 0x00 _{string.Join(" ", tail.Select(b => $"0x{b:X2}"))}_ _{i}_");

            byte[] part = data.Take(zeroIndex).ToArray(); // wyodrębnij segment jako byte[]
            setter(part);

            // Zaktualizuj data, pomijając fragment i separator
            data = data.Skip(zeroIndex + 1).ToArray();
            i+= zeroIndex+1;
        }
        
    }

    public byte[] toByte()
    {
        var baseId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id.ToString()));
        var baseTo = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(to));
        var baseFrom = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(from));
        var baseTag = Convert.ToBase64String(tag);
        var baseIv = Convert.ToBase64String(iv);
        var baseMessage = Convert.ToBase64String(message);
        var baseSign = Convert.ToBase64String(sign);
        
        
        var byteLength = baseId.Length+1 + 
                         baseFrom.Length + 1 /*one extra space for 0x0 byte to separate fileds*/ +
                         /*additionaly we cant allow field to have 0x0 byte insite so we have to base Encoded all fields*/
                         baseTo.Length + 1 +
                         baseTag.Length + 1 +
                         baseIv.Length + 1 +
                         baseMessage.Length + 1 +
                         baseSign.Length + 1 +
                         1/*is eco*/ + 1;
        byte[] res = new byte[byteLength];
        var offset = 0;

        var sourceTab = new[] { baseId, baseFrom, baseTo, baseTag, baseIv, baseMessage, baseSign };

        foreach (var str in sourceTab)
        {
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(str), 0, res, offset, str.Length);
            offset += str.Length;
            res[offset++] = 0x0;
        }

        res[offset++] = (byte)(isEncoded ? 0x01 : 0x02);
        res[offset] = 0x0;
        return res;
    }

    public override string ToString()
    {
        return $"{from} == {message} ==> {to}, guid = {id} {sign}";
    }
    

    
    

}


public record simpleMessage(string from,string to, string message);