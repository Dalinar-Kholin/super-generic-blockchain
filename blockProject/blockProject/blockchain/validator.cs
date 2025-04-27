using System.Security.Cryptography;
using System.Text;
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
    
    // todo: zmienić na drzewa merkele
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

    public Error? validate(BlockType block)
    {
        // sprawdzanie hashy
        if (calcDataHash(block) != block.DataHash || calcHash(block) != block.Hash) return new Error("bad hashesh");
        // sprawdzanie rekordu danych
        if (block.Records.Count > 3) return new Error("to many records");
        // sprawdzenie czy taki poprzednik jest w naszym blockchainie
        var blockchain = Blockchain.GetInstance().GetChain();
        if (blockchain.FindIndex(blk => blk.Hash == block.PreviousHash) == -1)
            return new Error("previous block doesnt exist in blockchain");
        // czy ten blok już nie znajduje się w blockchainie
        if (blockchain.FindIndex(blk => blk.Hash == block.Hash) != -1) return new Error("block already in blockchain");
        // sprawdzenie czy rekordy które chcemy dodać nie znajdują się już w blockchainine
        // todo: na razie wyłączone, czy to jest wgl potrzebne???
        var dataValidation = isDataValid(block);
        if (dataValidation != null) return dataValidation;
        return null;
        // return (from bl in blockchain from rec in bl.Records from newRecords in block.Records select newRecords).All(newRecords => block.Records.FindIndex(rec => rec.id == newRecords.id) == -1) ? new Error("record already in blockchain") : null;
    }

// czy dane podane w bloku są poprawne, np czy użytkownik ma odpowienią ilość waluty itd 
    public Error? isDataValid(BlockType blk)
    {
        return null;
    }
}