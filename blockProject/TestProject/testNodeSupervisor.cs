using System.Net;
using blockProject.nodeCommunicatio;

namespace TestProject;



public class testNodeSupervisor
{
    
    [Trait("cat", "ping")]
    [Fact]
    public async Task testPinging()
    {
        const int node1Port = 9999;
        const int node2Port = 8888;
        const string nodeIp = "127.0.0.1";

        var sender1 = new DataSender();
        var l1 = new Listener(node1Port);
        var t1 = new Thread(l1.Start);
        var t2 = new Thread(new Listener(node2Port).Start);
        t2.Start();
        t1.Start();
        
        sender1.getIpMaster().AddIP(new IPEndPoint(IPAddress.Parse(nodeIp), node1Port));
        sender1.getIpMaster().AddIP(new IPEndPoint(IPAddress.Parse(nodeIp), node2Port));
        new Thread(new NodeCommunicationSupervisor(sender1).Start).Start(); // menage communication with node

        await Task.Delay(3000);
        Assert.Equal(2, sender1.getIpMaster().IPs.Count);
        l1.Abort();
        await Task.Delay(4000);
        Assert.Single(sender1.getIpMaster().IPs);
        l1 = new Listener(node1Port);
        new Thread(l1.Start).Start();
        await Task.Delay(2000);
        Assert.Equal(2, sender1.getIpMaster().IPs.Count);
    }    
}