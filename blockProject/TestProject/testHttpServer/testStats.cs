using System.Net;
using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using Newtonsoft.Json;

namespace TestProject.testHttpServer;

[Collection("SequentialTests")]
public class testStats
{
    [Trait("cat", "http")]
    [Fact]
    public async Task basicTestStats()
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


        const int node1Port = 9988;
        const int node2Port = 8899;
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
                response = await client.GetAsync(
                    $"http://127.0.0.1:{node1Port + 1}/api/addNewNode?port={node2Port}&ip={node2Ip}");
                Assert.Equal(200, (int)response.StatusCode);
                response = await client.GetAsync(
                    $"http://127.0.0.1:{node1Port + 1}/api/addNewNode?port={node2Port}&ip={node2Ip}");
                Assert.Equal(200, (int)response.StatusCode); // sprawdzamy od razu czy nie powtarzamy węzłów

                response = await client.GetAsync($"http://127.0.0.1:{node1Port + 1}/api/getStats");
                var body = await response.Content.ReadAsStringAsync();

                var deserialized = JsonConvert.DeserializeAnonymousType(body, new
                {
                    success = true,
                    result = new
                    {
                        blockCount = 0,
                        recordCount = 0,
                        workingTime = 0,
                        friendNodeCount = 0,
                        friendNode = new string[] { }
                    }
                });


                // sprawdzenie jak wyglądają prawdziwe dane 
                var json = File.ReadAllText(dh._filePath);
                var chain = JsonConvert.DeserializeObject<List<Block>>(json) ?? new List<Block>();
                
                Assert.NotNull(deserialized);
                Assert.Equal(chain.Count, deserialized.result.blockCount);
                Assert.Equal(chain.Aggregate(0 , (i, block) => i + block.recordsInBlock ), deserialized.result.recordCount);
                Assert.True(deserialized.result.friendNode.Length == 1);
                Assert.Equal(new IPEndPoint(IPAddress.Parse(node2Ip), node2Port).ToString(),
                    deserialized.result.friendNode[0]);
            }
            catch (Exception e)
            {
                Assert.Fail($"cant handle communication {e}\n");
            }
        }

        Blockchain.Reset();
    }
}