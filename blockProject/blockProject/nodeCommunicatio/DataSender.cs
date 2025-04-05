using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.randomSrc;

namespace blockProject.nodeCommunicatio;


// klasa do wysyłania danych do innych węzłów
public class DataSender
{

    private List<IPEndPoint> IPs = [];

    public void AddIP(IPEndPoint ip)
    {
        IPs.Add(ip);
    }

    public async Task<Error?> ReceiveBlockchain(string data)
    {
        Console.WriteLine($"Próba wysłania danych: {data} do {IPs.Count} węzłów");

        foreach (var ip in IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using NetworkStream stream = client.GetStream();
            try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                await stream.WriteAsync(bytes, 0, bytes.Length);

                var buffer = new byte[16_384];
                int readed = await stream.ReadAsync(buffer);
                string result = Encoding.UTF8.GetString(buffer, 0, readed).TrimEnd();

                Console.WriteLine($"Odebrano odpowiedź: {result}");

                if (result.StartsWith("SEND_BLOCKCHAIN"))
                {
                    // Przetwarzaj otrzymany blockchain
                    string blockchainData = result.Substring(15);
                    await stream.WriteAsync(Encoding.UTF8.GetBytes("nice data"), 0, "nice data".Length);
                    Console.WriteLine($"Odebrano blockchain: {blockchainData}");
                    // Zapisz blockchain
                    File.WriteAllText("data.json", blockchainData);
                    return null;
                }

                else
                {
                    return new Error("Invalid response: " + result);
                }
            }
            catch (Exception e)
            {
                return new Error($"Nie udało się wysłać danych {e}");
            }
        }
        return null;
    }
    public async Task<Error?> SendData(IBlockchain blockchain)
    {
        var data = blockchain.GetBlockchain();
        data = "inasif\n";
        foreach (var ip in IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using NetworkStream stream = client.GetStream();
            try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                stream.Write(bytes, 0, bytes.Length);
                var buffer = new byte[1_024];
                int readed = await stream.ReadAsync(buffer);
                string result = Encoding.UTF8.GetString(buffer, 0, readed);
                result = result.TrimEnd();
                if (result != $"nice data")
                {
                    return new Error("bad server Response");
                }
            }
            catch (Exception e)
            {
                return new Error($"nie udało się wysłać danych {e}");
            }

        }
        return null;
    }
}