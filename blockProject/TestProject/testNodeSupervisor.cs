using blockProject.nodeCommunicatio;

namespace TestProject;



public class testNodeSupervisor
{
    public void testPinging()
    {
        const int node1Port = 9999;
        const int node2Port = 8888;
        const string node2Ip = "127.0.0.1";

        var sender = new DataSender();
        new Thread(new Listener(node1Port).Start).Start();
        
        new Thread(new Listener(node2Port).Start).Start();
        
    }    
}