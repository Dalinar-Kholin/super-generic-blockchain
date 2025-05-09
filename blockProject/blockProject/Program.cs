using System.Security.Cryptography;
using System.Text;
using blockProject.blockchain;
using blockProject.httpServer;
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

        // inicjalizacja blockchainu
        var dh = singleFileBlockchainDataHandler.GetInstance();
        JsonKeyMaster.path = ".KeyFile";
        //dh._filePath = "D:\\nokia\\blockProject\\blockProject\\data.json";
        //if (!File.Exists(dh._filePath))
        //{
        //    Console.WriteLine($"fooo");
        //}
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
        app.UseHttpsRedirection();
        
        app.UseDefaultFiles(); // Szuka index.html automatycznie
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        
        // teraz grupujemy API
        
        var api = app.MapGroup("/api");
        var anon = app.MapGroup("/anon");
        var auth = app.MapGroup("/auth");
        var supervisor = app.MapGroup("/supervisor");
        
        api.AddEndpointFilter(async (context, next) =>
        {
            var cookie = context.HttpContext.Request.Cookies["uuid"];
            if (cookie == null)
            {
                context.HttpContext.Response.StatusCode = 403;
                return null;
            };
            var res = new JsonKeyMaster().getKeys(cookie);
            if (res.err != null)
            {
                Console.WriteLine($"error := {res.err.Message}");
                
                
                
                context.HttpContext.Response.StatusCode = 403;
                return null;
            }
            return await next(context);
        });
        
        supervisor.MapGet("/addNewNode", httpMaster.AddNewNode);
        supervisor.MapGet("/getFriendIp", httpMaster.GetFriendIp); // to jest testowe
        supervisor.MapGet("/getStats", httpMaster.GetStat);

        //api.MapGet("/SendBlock", httpMaster.SendBlock); // to jest testowe
        // api.MapGet("/sendMessage", httpMaster.ping); // test komunikacji
        api.MapPost("/addRecord", httpMaster.AddRecord);
        api.MapGet("/getMessages", httpMaster.GetMessages);
        //api.MapGet("/getNode", (HttpContext httpContext) => "essa"); // test
        var loginMaster = new LoginMaster();

        auth.MapPost("/login", loginMaster.login);
        auth.MapPost("/register", loginMaster.addUser);

        var anonServer = new anonServer();
        anon.MapGet("/getMessages", anonServer.getMessages);
        
        Console.WriteLine("starting server\n");

        app.Run($"http://127.0.0.1:{port + 1}/");
    }
}