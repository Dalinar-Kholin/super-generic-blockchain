using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.randomSrc;

namespace blockProject.nodeCommunicatio;




// klasa do wysyłania danych do innych węzłów
public class DataSender
{
    private List<IPEndPoint> IPs = new List<IPEndPoint>();
    

    public void AddIP(IPEndPoint ip)
    {
        IPs.Add(ip);
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
                if ( result != $"nice data")
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