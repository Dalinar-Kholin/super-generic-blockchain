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

        // blockchain initialization 
        var dh = singleFileBlockchainDataHandler.GetInstance();
        JsonKeyMaster.path = ".KeyFile";

        // loading blockhain data
        var (storedChain, error) = dh.readBlockchain();
        if (error == null)
        {
            Blockchain.GetInstance().SetChain(storedChain);
        }

        var sender = new DataSender();
        var httpMaster = new HttpMaster(sender);
        new Thread(new Listener(port).Start).Start(); // start listener for node communication

        var builder = WebApplication.CreateBuilder();

        // CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins("http://localhost:3000") 
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); 
            });
        });

        var app = builder.Build();

        // CORS
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

        // auth filter — wymaga ciasteczka uuid i poprawnego klucza
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

        api.WithGroupName("/api")
            .AddEndpointFilter(async (context, next) =>
            {
                context.HttpContext.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; script-src 'self'; object-src 'none'; base-uri 'self';";
                context.HttpContext.Response.Headers["X-Frame-Options"] = "DENY";
                context.HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.HttpContext.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.HttpContext.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

                return await next(context);
            });

        supervisor.MapGet("/addNewNode", httpMaster.AddNewNode);
        supervisor.MapGet("/getFriendIp", httpMaster.GetFriendIp);
        supervisor.MapGet("/getStats", httpMaster.GetStat);

        api.MapPost("/addRecord", httpMaster.AddRecord);
        api.MapGet("/getMessages", httpMaster.GetMessages);

        var loginMaster = new LoginMaster();
        auth.MapPost("/login", loginMaster.login);
        auth.MapPost("/register", loginMaster.addUser);

        var anonServer = new anonServer();
        anon.MapGet("/getMessages", anonServer.getMessages);

        Console.WriteLine("starting server\n");

        app.Run($"http://127.0.0.1:{port + 1}/");
    }
}
