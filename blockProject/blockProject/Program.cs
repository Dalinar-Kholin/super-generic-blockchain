using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace blockProject;

internal class Program
{
    private static void Main(string[] args)
    {
        var port = int.Parse(args[0]);

        var sender = new DataSender();
        var httpMaster = new HttpMaster(sender);

        new Thread(new Listener(port).Start)
            .Start(); // włączamy wątek odpowiedzialny za nasłuchiwanie

        var builder = WebApplication.CreateBuilder();

        var app = builder.Build();
        var api = app.MapGroup("/api");

        api.MapGet("/addNewNode", httpMaster.AddNewNode);

        api.MapGet("/SendBlock", httpMaster.SendBlock);

        api.MapGet("/getFriendIp", httpMaster.GetFriendIp);

        //sprawdzenie czy komunikacja działa
        api.MapGet("/sendMessage", httpMaster.SendMessage);

        api.MapGet("/getNode", (HttpContext httpContext) => "essa");



        Console.WriteLine("starting server\n");
        app.Run($"http://127.0.0.1:{port + 1}/");
    }
}