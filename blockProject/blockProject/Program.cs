using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;


namespace blockProject;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("XD");
        var port = int.Parse(args[0]);

        // inicjalizacja blockchainu
        var dh = singleFileBlockchainDataHandler.GetInstance();
        dh._filePath = "data.json";
        var (storedChain, error) = dh.readBlockchain();
        if (error != null)
        {
            // jeżeli nie udało się załadować blockchainu spadamy z rowerka
            Console.WriteLine($"nie udało się załadować blockchainu z powodu {error.Message}");
            Environment.Exit(1);
        }

        Blockchain.GetInstance().SetChain(storedChain);

        var sender = new DataSender();
        var httpMaster = new HttpMaster(sender);

        new Thread(new Listener(port).Start).Start(); // włączamy wątek odpowiedzialny za nasłuchiwanie

        var builder = WebApplication.CreateBuilder();

        // DODANE: Rejestrujemy politykę CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // DODANE: Aktywujemy middleware CORS — MUSI być przed MapGroup i Run
        app.UseCors("AllowFrontend");

        // teraz grupujemy API
        var api = app.MapGroup("/api");

        api.MapGet("/addNewNode", httpMaster.AddNewNode);
        api.MapGet("/SendBlock", httpMaster.SendBlock); // to jest testowe
        api.MapGet("/getFriendIp", httpMaster.GetFriendIp); // to jest testowe
        api.MapGet("/sendMessage", httpMaster.SendMessage); // test komunikacji
        api.MapGet("/getNode", (HttpContext httpContext) => "essa"); // test
        api.MapGet("/getStats", httpMaster.GetStat);
        api.MapPost("/addRecord", httpMaster.AddRecord);

        Console.WriteLine("starting server\n");

        app.Run($"http://127.0.0.1:{port + 1}/");
    }
}

