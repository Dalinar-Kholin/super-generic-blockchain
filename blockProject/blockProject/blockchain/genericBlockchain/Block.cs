using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace blockProject.blockchain;




public class BlockHeader
{
    public BlockHeader() { }

    public byte[] miner { get; set; } = [];
    public string PreviousHash { get; set; } = "";
    public string Hash { get; set; } = ""; // first 3 byte should be 0x0
    public string DataHash { get; set; } = ""; // block body hash
    public int Nonce { get; set; } = 0; // allows hash condition be set
    public int recordsInBlock { get; set; } = 0;
    public List<int> possitionsOfRecords { get; set; } = Enumerable.Repeat(-1, 20).ToList();
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

        // update possitions of records end
        if (header.recordsInBlock == 0)
        {
            header.possitionsOfRecords[0] = record.Length;
        }
        else
        {
            header.possitionsOfRecords[header.recordsInBlock] = record.Length + header.possitionsOfRecords[header.recordsInBlock - 1];
        }
        return ++header.recordsInBlock;
    }

    // naprawic
    // returns a record from a given position
    public Memory<byte> GetRecord(int position)
    {
        if (header.possitionsOfRecords[position] == -1) throw new IndexOutOfRangeException();
        int start = position == 0 ? 0 : header.possitionsOfRecords[position - 1];
        int end = header.possitionsOfRecords[position];

        return new Memory<byte>(body.Records, start, end);
    }

    // naprawic
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var v in body.Records) sb.Append(v + "\n");

        return $"{header.Hash} {header.Nonce} {header.DataHash}\n{sb}";
    }
}