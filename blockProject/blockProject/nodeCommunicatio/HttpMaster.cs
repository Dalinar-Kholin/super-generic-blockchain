using System.Net;
using blockProject.blockchain;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace blockProject.nodeCommunicatio;


public class LoginMaster
{
    private readonly BlockKeyMaster keys;

    public async Task addUser()
    {
        
    } 
    public async Task login()
    {
        
    } 
}



public class HttpMaster
{
    private readonly DataSender sender;
    private readonly long startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public HttpMaster(DataSender sender)
    {
        this.sender = sender;
    }

    // wysłanie bloku do sąsiadów
    public async Task SendBlock(HttpContext context)
    {
        // tutaj jest chyba więcej logiki do dodania
        var errorResult = await sender.SendData(Blockchain.GetInstance().newestBlock);

        if (errorResult != null)
        {
            Console.WriteLine($"Błąd: {errorResult.Message}");
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = errorResult.Message
            });
            return;
        }

        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = "Block sent successfully"
        });
    }

    public async Task AddNewNode(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        if (context.Request.Query.TryGetValue("port", out var port) &&
            context.Request.Query.TryGetValue("ip", out var ip))
        {
            var destPort = port.ToString();
            Console.WriteLine(ip.ToString());

            sender.AddIP(new IPEndPoint(IPAddress.Parse(ip.ToString()), int.Parse(destPort)));
            Console.WriteLine("dodano nowy węzeł");

            var errorResult = await sender.ReceiveBlockchain();
            if (errorResult != null)
            {
                Console.WriteLine($"Błąd: {errorResult.Message}");
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    result = errorResult.Message
                });
                return;
            }

            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                result = "Node added and blockchain requested"
            });
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = "Missing port or ip parameters"
            });
        }
    }

    public async Task GetFriendIp(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { success = true, result = sender.GetIps() });
    }

    public async Task ping(HttpContext context)
    {
        var res = await sender.pingNode();
        Console.WriteLine(res != null ? $"pojawił się błąd {res.Message}" : "success");

        var result = new
        {
            success = res == null,
            result = res != null ? $"pojawił się błąd {res.Message}" : "success"
        };

        await context.Response.WriteAsJsonAsync(result);
    }

    
    
    
    
    // zapytanie curl -X POST http://<ip>:<port>/api/addRecord -d '{Key: "data", Value: "data"}'
    // powinno zwrócić JSON {"result" : "success"}
    // TODO: do przetestowania automatycznego
    public record recivedRecordData(string to, string message, bool shouldBeEncrypted);
    
    public async Task AddRecord(HttpContext context)
    {
        context.Response.ContentType = "application/json";
     
        // jakoś z requesta pobieram klucz publiczny i prywatny
        var privateKey = "";
        var publicKey = "";
        
        IBlockchain<BlockType, messageRecord> block = Blockchain.GetInstance();
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        // zakładam że w body będzie JSON o typie Record
        var record = JsonConvert.DeserializeObject<recivedRecordData>(body);
        if (record == null)
        {
            await context.Response.WriteAsJsonAsync(new { success = false, result = "bad request body" });
            return;
        }

        Console.WriteLine($"wartość rekordu{record}");
        /*BlockType rec;
        if (record.shouldBeEncrypted) {
            rec = block.AddRecord(new messageRecord(publicKey, record.to, ));
        }
        else
        {
            rec = block.AddRecord(new messageRecord(publicKey, record.to, ));
        }
        */
        
        await sender.SendData(record);
        //if (rec != null) await sender.SendData(rec);

        await context.Response.WriteAsJsonAsync(new { success = true, result = "" });
    }


    public async Task GetBlockchain(HttpContext ctx)
    {
        var chain = Blockchain.GetInstance().GetParsedBlockchain();
        await ctx.Response.WriteAsJsonAsync(new { success = true, result = chain });
    }

    /* metoda powinna zwrócić coś w stylu {"blockCount":5,"recordCount":6,"workingTime":16,"friendNodeCount":0,"friendNode":[]}
    gdzie blockCount: int - ile jest bloków w blockchaine, recordCount: int - liczba dyskretnych ramek danych, workingTime: int64 - czas działania servera w sekundach, friendNodeCount: int - liczba znajomych węzłów, friendNode: []string - tablica stringów w postaci ip:port*/
    public async Task GetStat(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        // TODO: dodać ile my wyprodukowaliśmy węzłów oraz rekordów danych
        var chain = Blockchain.GetInstance().GetChain();

        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = new
            {
                blockCount = chain.Count,
                recordCount = chain.Aggregate(new List<messageRecord>(), (acc, x) =>
                {
                    acc.AddRange(x.Records);
                    return acc;
                }).Count, // taki brzydki fold i w sumie to pewnie mniej wydajny
                workingTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime,
                friendNodeCount = sender.GetIps().Count,
                friendNode = sender.GetIps()
            }
        });
    }
}