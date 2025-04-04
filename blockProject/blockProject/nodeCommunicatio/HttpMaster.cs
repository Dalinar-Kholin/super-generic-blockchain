using System.Net;
using blockProject.blockchain;
using Microsoft.AspNetCore.Http;

namespace blockProject.nodeCommunicatio;

public class HttpMaster(DataSender sender)
{
    public void AddNewNode(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("port", out var port) &&  context.Request.Query.TryGetValue("ip", out var ip))
        {
            string destPort = port.ToString();
            Console.WriteLine(ip.ToString());

            sender.AddIP(new IPEndPoint(IPAddress.Parse(ip.ToString()),int.Parse(destPort)));
                
        }
    }

    public void SendMessage(HttpContext context)
    {
        IBlockchain block = NonBlockChain.GetInstance();
        var res = sender.SendData(block);
        res.Wait();
        Console.WriteLine(res.Result != null ? $"pojawił się błąd {res.Result?.error}" : "sucess");
            
        var result = new { success = true, message = res.Result != null ? $"pojawił się błąd {res.Result?.error}" : "sucess" };

        // Ustawienie typu odpowiedzi
        context.Response.ContentType = "application/json";

        // Zapisanie (wysłanie) odpowiedzi jako JSON
        var task = context.Response.WriteAsJsonAsync(result);
        task.Wait();
    }
    
}