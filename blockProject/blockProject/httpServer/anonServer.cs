using blockProject.blockchain;
using Microsoft.AspNetCore.Http;

namespace blockProject.httpServer;




public class anonServer
{
    public async Task getMessages(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var FindNthIndex = (byte[] data, int occurrence, int toFind) =>
        {
            int count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == toFind)
                {
                    count++;
                    if (count == occurrence)
                        return i;
                }
            }

            return -1; // nie znaleziono
        };
        
        
        var res = Blockchain.GetInstance().GetChain().Aggregate(
            new List<simpleMessage>(), (accumulate, block) =>
            {
                var data = block.body.Records;
                List<messageRecord> records = new List<messageRecord>();
                for (int i = 0; i < 3; i++)
                {
                    var index = FindNthIndex(data, 8, 0x0);
                    if (index==-1) break;
                    byte[] part = data.Take(index+1).ToArray(); // wyodrębnij segment jako byte[]
                    records.Add(new messageRecord(part));

                    // Zaktualizuj data, pomijając fragment i separator
                    data = data.Skip(index + 1).ToArray();
                }
                    
                records.Aggregate(new List<simpleMessage>(), (_, r) =>
                {
                    if (r.to == "0x0") accumulate.Add(r.decrypt(new Keys([], [])));
                    return null!;
                });
                return accumulate;
            }
        );
        
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = res
        });

    } 
}