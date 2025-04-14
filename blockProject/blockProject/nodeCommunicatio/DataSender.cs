using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.nodeCommunicatio.communicationFrames;
using blockProject.randomSrc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace blockProject.nodeCommunicatio;

// klasa do wysyłania danych do innych węzłów
public class DataSender
{
    private readonly List<IPEndPoint> IPs = [];
    private readonly IblockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();

    public List<string> GetIps()
    {
        return IPs.Select(ip => ip.ToString()).ToList();
    }

    public void AddIP(IPEndPoint ip)
    {
        IPs.Add(ip);
    }

    // do lekkiej zmiany
    // wyslanie bloku do sasiadów
    public async Task<Error?> SendBlock(BlockType block)
    {
        foreach (var ip in IPs) // przy metodzie plotki przesyłamy do losowych sąsiadów w liczbie x
        {
            var requestFrame =
                new Frame(Requests.ADD_BLOCK, JToken.FromObject(block));

            Console.WriteLine($"Próba wysłania danych: {requestFrame} do {ip.Address}");
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();
            try
            {
                string data = JsonConvert.SerializeObject(requestFrame);
                var bytes = Encoding.UTF8.GetBytes(data);
                await stream.WriteAsync(bytes);

                var buffer = new byte[1_024];
                var readed = await stream.ReadAsync(buffer);
                var result = Encoding.UTF8.GetString(buffer, 0, readed);
                var json = JsonConvert.DeserializeObject<Frame>(result);

                if (json is { Request: Requests.ADD_BLOCK })
                {
                    return null;
                }

                return new Error("Invalid response: " + result);
            }
            catch (Exception e)
            {
                return new Error($"Nie udało się wysłać danych {e}");
            }
        }
        return null;
    }

    // pozyskanie blockchaina od innych wezlow
    public async Task<Error?> ReceiveBlockchain()
    {
        var requestFrame =
            JsonSerializer.Serialize(new Frame(Requests.GET_BLOCKCHAIN, JToken.FromObject((List<BlockType>)[])));

        Console.WriteLine($"Próba wysłania danych: {requestFrame} do {IPs.Count} węzłów");

        foreach (var ip in IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();
            try
            {
                var bytes = Encoding.UTF8.GetBytes(requestFrame);
                await stream.WriteAsync(bytes, 0, bytes.Length);

                var buffer =
                    new byte[16_384]; //TODO: zmienić to tak by nie było problemu z przesyłaniem dużych łańcuchów bloków
                var readed = await stream.ReadAsync(buffer);
                var result = Encoding.UTF8.GetString(buffer, 0, readed);
                var json = JsonConvert.DeserializeObject<Frame>(result);

                if (json is { Request: Requests.GET_BLOCKCHAIN })
                {
                    var blockchin = json.data.ToObject<List<BlockType>>();
                    // TODO: porównanie ze swoim blockchainem i załadowanie do pamięci
                    var block = (blockchin ?? new List<BlockType>());
                    Blockchain.GetInstance().chain = block;
                    _blockchainDataHandler.writeBlockchain(block);
                    // Zapisz blockchain
                    return null;
                }

                return new Error("Invalid response: " + result);
            }
            catch (Exception e)
            {
                return new Error($"Nie udało się wysłać danych {e}");
            }
        }

        return null;
    }

    public async Task<Error?> SendData<T>(IBlockchain<T> blockchain)
    {
        foreach (var ip in IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();
            try
            {
                var data = JsonConvert.SerializeObject(new Frame(Requests.CONNECTION_PING, JToken.FromObject("")));
                var bytes = Encoding.UTF8.GetBytes(data);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                var buffer = new byte[1_024];
                var readed = await stream.ReadAsync(buffer);
                var result = Encoding.UTF8.GetString(buffer, 0, readed);
                var jsonObject = JsonConvert.DeserializeObject<Frame>(result);
                if (jsonObject?.Request != Requests.CONNECTION_PING) return new Error("bad server Response");
            }
            catch (Exception e)
            {
                return new Error($"nie udało się wysłać danych {e}");
            }
        }

        return null;
    }
}

public record Frame(Requests Request, JToken data);