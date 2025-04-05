using System.Net;
using System.Net.Sockets;
using System.Text;

namespace blockProject.nodeCommunicatio;


public class Listener
{
    public ICommunicationMaster Master;
    private int _port;

    public Listener(ICommunicationMaster master, int port)
    {
        _port = port;
        Master = master;
    }

    public async void Start()
    {
        var lis = new TcpListener(IPAddress.Any, _port);
        lis.Start(4096); // startujemy nasłuchiwanie z  max kolejką połączeń 4096
        while (true)
        {
            var client = await lis.AcceptTcpClientAsync(); // czekamy na klienta
            _ = Task.Run(async () =>
            {
                await using var stream = client.GetStream();

                while (true)
                { // jakoś tutaj powinna odbyć się obsługa komunikacji z klientem, odwołujemy się do Mastera
                    byte[] buffer = new byte[1_024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);


                    if (string.IsNullOrEmpty(receivedJson))
                    {
                        Console.WriteLine("połączenie zakończone\n");
                        break;
                    }

                    Console.WriteLine($"Odebrano: {receivedJson}");

                    if (receivedJson.StartsWith("GET_BLOCKCHAIN"))
                    {
                        var data = "SEND_BLOCKCHAIN" + Master.GetBlockchain();
                        byte[] responseBytes = Encoding.UTF8.GetBytes(data);
                        await stream.WriteAsync(responseBytes);
                        Console.WriteLine("Wysłano blockchain");
                    }
                }
            });
        }
    }
}