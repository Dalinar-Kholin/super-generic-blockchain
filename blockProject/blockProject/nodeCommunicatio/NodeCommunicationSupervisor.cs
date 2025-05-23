using System.Net;
using blockProject.randomSrc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace blockProject.nodeCommunicatio;





public class NodeCommunicationSupervisor
{
    private List<IPEndPoint> BlackListed = new();
    private DataSender _sender;
    private Dictionary<IPEndPoint, int> hashmap = new ();
    public NodeCommunicationSupervisor(DataSender sender)
    {
        _sender = sender;
    }


    private void doWork()
    {
        Task<(Error? err , IPEndPoint ip)>[] tasks = hashmap.Keys.Select(key => _sender.pingNode(key)).ToArray();
        Task.WaitAll(tasks);
        foreach (var t in tasks)
        {
            if (t.Result.err != null)
            {
                hashmap[t.Result.ip] += 1;
                if (hashmap[t.Result.ip] >= 5)
                {
                    _sender.getIpMaster().AddBlackList(t.Result.ip);
                }
            }
            else hashmap[t.Result.ip] = 0;
        }
    }

    public async void Start()
    {
        foreach (var x in _sender.getIpMaster().getParsed())
        {
            hashmap.Add(x,0);
        }

        while (true)
        {
            doWork();
            await Task.Delay(5_000);
        }
        
    }
    
}