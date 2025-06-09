using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using blockProject.blockchain;
using blockProject.blockchain.genericBlockchain;
using blockProject.nodeCommunicatio.communicationFrames;
using blockProject.randomSrc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace blockProject.nodeCommunicatio;

public class IPMaster
{
    public readonly List<IPEndPoint> IPs = [];

    public readonly List<IPEndPoint> BlackListed = [];

    public List<string> GetIps()
    {
        return IPs.Select(ip => ip.ToString()).ToList();
    }

    public List<IPEndPoint> getParsed()
    {
        return IPs;
    }

    public Error? delIps(IPEndPoint ip)
    {
        return IPs.Remove(ip) ? null : new Error("no item in collection");
    }
    public void AddBlackList(IPEndPoint ip)
    {
        BlackListed.Add(ip);
    }
    public void delBlacklisted(IPEndPoint ip)
    {
        BlackListed.Remove(ip);
    }

    public void AddIP(IPEndPoint ip)
    {

        if (BlackListed.FindIndex(x => Equals(x, ip)) != -1) return; // jeżeli jest black listowany wywalamy
        if (IPs.Aggregate(false, (acc, x) => acc ? acc : x.ToString() == ip.ToString()))
            return; // check if we already have such IP on the list
        IPs.Add(ip);
    }
}


// class intendet to communication with other node
public class DataSender
{
    private readonly IBlockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();
    private IPMaster _ipMaster = new();

    public IPMaster getIpMaster()
    {
        return _ipMaster;
    }


    public async Task<Error?> SendData<T>(T dataToSend, IPEndPoint? exclude = null)
    {
        Console.WriteLine($"Attempting to send data");
        if (dataToSend == null) return new Error("empty data");
        var type = typeof(T) == typeof(recordType) ? Requests.ADD_RECORD : Requests.ADD_BLOCK;
        var frame = new Frame(type, JToken.FromObject(dataToSend));
        foreach (var ip in _ipMaster.IPs)
        {
            Console.WriteLine($"Sending data to {ip}");
            if (exclude != null && ip.Equals(exclude)) continue;

            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();
            try
            {
                var data = JsonConvert.SerializeObject(frame);
                var bytes = Encoding.UTF8.GetBytes(data);
                await stream.WriteAsync(bytes);

                var buffer = new byte[262_144];
                var readed = await stream.ReadAsync(buffer);
                var result = Encoding.UTF8.GetString(buffer, 0, readed);
                var json = JsonConvert.DeserializeObject<Frame>(result);

                if (json != null && json.Request == type) continue; // continuing the loop

                return new Error("Invalid response: " + result);
            }
            catch (Exception e)
            {
                return new Error($"Error sending record: {e}");
            }
        }
        return null;
    }

    // obtaining blockchain from other nodes
    public async Task<Error?> ReceiveBlockchain()
    {
        var requestFrame =
            JsonSerializer.Serialize(new Frame(Requests.GET_BLOCKCHAIN, JToken.FromObject((List<BlockType>)[])));

        Console.WriteLine($"Attempting to send data: {requestFrame} to {_ipMaster.IPs.Count} nodes");

        foreach (var ip in _ipMaster.IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();
            try
            {
                var bytes = Encoding.UTF8.GetBytes(requestFrame);
                await stream.WriteAsync(bytes, 0, bytes.Length);

                //TODO: change it so that there is no problem with sending large blockchains

                var buffer =
                    new byte[262_144];

                var readed = await stream.ReadAsync(buffer);
                var result = Encoding.UTF8.GetString(buffer, 0, readed);

                var json = JsonConvert.DeserializeObject<Frame>(result);
                if (json is { Request: Requests.GET_BLOCKCHAIN })
                {
                    var blockchin = json.data.ToObject<List<BlockType>>();

                    // TODO: comparison with our blockchain and loading into memory

                    var block = blockchin ?? new List<BlockType>();
                    Blockchain.GetInstance().SetChain(block);
                    _blockchainDataHandler.writeBlockchain(block);
                    return null;
                }

                return new Error("Invalid response: " + result);
            }
            catch (Exception e)
            {
                return new Error($"Failed to send data: {e}");
            }
        }

        return null;
    }


    public async Task<(Error?, IPEndPoint)> pingNode(IPEndPoint ip)
    {
        using TcpClient client = new();
        try
        {
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();

            var data = JsonConvert.SerializeObject(new Frame(Requests.CONNECTION_PING, JToken.FromObject("")));
            var bytes = Encoding.UTF8.GetBytes(data);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            var buffer = new byte[262_144];
            var readed = await stream.ReadAsync(buffer);
            var result = Encoding.UTF8.GetString(buffer, 0, readed);
            var jsonObject = JsonConvert.DeserializeObject<Frame>(result);
            if (jsonObject?.Request != Requests.CONNECTION_PING) return (new Error("bad server Response"), ip);
        }
        catch (Exception e)
        {
            return (new Error($"failed to send data: {e}"), ip);
        }

        return (null, ip);
    }

    public async Task<Error?> pingNodes()
    {
        foreach (var ip in _ipMaster.IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using var stream = client.GetStream();
            try
            {
                var data = JsonConvert.SerializeObject(new Frame(Requests.CONNECTION_PING, JToken.FromObject("")));
                var bytes = Encoding.UTF8.GetBytes(data);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                var buffer = new byte[262_144];
                var readed = await stream.ReadAsync(buffer);
                var result = Encoding.UTF8.GetString(buffer, 0, readed);
                var jsonObject = JsonConvert.DeserializeObject<Frame>(result);
                if (jsonObject?.Request != Requests.CONNECTION_PING) return new Error("bad server Response");
            }
            catch (Exception e)
            {
                return new Error($"failed to send data: {e}");
            }
        }

        return null;
    }
}

public record Frame(Requests Request, JToken data);