using blockProject.randomSrc;

namespace blockProject.nodeCommunicatio;


public interface IKeyMaster
{
    (string PublicKey, string privateKey, Error)? takeKeys(string uuid, string password);
    Error? storeKeys(string uuid, string password, string publicKey, string privateKey);
}

public class BlockKeyMaster : IKeyMaster{
    public (string PublicKey, string privateKey, Error)? takeKeys(string uuid, string password)
    {
        throw new NotImplementedException();
    }

    public Error? storeKeys(string uuid, string password, string publicKey, string privateKey)
    {
        throw new NotImplementedException();
    }
}