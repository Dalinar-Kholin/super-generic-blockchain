global using recordType = blockProject.blockchain.messageRecord; // alias na typ aktualnie używanego bloku
namespace blockProject.blockchain;




public class messageRecord
{
    public Guid id;
    
    public string from { get; set; }

    public string to { get; set; }
    
    
    // sama wiadomość będzie zaszyfrowana za pomocą AES-a a klucz do AES-a będzie zaszyforwany za pomocą kluczy asymetrycznych
    public byte[] aesKey { get; set; } = [];
    public byte[] IV { get; set; } = [];
    public byte[] message { get; set; } // jako iż może to być zaszyfrowane to string nie średnio się do tego nadaje
    public string sign { get; set; }
    public bool isEncoded { get; set; }
    
    public messageRecord(string f, string t, byte[] m, string s, bool iE = true)
    {
        id = Guid.NewGuid();
        from = f;
        to = t;
        message = m;
        sign = s;
        isEncoded = iE;
    }
    
    public override string ToString()
    {
        return $"{from} == {message} ==> {to}, guid = {id} {sign}";
    }
}