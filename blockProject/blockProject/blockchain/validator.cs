using System.Security.Cryptography;
using System.Text;

namespace blockProject.blockchain;

public interface IValidator
{
    public string calcDataHash(BlockType block);
    public string calcHash(BlockType block);
    public bool validate(BlockType block);
}

public class Validator : IValidator
{
    public string calcDataHash(BlockType block)
    {
        var data = new StringBuilder();

        foreach (var record in block.Records) data.Append(record);
        using (var sha512 = SHA512.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(data.ToString());
            var hashBytes = sha512.ComputeHash(inputBytes);
            // Convert byte array to a hexadecimal string
            var sb = new StringBuilder();
            foreach (var b in hashBytes) sb.Append(b.ToString("x2")); // Convert byte to hex
            return sb.ToString();
        }
    }

    public string calcHash(BlockType block)
    {
        var data = new StringBuilder();

        data.Append(block.PreviousHash);
        data.Append(block.DataHash);

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
                    block.Nonce = i;
                    return sb.ToString();
                }
            }
    }

    public bool validate(BlockType block)
    {
        return true;
    }
}