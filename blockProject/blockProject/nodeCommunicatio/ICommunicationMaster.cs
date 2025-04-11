namespace blockProject.nodeCommunicatio;

// interface obsługi komunikaci z innymi węzłami jako klientami
public interface ICommunicationMaster
{
    public void AddToBlockchain(string data);
    public void GetNodes();
    public List<BlockType> GetBlockchain();
}