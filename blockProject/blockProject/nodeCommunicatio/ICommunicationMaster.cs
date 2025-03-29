namespace blockProject.nodeCommunicatio;



// interface obsługi komunikaci z innymi węzłami jako klientami
public interface ICommunicationMaster
{
    public void AddToBlockchain();
    public void GetNodes();
}