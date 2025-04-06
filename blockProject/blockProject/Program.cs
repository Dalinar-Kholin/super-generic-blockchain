using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace blockProject;


internal class Program
{
    
    private static void Main(string[] args)
    {
        var port = int.Parse(args[0]);
        new Thread(new Listener(new SimpleTextCm(),port ).Start).Start(); // włączamy wątek odpowiedzialny za nasłuchiwanie

        var builder = WebApplication.CreateBuilder();

        var app = builder.Build();
        var api = app.MapGroup("/api");

        var httpMaster = new HttpMaster(new DataSender());
        
        api.MapGet("/addNewNode", httpMaster.AddNewNode);
        
        //sprawdzenie czy komunikacja działa
        api.MapGet("/sendMessage", httpMaster.SendMessage);
        
        api.MapGet("/getNode", (HttpContext httpContext) => "essa");
        
        Console.WriteLine("starting server\n");
        app.Run($"http://127.0.0.1:{port+1}/");
    }
}