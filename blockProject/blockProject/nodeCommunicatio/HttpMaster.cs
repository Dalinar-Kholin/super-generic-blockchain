using System.Net;
using blockProject.blockchain;
using Microsoft.AspNetCore.Http;

namespace blockProject.nodeCommunicatio;

public class HttpMaster
{
    private readonly DataSender sender;

    public HttpMaster(DataSender sender)
    {
        this.sender = sender;
    }

    // wysłanie bloku do sąsiadów
    public async Task SendBlock(HttpContext context)
    {
        //testowe dane
        Record record = new Record("test", "test");
        BlockType block = new BlockType("0x0");
        block.AddRecord(record);

        Console.WriteLine($"Wysłano blok: {block}");
        var errorResult = await sender.SendBlock(block);

        if (errorResult != null)
        {
            Console.WriteLine($"Błąd: {errorResult.Message}");
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = errorResult.Message
            });
            return;
        }
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            message = "Block sent successfully"
        });
    }

    public async Task AddNewNode(HttpContext context)
    {
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
                    error = errorResult.Message
                });
                return;
            }

            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = "Node added and blockchain requested"
            });
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = "Missing port or ip parameters"
            });
        }
    }

    public async Task GetFriendIp(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { result = sender.GetIps() });
    }

    public async Task SendMessage(HttpContext context)
    {
        IBlockchain<BlockType> block = Blockchain.GetInstance();
        var res = await sender.SendData(block);
        Console.WriteLine(res != null ? $"pojawił się błąd {res.Message}" : "success");

        var result = new
        {
            success = res == null,
            message = res != null ? $"pojawił się błąd {res.Message}" : "success"
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(result);
    }
}