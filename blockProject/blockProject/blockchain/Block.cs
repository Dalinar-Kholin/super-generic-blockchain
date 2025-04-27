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
    public List<messageRecord> Records { get; set; } = new();
    public string PreviousHash { get; set; } = "";
    public string Hash { get; set; } = ""; // ostatnie 3 liczby hasha w zapisie 0x powinny być 0
    public string DataHash { get; set; } = ""; // hash danych przechowywanych w bloku
    public int Nonce { get; set; } = 0; // liczba która pozwala na spełnienie warunku poprawności hasha
    
    
    
    public void AddRecord(messageRecord record)
    {
        Records.Add(record);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var v in Records) sb.Append(v + "\n");

        return $"{Hash} {Nonce} {DataHash}\n{sb}";
    }
}