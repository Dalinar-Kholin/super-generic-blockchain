using System.Net;
using blockProject.blockchain;
using Microsoft.AspNetCore.Http;
using blockProject.randomSrc;
using System.Threading.Tasks;

namespace blockProject.nodeCommunicatio
{
    public class HttpMaster
    {
        private readonly DataSender sender;

        public HttpMaster(DataSender sender)
        {
            this.sender = sender;
        }

        public async Task AddNewNode(HttpContext context)
        {
            if (context.Request.Query.TryGetValue("port", out var port) &&
                context.Request.Query.TryGetValue("ip", out var ip))
            {
                string destPort = port.ToString();
                Console.WriteLine(ip.ToString());

                sender.AddIP(new IPEndPoint(IPAddress.Parse(ip.ToString()), int.Parse(destPort)));
                Console.WriteLine("dodano nowy węzeł");

                Error? errorResult = await sender.ReceiveBlockchain("GET_BLOCKCHAIN");
                if (errorResult != null)
                {
                    Console.WriteLine($"Błąd: {errorResult.Message}");
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = errorResult.Message
                    });
                    return;
                }

                await context.Response.WriteAsJsonAsync(new
                {
                    success = true,
                    message = "Node added and blockchain requested"
                });
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    error = "Missing port or ip parameters"
                });
            }
        }

        public async Task SendMessage(HttpContext context)
        {
            IBlockchain block = NonBlockChain.GetInstance();
            Error? res = await sender.SendData(block);
            Console.WriteLine(res != null ? $"pojawił się błąd {res.Message}" : "success");

            var result = new
            {
                success = res == null,
                message = res != null ? $"pojawił się błąd {res.Message}" : "success"
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}