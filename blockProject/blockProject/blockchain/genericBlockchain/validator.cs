using System.Security.Cryptography;
using System.Text;
using blockProject.blockchain.genericBlockchain;
using blockProject.randomSrc;

namespace blockProject.blockchain;

public interface IValidator
{
    public string calcDataHash(BlockType block);
    public string calcHash(BlockType block);
    public Error? validate(BlockType block);
}

public class Validator : IValidator
{

    // TODO: Replace with Merkle Trees
    public string calcDataHash(BlockType block)
    {
        var data = new StringBuilder();

        foreach (var record in block.body.Records)
        {
            data.Append(record.ToString());
        }

        using (var sha512 = SHA512.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(data.ToString());
            var hashBytes = sha512.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Convert byte to hex
            }

            return sb.ToString();
        }
    }

    public string calcHash(BlockType block)
    {
        var data = new StringBuilder();

        data.Append(block.header.PreviousHash);
        data.Append(block.header.DataHash);

        var stringableData = data.ToString();

        var i = 0;

        while (true)
            using (var sha512 = SHA512.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(stringableData + i++);
                var hashBytes = sha512.ComputeHash(inputBytes);
                // Convert byte array to a hexadecimal string
                var sb = new StringBuilder();

                foreach (var b in hashBytes) sb.Append(b.ToString("x2")); // Convert byte to hex
                var result = sb.ToString();
                if (result.Substring(0, 3) == "000")
                {
                    block.header.Nonce = i;
                    return result;
                }
            }
    }

    public Error? validate(BlockType block)
    {
        // Validate hashes
        if (calcDataHash(block) != block.header.DataHash || calcHash(block) != block.header.Hash)
        {
            return new Error("invalid hashes");
        }

        // Validate record count
        if (block.header.recordsInBlock > 3)
        {
            return new Error("too many records");
        }

        // Validate previous block exists
        var blockchain = Blockchain.GetInstance().GetChain();
		if (block.header.PreviousHash != "0" &&
	    blockchain.FindIndex(blk => blk.header.Hash == block.header.PreviousHash) == -1)
		{
			return new Error("Previous block doesn't exist in blockchain");
		}

		// Check if block already exists in blockchain
		if (blockchain.FindIndex(blk => blk.header.Hash == block.header.Hash) != -1)
        {
            return new Error("Block already in blockchain");
        }


        return null;
    }
}