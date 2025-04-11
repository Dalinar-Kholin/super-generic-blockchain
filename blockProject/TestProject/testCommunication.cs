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

public class testCommunication
{
    private readonly ITestOutputHelper _testOutputHelper;

    public testCommunication(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private WebApplication MakeApi()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.SuppressStatusMessages(true); 
        builder.Logging.ClearProviders();// uciszenie diagnostyki servera http
        var app = builder.Build();
        var api = app.MapGroup("/api");

        var httpMaster = new HttpMaster(new DataSender());

        api.MapGet("/addNewNode", httpMaster.AddNewNode);

        api.MapGet("/sendMessage", httpMaster.SendMessage);
        api.MapGet("/ping", (HttpContext context) => "alive");
        api.MapGet("/getFriendIps", httpMaster.GetFriendIp);
        return app;
    }

    [Fact]
    public async Task testBasicCommunicationWithData()
    {
        const int node1Port = 9999;
        const int node2Port = 8888;
        const string node2Ip = "127.0.0.1";
        new Thread(new Listener(new SimpleTextCm(), node1Port).Start).Start();
        new Thread(new Listener(new SimpleTextCm(), node2Port).Start).Start();
        var node1 = MakeApi();
        var node2 = MakeApi();

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
                Assert.True(Blockchain.GetInstance().GetBlockchain().Count > 0); // sprawdzamy czy realnie dostaliśmy jakiś blockchain
                response = await client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/getFriendIps");
                var body = await response.Content.ReadAsStringAsync();

                var deserialized = JsonConvert.DeserializeAnonymousType(body, new
                {
                    result = new List<string>()
                });

                if (deserialized == null) throw new Exception("Błąd deserializacji JSON");

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
                var newTemplate = new { success = true, message = "sucess" };
                var newResult = JsonConvert.DeserializeAnonymousType(body, newTemplate);
                Assert.Equal("success", newResult?.message);
                Assert.Equal(200, (int)response.StatusCode);
            }
            catch (Exception e)
            {
                Assert.Fail($"cant handle communication {e}\n");
            }
        }
    }
}