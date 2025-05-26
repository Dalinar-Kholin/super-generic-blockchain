using System.Text;
using Newtonsoft.Json;

namespace blockProject.blockchain;




public class BlockHeader
{
    public BlockHeader() { }
    public string PreviousHash { get; set; } = "";
    public string Hash { get; set; } = ""; // first 3 byte should be 0x0
    public string DataHash { get; set; } = ""; // block body hash
    public int Nonce { get; set; } = 0; // allows hash condition be set
    public int recordsInBlock { get; set; } = 0;
}

public class BlockBody
{
    public BlockBody() { }
    public byte[] Records { get; set; } = { };
}


public class Block
{
    // blockchain data
    public BlockBody body = new BlockBody();
    // hashes nonces 
    public BlockHeader header = new BlockHeader();

    [JsonConstructor]
    public Block() { }

    public Block(string previousHash)
    {
        header.PreviousHash = previousHash;
    }


    public int AddRecord(byte[] record)
    {
        byte[] combined = new byte[record.Length + body.Records.Length];

        Buffer.BlockCopy(body.Records, 0, combined, 0, body.Records.Length);
        Buffer.BlockCopy(record, 0, combined, body.Records.Length, record.Length);
        body.Records = combined;
        return ++header.recordsInBlock;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var v in body.Records) sb.Append(v + "\n");

        return $"{header.Hash} {header.Nonce} {header.DataHash}\n{sb}";
    }
}