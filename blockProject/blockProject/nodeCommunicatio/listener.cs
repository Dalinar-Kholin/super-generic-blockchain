using System.Net;
using System.Net.Sockets;

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
                using var reader = new StreamReader(stream);
                await using var writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                while (true)
                { // jakoś tutaj powinna odbyć się obsługa komunikacji z klientem, odwołujemy się do Mastera
                    var message = await reader.ReadLineAsync();
                    
                    if (string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("połączenie zakończone");
                        break;
                    };
                    Console.WriteLine($"Odebrano: {message}");

                    await writer.WriteLineAsync("nice data");
                }
            });
        }
    }
}