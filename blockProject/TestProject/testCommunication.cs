using System.Net;
using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace TestProject;

[CollectionDefinition("SequentialTests", DisableParallelization = true)]
public class SequentialTestsCollection
{
}

[Collection("SequentialTests")]
public class TestHelper
{
    public static WebApplication MakeApi()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.SuppressStatusMessages(true);
        builder.Logging.ClearProviders(); // uciszenie diagnostyki servera http
        var app = builder.Build();
        var api = app.MapGroup("/api");

        var sender = new DataSender();
        var httpMaster = new HttpMaster(sender);

        api.MapGet("/addNewNode", httpMaster.AddNewNode);

        api.MapGet("/sendMessage", httpMaster.ping);
        api.MapGet("/ping", (HttpContext context) => "alive");
        api.MapGet("/getFriendIps", httpMaster.GetFriendIp);
        api.MapGet("/getStats", httpMaster.GetStat);
        return app;
    }
}

public class testCommunication
{
    private readonly ITestOutputHelper _testOutputHelper;

    public testCommunication(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }


    [Fact]
    public async Task testBasicCommunicationWithData()
    {
        var dh = singleFileBlockchainDataHandler.GetTestInstance();
        dh._filePath = "../../../data.json";
        var (storedChain, error) = dh.readBlockchain();
        if (error != null)
        {
            // jeżeli nie udało się załadować blockchainu spadamy z rowerka
            Console.WriteLine($"nie udało się załadować blockchainu z powodu {error.Message}");
            Environment.Exit(1);
        }

        Blockchain.GetInstance().SetChain(storedChain);

        const int node1Port = 9999;
        const int node2Port = 8888;
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
                    var response1Task = client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/ping");
                    var response2Task = client.GetAsync($"http://127.0.0.1:{node2Port + 1}/api/ping");
                    var responses = await Task.WhenAll(response1Task, response2Task);
                    if (responses[0].StatusCode == HttpStatusCode.OK &&
                        responses[1].StatusCode == HttpStatusCode.OK) break;
                } // czeakanie aż servery http się włączą
                catch (Exception)
                {
                }
        }

        using (var client = new HttpClient())
        {
            try
            {
                var response =
                    await client.GetAsync(
                        $"http://127.0.0.1:{node1Port + 1}/api/addNewNode?port={node2Port}&ip={node2Ip}");
                Assert.Equal(200, (int)response.StatusCode);
                Assert.True(Blockchain.GetInstance().GetBlockchain().Count >
                            0); // sprawdzamy czy realnie dostaliśmy jakiś blockchain
                response = await client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/getFriendIps");
                var body = await response.Content.ReadAsStringAsync();

                var deserialized = JsonConvert.DeserializeAnonymousType(body, new
                {
                    success = true,
                    result = new List<string>()
                });

                Assert.NotNull(deserialized);
                Assert.True(deserialized.success);
                // Konwertujemy stringi na IPEndPoint
                var result = deserialized.result.Select(ipString =>
                {
                    var parts = ipString.Split(':');
                    if (parts.Length != 2) throw new FormatException($"Niepoprawny format adresu: {ipString}");

                    var ipAddress = IPAddress.Parse(parts[0]);
                    var port = int.Parse(parts[1]);
                    return new IPEndPoint(ipAddress, port);
                }).ToList();
                Assert.Equal(200, (int)response.StatusCode);
                Assert.Equal(new IPEndPoint(IPAddress.Parse(node2Ip), node2Port), result[0]);
                response = await client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/sendMessage");
                body = await response.Content.ReadAsStringAsync();
                var newTemplate = new { success = true, result = "sucess" };
                var newResult = JsonConvert.DeserializeAnonymousType(body, newTemplate);
                Assert.Equal("success", newResult?.result);
                Assert.Equal(200, (int)response.StatusCode);
            }
            catch (Exception e)
            {
                Assert.Fail($"cant handle communication {e}\n");
            }
        }

        Blockchain.Reset();
    }
}