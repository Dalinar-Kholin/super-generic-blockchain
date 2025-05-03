using System.Buffers.Text;
using System.Net;
using System.Text;
using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace blockProject.httpServer;



public class LoginMaster
{
    private record registerData(string Username, string password, string privateKey, string publicKey);
    private record loginData(string Username, string password);
    
    public async Task addUser(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        // zakładam że w body będzie JSON o typie registerData
        var data = JsonConvert.DeserializeObject<registerData>(body);
        if (data == null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = "bad request"
            });
            return;
        }
        var keyMaster = new JsonKeyMaster();
        
        var err = keyMaster.deepStore(new Keys(Convert.FromBase64String(data.privateKey), Convert.FromBase64String(data.publicKey)), data.Username, data.password);
        if (err != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = err.Message // mając możliwą rejestrace nie opędzimy się od posiadania wyroczni mówiącej o tym czy dany user istnieje
            });
            return;
        }

        var res = keyMaster.loadKeys(data.Username, data.password);
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = data.Username
        });
        context.Response.Cookies.Append("uuid", res.uuid);
        
    }
    public async Task login(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        // zakładam że w body będzie JSON o typie registerData
        var data = JsonConvert.DeserializeObject<loginData>(body);
        if (data == null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = "bad request"
            });
            return;
        }
        var keyMaster = new JsonKeyMaster();
        
        var res = keyMaster.loadKeys(data.Username, data.password);
        if (res.Item2 != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = "username or password incorrect"
            });
            return;
        }
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = data.Username
        });
        context.Response.Cookies.Append("uuid", res.uuid);
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


    public async Task GetMessages(HttpContext context)
    {
        var cookie = context.Request.Cookies["uuid"];
        if (cookie == null)
        {
            context.Response.StatusCode = 403;
            return;
        };
        var res = new JsonKeyMaster().getKeys(cookie);
        var messages = Blockchain.GetInstance().GetChain().Aggregate(
            new List<simpleMessage>(), (accumulate, block) =>
            {
                block.Records.Aggregate(new List<simpleMessage>(), (_, r) =>
                {
                    
                    if (r.to == Convert.ToBase64String(res.keys.PublicKey)) accumulate.Add(new simpleMessage(r.from, r.to, Encoding.ASCII.GetString(r.message)));
                    return null!;
                });
                return accumulate;
            }
        );
        
        
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = messages
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

        var cookie = context.Request.Cookies["uuid"];
        if (cookie == null)
        {
            context.Response.StatusCode = 403;
            return;
        };
        var res = new JsonKeyMaster().getKeys(cookie);
        // i tak oto mamy klucze kwestia jeszcze szyfrowania wiadomości, ale to na następny tydzień

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