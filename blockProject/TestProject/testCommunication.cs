using System.Net;
using blockProject.nodeCommunicatio;
using Microsoft.AspNetCore.Builder;
using Xunit.Abstractions;

namespace TestProject;

public class testCommunication
{
    private readonly ITestOutputHelper _testOutputHelper;

    public testCommunication(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public WebApplication MakeApi()
    {
        var builder = WebApplication.CreateBuilder();

        var app = builder.Build();
        var api = app.MapGroup("/api");

        var httpMaster = new HttpMaster(new DataSender());

        api.MapGet("/addNewNode", httpMaster.AddNewNode);

        api.MapGet("/sendMessage", httpMaster.SendMessage);
        return app;
    }

    [Fact]
    public async Task nice()
    {
        const int node1Port = 9999;
        const int node2Port = 8888;
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

        await Task.Delay(2000);

        using (var client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/addNewNode?port={node2Port}&ip=127.0.0.1");
                Console.WriteLine((int)response.StatusCode);

                response = await client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/sendMessage");
                Console.WriteLine((int)response.StatusCode);

            }
            catch (Exception e)
            {
                Assert.Fail($"cant handle communication {e}\n");
            }
        }




    }
}