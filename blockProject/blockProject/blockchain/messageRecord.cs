global using recordType = blockProject.blockchain.messageRecord;
using System.Security.Cryptography;
using System.Text;
using blockProject.httpServer;
using Newtonsoft.Json;

namespace blockProject.blockchain;




public class messageRecord
{
    public Guid id;
    
    public string from { get; set; }

    public string to { get; set; }
    
    
    // sama wiadomość będzie zaszyfrowana za pomocą AES-a a klucz do AES-a będzie zaszyforwany za pomocą kluczy asymetrycznych
    public byte[] tag { get; set; } = [];
    public byte[] iv { get; set; } = [];
    public byte[] message { get; set; } // jako iż może to być zaszyfrowane to string nie średnio się do tego nadaje
    public byte[] sign { get; set; }// podpis
    public bool isEncoded { get; set; }
    
    [JsonConstructor]
    public messageRecord(Guid id, string from, string to, byte[] tag, byte[] iv, byte[] message, byte[] sign, bool isEncoded)
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
        
        sign = ecdsaPrivate.SignData(message, HashAlgorithmName.SHA256);;
        isEncoded = iE;
        
    }


    public simpleMessage decrypt(Keys keys)
    {
        if (!isEncoded)
        {
            return new simpleMessage(from, to, Convert.ToString(message)!);
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
    
    
    
    public override string ToString()
    {
        return $"{from} == {message} ==> {to}, guid = {id} {sign}";
    }
    

    
    

}


public record simpleMessage(string from,string to, string message);