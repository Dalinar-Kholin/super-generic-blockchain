using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.nodeCommunicatio.communicationFrames;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace blockProject.nodeCommunicatio;

public class Listener
{
    private readonly IblockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();
    private readonly int _port;
    private readonly DataSender _sender = new();

    public Listener(int port)
    {
        _port = port;
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
                var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                    
                    
                    
                while (true)
                {
                    // jakoś tutaj powinna odbyć się obsługa komunikacji z klientem, odwołujemy się do Mastera
                    var buffer = new byte[1_024];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var receivedJson = JsonConvert.DeserializeObject<Frame>(body);

                    if (receivedJson == null)
                    {
                        Console.WriteLine("połączenie zakończone\n");
                        break;
                    }

                    string data;
                    byte[] responseBytes;
                    switch (receivedJson.Request)
                    {
                        // zmienić to potem na communication mastera
                        case Requests.GET_BLOCKCHAIN:
                            var res = new Frame(Requests.GET_BLOCKCHAIN,
                                JToken.FromObject(Blockchain.GetInstance().GetChain()));
                            data = JsonConvert.SerializeObject(res);
                            responseBytes = Encoding.UTF8.GetBytes(data);
                            await stream.WriteAsync(responseBytes);
                            // Console.WriteLine($"Wysłano blockchain {data}");
                            break;

                        case Requests.ADD_RECORD:

                            Console.WriteLine($"Otrzymano rekord: {receivedJson.data}");
                            var record = receivedJson.data.ToObject<messageRecord>();
                            // todo: tutaj powinna odbyć się walidacja rekordu, czy jest poprawny, oraz czy już nie występuje w naszej sieci
                            if (record != null)
                            {
                                var addedBlock = Blockchain.GetInstance().AddRecord(record);
                                if (addedBlock != null) _blockchainDataHandler.writeBlockc(addedBlock);

                                
                                await _sender.SendData(record, endpoint);
                            }

                            break;

                        case Requests.ADD_BLOCK:
                            Console.WriteLine($"Otrzymano blok: {receivedJson.data}");
                            var block = receivedJson.data.ToObject<BlockType>()!;

                            // todo: tutaj powinna odbyć się walidacja rekordu, czy jest poprawny, oraz czy już nie występuje w naszej sieci
                            
                            // jakaś walidacja bloku + inne akcje gdyby blok był niepoprawny
                            // TODO: implementacja tej metody
                            // dodanie bloku do blockchaina
                            Blockchain.GetInstance().AddBlock(block);
                            //AddToBlockchain(block); // do zaimplementowania
                            _blockchainDataHandler.writeBlockc(block); // zapisz blockchain do pliku
                            // wysłanie potwierdzenia otrzymania bloku
                            var response = new { Request = Requests.ADD_BLOCK };
                            var jsonResponse = JsonConvert.SerializeObject(response);
                            Console.WriteLine($"Wysłano potwierdzenie otrzymania bloku: {jsonResponse}");
                            await stream.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));

                            // propagacja bloku do innych węzłów
                            _ = _sender.SendData(block, endpoint);
                            
                            break;
                        case Requests.CONNECTION_PING:
                            var result = new Frame(Requests.CONNECTION_PING, JToken.FromObject(""));
                            data = JsonConvert.SerializeObject(result);
                            responseBytes = Encoding.UTF8.GetBytes(data);
                            await stream.WriteAsync(responseBytes);
                            break;
                    }
                }
            });
        }
    }
}