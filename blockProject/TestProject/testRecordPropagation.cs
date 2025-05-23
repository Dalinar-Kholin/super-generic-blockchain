using System.Net;
using blockProject.blockchain;
using blockProject.httpServer;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using blockProject.blockchain.genericBlockchain;

namespace TestProject.testAddingRecords;

public class TestHelper
{
    public static WebApplication MakeApi()
    {
        var sender = new DataSender();
        var httpMaster = new HttpMaster(sender);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.SuppressStatusMessages(true);
        builder.Logging.ClearProviders(); // uciszenie diagnostyki servera http

        var app = builder.Build();

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
            }
            ;
            var res = new JsonKeyMaster().getKeys(cookie);
            if (res.err != null)
            {
                Console.WriteLine($"error := {res.err.Message}");



                context.HttpContext.Response.StatusCode = 403;
                return null;
            }
            return await next(context);
        });

        //api.MapGet("/SendBlock", httpMaster.SendBlock); // test method
        //api.MapGet("/sendMessage", httpMaster.ping); // communication test
        //api.MapGet("/getNode", (HttpContext httpContext) => "essa"); // test

        supervisor.MapGet("/addNewNode", httpMaster.AddNewNode);
        supervisor.MapGet("/getStats", httpMaster.GetStat);
        supervisor.MapGet("/ping", (HttpContext context) => "alive");
        supervisor.MapGet("/getFriendIp", httpMaster.GetFriendIp); // test method

        api.MapPost("/addRecord", httpMaster.AddRecord);
        api.MapGet("/getMessages", httpMaster.GetMessages);

        var loginMaster = new LoginMaster();

        auth.MapPost("/login", loginMaster.login);
        auth.MapPost("/register", loginMaster.addUser);

        var anonServer = new anonServer();
        anon.MapGet("/getMessages", anonServer.getMessages);
        return app;
    }
}

[Collection("SequentialTests")]
public class testRecordPropagation
{
    private readonly ITestOutputHelper _testOutputHelper;

    public testRecordPropagation(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task testAddingRecords()
    {
        var dh = singleFileBlockchainDataHandler.GetTestInstance();
        dh._filePath = "../../../data.json";
        var (storedChain, error) = dh.readBlockchain();
        if (error != null)
        {
            Console.WriteLine($"failed to load blockchain due to: {error.Message}");
            Environment.Exit(1);
        }

        Blockchain.GetInstance().SetChain(storedChain);

        const int node1Port = 9990;
        const int node2Port = 8880;
        const string node2Ip = "127.0.0.1";

        var sender = new DataSender();
        new Thread(new Listener(node1Port).Start).Start();
        new Thread(new Listener(node2Port).Start).Start();
        var node1 = TestHelper.MakeApi();
        var node2 = TestHelper.MakeApi();

        new Thread(
            () => { node1.Run($"http://127.0.0.1:{node1Port + 1}/"); }
        ).Start();
        new Thread(
            () => { node2.Run($"http://127.0.0.1:{node2Port + 1}/"); }
        ).Start();
        using (var client = new HttpClient())
        {
            while (true)
                try
                {
                    var response1Task = client.GetAsync($"http://127.0.0.1:{node1Port + 1}/supervisor/ping");
                    var response2Task = client.GetAsync($"http://127.0.0.1:{node2Port + 1}/supervisor/ping");
                    var responses = await Task.WhenAll(response1Task, response2Task);
                    if (responses[0].StatusCode == HttpStatusCode.OK &&
                        responses[1].StatusCode == HttpStatusCode.OK) break;
                } // waiting for http servers to start
                catch (Exception)
                {
                }
        }

        using (var client = new HttpClient())
        {
            var loginData = new
            {
                username = "ogg",
                password = "nice"
            };

            // Serializing data to JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response1 = await client.PostAsync($"http://127.0.0.1:{node1Port + 1}/auth/login", content);

            var body = await response1.Content.ReadAsStringAsync();
            var newTemplate = new { success = true, result = "sucess" };
            var newResult = JsonConvert.DeserializeAnonymousType(body, newTemplate);
            Assert.True(newResult.success);
            Assert.Equal("ogg", newResult.result); // we check if we received the correct data

            var requestData = new
            {
                to = "0x0",
                message = "xd4",
                shouldBeEncrypted = false
            };

            json = JsonConvert.SerializeObject(requestData);
            content = new StringContent(json, Encoding.UTF8, "application/json");

            int numberOfBlocks = Blockchain.GetInstance().GetChain().Count();
            Console.WriteLine($"number of blocks before adding {numberOfBlocks}");

            HttpResponseMessage response2 = await client.PostAsync($"http://127.0.0.1:{node1Port + 1}/api/addRecord", content);
            var body2 = await response2.Content.ReadAsStringAsync();
            newResult = JsonConvert.DeserializeAnonymousType(body2, newTemplate);
            Assert.Equal(numberOfBlocks, Blockchain.GetInstance().GetChain().Count());

            HttpResponseMessage response3 = await client.PostAsync($"http://127.0.0.1:{node1Port + 1}/api/addRecord", content);
            var body3 = await response2.Content.ReadAsStringAsync();
            newResult = JsonConvert.DeserializeAnonymousType(body3, newTemplate);
            Assert.Equal(numberOfBlocks, Blockchain.GetInstance().GetChain().Count());

            HttpResponseMessage response4 = await client.PostAsync($"http://127.0.0.1:{node1Port + 1}/api/addRecord", content);
            var body4 = await response2.Content.ReadAsStringAsync();
            newResult = JsonConvert.DeserializeAnonymousType(body4, newTemplate);
            Assert.NotEqual(numberOfBlocks, Blockchain.GetInstance().GetChain().Count());

            HttpResponseMessage response5 = await client.PostAsync($"http://127.0.0.1:{node1Port + 1}/api/addRecord", content);
            var body5 = await response2.Content.ReadAsStringAsync();
            newResult = JsonConvert.DeserializeAnonymousType(body5, newTemplate);
            Assert.Equal(numberOfBlocks + 1, Blockchain.GetInstance().GetChain().Count());
        }

        Blockchain.Reset();
    }
}