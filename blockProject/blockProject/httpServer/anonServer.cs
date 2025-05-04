using System.Text;
using blockProject.blockchain;
using Microsoft.AspNetCore.Http;

namespace blockProject.httpServer;




public class anonServer
{
    public async Task getMessages(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var res = Blockchain.GetInstance().GetChain().Aggregate(
            new List<simpleMessage>(), (accumulate, block) =>
            {
                block.Records.Aggregate(new List<simpleMessage>(), (_, r) =>
                {
                    if (r.to == "0x0") accumulate.Add(r.decrypt(new Keys([], [])));
                    return null!;
                });
                return accumulate;
            }
        );

        
        
        
        await context.Response.WriteAsJsonAsync(new
        {
            success = true,
            result = res
        });

    } 
}