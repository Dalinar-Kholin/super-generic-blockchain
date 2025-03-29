using System.Net;
using System.Net.Sockets;
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

        foreach (var ip in IPs)
        {
            using TcpClient client = new();
            await client.ConnectAsync(ip);
            await using NetworkStream stream = client.GetStream();
            try
            {
                stream.WriteByte(Convert.ToByte(data));
                var buffer = new byte[1_024];
                _ = await stream.ReadAsync(buffer);
                if ( String.Compare(Convert.ToString(buffer), "nice data", StringComparison.Ordinal) != 0)
                {
                    return new Error("bad server Response");
                }
            }
            catch (Exception)
            {
                return new Error("nie udało się wysłać danych");
            }

        }
        return null;
    }
}