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
            header.possitionsOfRecords[0] = record.Length - 1;
        }
        else
        {
            header.possitionsOfRecords[header.recordsInBlock] = record.Length + header.possitionsOfRecords[header.recordsInBlock - 1];
        }
        return ++header.recordsInBlock;
    }

    // returns a record from a given position
    public Span<byte> GetRecordSpan(int position)
    {
        /*
        if (position < 0 || position >= header.recordsInBlock || header.possitionsOfRecords[position] == -1)
        {
            Console.WriteLine($"Invalid position: {position}, recordsInBlock: {header.recordsInBlock}, positionsOfRecords: {string.Join(", ", header.possitionsOfRecords)}");
        }
        */
        if (position < 0 || position >= header.recordsInBlock || header.possitionsOfRecords[position] == -1)
            throw new IndexOutOfRangeException("Invalid record position.");

        int start = position == 0 ? 0 : header.possitionsOfRecords[position - 1] + 1;
        int end = header.possitionsOfRecords[position];
        int length = end - start + 1;  // +1 ponieważ end to indeks ostatniego bajtu

        return new Span<byte>(body.Records, start, length);
    }

    public bool CompareRecords(Block block1, int pos1, Block block2, int pos2)
    {
        Span<byte> record1 = block1.GetRecordSpan(pos1);
        Span<byte> record2 = block2.GetRecordSpan(pos2);

        return record1.SequenceEqual(record2);
    }

    // do poprawienia
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var v in body.Records) sb.Append(v + "\n");

        return $"{header.Hash} {header.Nonce} {header.DataHash}\n{sb}";
    }
}