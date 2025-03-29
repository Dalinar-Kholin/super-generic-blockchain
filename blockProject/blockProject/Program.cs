using System.Net;
using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace blockProject;




internal class Program
{
    
    private static void Main(string[] args)
    {
        var port = int.Parse(args[0]);
        new Thread(new Listener(new SimpleTextCm(),port ).Start).Start(); // włączamy wątek odpowiedzialny za nasłuchiwanie

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddOpenApi();
        var sender = new DataSender();
        var app = builder.Build();
        var api = app.MapGroup("/api");
        
        api.MapGet("/addNewNode", (HttpContext context) => // zmić na POST i dodać czytanie z BODY
        {
            if (context.Request.Query.TryGetValue("port", out var port) &&  context.Request.Query.TryGetValue("ip", out var ip))
            {
                string destPort = port.ToString();
                Console.WriteLine(ip.ToString());

                sender.AddIP(new IPEndPoint(IPAddress.Parse(ip.ToString()),int.Parse(destPort)));     
            }
        });

        
        //sprawdzenie czy komunikacja działa
        api.MapGet("/sendMessage", (HttpContext context) =>
        {
            IBlockchain block = NonBlockChain.GetInstance();
            var res = sender.SendData(block);
            res.Wait();
            Console.WriteLine(res.Result == null ? $"pojawił się błąd{res.Result?.error}" : "sucess");
        });
        
        api.MapGet("/getNode", (HttpContext httpContext) => { return "essa"; });
       
        Console.WriteLine("starting server\n");
        app.Run($"http://127.0.0.1:{port+1}/");
    }
}