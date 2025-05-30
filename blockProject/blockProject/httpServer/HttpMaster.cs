using System.Buffers.Text;
using System.Net;
using System.Net.Mime;
using System.Text;
using blockProject.blockchain;
using blockProject.blockchain.genericBlockchain;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace blockProject.httpServer;


public class LoginMaster
{
    private record registerData(string username, string password, string privateKey, string publicKey);
    private record loginData(string username, string password);

    public async Task addUser(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        // we assume that the body will be JSON of type registerData
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
        if (data.privateKey.Length == 0)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                result = "where key stupido :/"
            });
            return;
        }
        var err = keyMaster.deepStore(new Keys(Convert.FromBase64String(data.privateKey), Convert.FromBase64String(data.publicKey)), data.username, data.password);
        if (err != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = err.Message // mając możliwą rejestrace nie opędzimy się od posiadania wyroczni mówiącej o tym czy dany user istnieje
            });
            return;
        }

        var res = keyMaster.loadKeys(data.username, data.password);
        context.Response.Cookies.Append("uuid", res.uuid);
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = data.username
        });


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

        var res = keyMaster.loadKeys(data.username, data.password);
        if (res.Item2 != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = "username or password incorrect"
            });
            return;
        }
        context.Response.Cookies.Append("uuid", res.uuid);
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = data.username
        });
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

    // sending block to other nodes
    public async Task SendBlock(HttpContext context)
    {
        // tutaj jest chyba więcej logiki do dodania

        var errorResult = await sender.SendData(Blockchain.GetInstance().newestBlock);

        if (errorResult != null)
        {
            Console.WriteLine($"Error: {errorResult.Message}");
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
        }
        ;
        var res = new JsonKeyMaster().getKeys(cookie);
        if (res.err != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                result = res.err
            });
            return;
        }

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

            return -1; // not found
        };

        var messages = Blockchain.GetInstance().GetChain().Aggregate(
            new List<simpleMessage>(), (accumulate, block) =>
            {

                var data = block.body.Records;
                List<messageRecord> records = new List<messageRecord>();
                for (int i = 0; i < 3; i++)
                {
                    var index = FindNthIndex(data, 8, 0x0);
                    if (index == -1) break;
                    byte[] part = data.Take(index+1).ToArray(); // extract segment as byte[]
                    Console.WriteLine($"essa {Encoding.UTF8.GetString(part)}");
                    records.Add(new messageRecord(part));

                    // Update data, skipping fragment and separator
                    data = data.Skip(index + 1).ToArray();
                }

                records.Aggregate(new List<simpleMessage>(), (_, r) =>
                {
                    if (r.to == Convert.ToBase64String(res.keys.PublicKey))
                    {
                        // todo: decrypting the message


                        accumulate.Add(r.decrypt(res.keys));
                    }
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

            sender.getIpMaster().AddIP(new IPEndPoint(IPAddress.Parse(ip.ToString()), int.Parse(destPort)));
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
        await context.Response.WriteAsJsonAsync(new { success = true, result = sender.getIpMaster().GetIps() });
    }

    public async Task ping(HttpContext context)
    {
        var res = await sender.pingNodes();
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
        }
        ;
        var res = new JsonKeyMaster().getKeys(cookie);

        // i tak oto mamy klucze kwestia jeszcze szyfrowania wiadomości, ale to na następny tydzień

        var block = Blockchain.GetInstance();
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        // we assume that the body will be JSON of Record type
        var record = JsonConvert.DeserializeObject<recivedRecordData>(body);
        if (record == null)
        {
            await context.Response.WriteAsJsonAsync(new { success = false, result = "bad request body" });
            return;
        }


        var rec = block.AddRecord(new recordType(record.to, Encoding.ASCII.GetBytes(record.message), res.keys, record.shouldBeEncrypted).toByte());


        Console.WriteLine($"wartość rekordu: {record}");

        if (rec != null)
        {
            singleFileBlockchainDataHandler.GetInstance().writeBlockchain(Blockchain.GetInstance().GetChain());
            await sender.SendData(rec);
        }
        else
        {
            await sender.SendData(record);
        }

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
        // TODO: add how many nodes and data records we produced
        var chain = Blockchain.GetInstance().GetChain();

        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = new
            {
                blockCount = chain.Count,
                recordCount = chain.Aggregate(0, (acc, x) =>
                {
                    acc += x.header.recordsInBlock;
                    return acc;
                }), // taki brzydki fold i w sumie to pewnie mniej wydajny
                workingTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime,
                friendNodeCount = sender.getIpMaster().GetIps().Count,
                friendNode = sender.getIpMaster().GetIps()
            }
        });
    }
}
