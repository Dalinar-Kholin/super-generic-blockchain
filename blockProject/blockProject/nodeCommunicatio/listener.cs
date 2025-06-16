using System.Net;
using System.Net.Sockets;
using System.Text;
using blockProject.blockchain;
using blockProject.blockchain.genericBlockchain;
using blockProject.nodeCommunicatio.communicationFrames;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace blockProject.nodeCommunicatio;

public class Listener
{
    private readonly IBlockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();
    private readonly int _port;
    private readonly DataSender _sender = new();
    private TcpListener lis;

    public Listener(int port)
    {
        _port = port;
    }

    public void Abort()
    {
        // casue a exception in doWork function
        lis.Stop();
    }


    public async void Start()
    {
        try
        {
            await doWork();
        }
        catch(Exception ex)
        {
            Console.WriteLine("aborted");
            //ignore
        }
    }
    
    
    public async Task doWork()
    {
        lis = new TcpListener(IPAddress.Any, _port);
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
                    var buffer = new byte[50];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var receivedJson = JsonConvert.DeserializeObject<Frame>(body);

                    if (receivedJson == null)
                    {
                        break;
                    }

                    switch (receivedJson.Request)
                    {
                        // zmienić to potem na communication mastera
                        case Requests.GET_BLOCKCHAIN:

                            var secondToken = 
                                Encoding.UTF8.GetBytes(
                                JsonConvert.SerializeObject(
                                    new SecondFrame(JToken.FromObject(
                                        Blockchain.GetInstance()
                                            .GetChain()))));
                            
                            var res = new Frame(Requests.GET_BLOCKCHAIN,secondToken.Length);
                            var BlockhcinData = JsonConvert.SerializeObject(res);
                            await stream.WriteAsync(Encoding.UTF8.GetBytes(BlockhcinData));
                            var blockchainBuffer =
                                new byte[50];

                            await stream.ReadAsync(blockchainBuffer);
                            
                            await stream.WriteAsync(secondToken);
                            break;
                        
                        case Requests.ADD_RECORD:

                            Console.WriteLine($"Otrzymano rekord o długości: {receivedJson.len}");
                            var frame = new Frame(Requests.ADD_RECORD, 0);
                            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(frame));
                            await stream.WriteAsync(bytes);
                            var addRecordBuffer = new byte[receivedJson.len];
                            var readed = Encoding.UTF8.GetString(addRecordBuffer, 0, await stream.ReadAsync(addRecordBuffer));
                            
                            var record = JsonConvert.DeserializeObject<SecondFrame>(readed).data.ToObject<messageRecord>();
                            
                            if (record != null)
                            {
                                var err = record.validate(record.from);
                                // todo: check is this record in blockchain
                                if (err != null)
                                {
                                    Console.WriteLine("bad signature\n");
                                    break;
                                }
                                
                                var addedBlock = Blockchain.GetInstance().AddRecord(record.toByte());
                                if (addedBlock != null)
                                {
                                    Blockchain.GetInstance().AddBlock(addedBlock);
                                    _blockchainDataHandler.writeBlock(addedBlock);
                                    await _sender.SendData(addedBlock, endpoint);
                                }
                                else
                                {
                                    await _sender.SendData(record, endpoint);
                                }
                            }

                            break;
                        
                        case Requests.ADD_BLOCK:
                            Console.WriteLine($"Otrzymano blok długości: {receivedJson.len}");
                            
                            Console.WriteLine($"Otrzymano rekord o długości: {receivedJson.len}");
                            var addBlockFrame = new Frame(Requests.ADD_RECORD, 0);
                            
                            await stream.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(addBlockFrame)));
                            
                            var addBlockBuffer = new byte[receivedJson.len];
                            var addBlockReaded = Encoding.UTF8.GetString(addBlockBuffer, 0, await stream.ReadAsync(addBlockBuffer));
                            var block = JsonConvert.DeserializeObject<SecondFrame>(addBlockReaded).data.ToObject<BlockType>()!;

                            // todo: tutaj powinna odbyć się walidacja rekordu, czy jest poprawny, oraz czy już nie występuje w naszej sieci

                            // jakaś walidacja bloku + inne akcje gdyby blok był niepoprawny
                            // TODO: implementacja tej metody
                            // dodanie bloku do blockchaina
                            Blockchain.GetInstance().AddBlock(block);
                            //AddToBlockchain(block); // do zaimplementowania
                            _blockchainDataHandler.writeBlock(block); // zapisz blockchain do pliku
                            // wysłanie potwierdzenia otrzymania bloku
                            var response = new { Request = Requests.ADD_BLOCK };
                            var jsonResponse = JsonConvert.SerializeObject(response);
                            Console.WriteLine($"Wysłano potwierdzenie otrzymania bloku: {jsonResponse}");
                            await stream.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));

                            // propagacja bloku do innych węzłów
                            _ = _sender.SendData(block, endpoint);

                            break;
                        
                        case Requests.CONNECTION_PING:
                            var result = new Frame(Requests.CONNECTION_PING, 0);
                            var connectionData = JsonConvert.SerializeObject(result);
                            await stream.WriteAsync(Encoding.UTF8.GetBytes(connectionData));
                            break;
                    }
                }
            });
        }
    }
}