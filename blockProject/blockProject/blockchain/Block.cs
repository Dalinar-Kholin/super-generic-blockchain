using System.Text;
using Newtonsoft.Json;

namespace blockProject.blockchain;

public class Block
{
    // dla parsowania JSON
    [JsonConstructor]
    public Block()
    {
    }

    public Block( /*int index,*/ string previousHash)
    {
        //Index = index;
        PreviousHash = previousHash;
        // Timestamp = DateTime.UtcNow;
    }
    // public int Index { get; set; } // czy indexem bloku nie jest jego hash?
    // public DateTime Timestamp { get; set; } // po co timestamp?

    
    // todo: rozbić to na Hader i body blocku
    //todo: to powinno być []byte, wtedy mamy generic
    public byte[] Records { get; set; } = {};
    public string PreviousHash { get; set; } = "";
    public string Hash { get; set; } = ""; // ostatnie 3 liczby hasha w zapisie 0x powinny być 0
    public string DataHash { get; set; } = ""; // hash danych przechowywanych w bloku
    public int Nonce { get; set; } = 0; // liczba która pozwala na spełnienie warunku poprawności hasha
    public int recordsInBlock { get; set; } = 0;


    public int AddRecord(byte[] record)
    {
        byte[] combined = new byte[record.Length + Records.Length];

        Buffer.BlockCopy(Records, 0, combined, 0, Records.Length);
        Buffer.BlockCopy(record, 0, combined, Records.Length, record.Length);
        Records = combined;
        return ++recordsInBlock;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var v in Records) sb.Append(v + "\n");

        return $"{Hash} {Nonce} {DataHash}\n{sb}";
    }
}