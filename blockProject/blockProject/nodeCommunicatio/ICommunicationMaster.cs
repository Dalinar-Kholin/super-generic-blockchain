namespace blockProject.nodeCommunicatio;

// interface obsługi komunikaci z innymi węzłami jako klientami
public interface ICommunicationMaster
{
    public void AddToBlockchain(BlockType block);
    public void GetNodes();
    public List<BlockType> GetBlockchain();

    public void SendFurther(BlockType block);
}