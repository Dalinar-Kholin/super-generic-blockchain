using blockProject.httpServer;
using blockProject.nodeCommunicatio;
using Newtonsoft.Json;

namespace blockProject.blockchain.genericBlockchain;



public class Blockchain
{
    private static Blockchain? _instance;
    private readonly IBlockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();
    private readonly Mutex _mutex = new();

    private readonly IValidator validator = new Validator();
    private List<BlockType> chain = new();


    // newest block to moduify when 3 records in it commited to blockchain
    public BlockType newestBlock = new();

    private Blockchain() { }

    // header tree
    private BlockNode _blockTree = new BlockNode();

    // dictionary by DataHash (because there may be several blocks with the same data)
    private Dictionary<string, List<BlockType>> _blockDict = new();



    
    // shortening the tree when possible (one of the branches is 5 blocks ahead of the others)
    private void DelateForks()
    {
        if (_blockTree._depth >= _blockTree._secondDepth + 5)
        {
            List<TreeHeader> hashes = _blockTree.GetPath(_blockTree._furthestNode); // returns a list of headers from the longest path in the tree

            _blockTree = new BlockNode(); // resetting the block tree
            for (int i = 0; i < hashes.Count; i++)
            {
                // we look for a block with DataHash hashes[i].DataHash in the dictionary and take the block with the appropriate hash
                foreach (var block in _blockDict[hashes[i].DataHash])
                {
                    if (block.header.Hash == hashes[i].Hash)
                    {
                        // a block with the appropriate hash was found
                        chain.Add(block);
                        // we remove all blocks from _blockDict with DataHash hashes[i].DataHash
                        _blockDict.Remove(hashes[i].DataHash);
                        break;
                    }
                }
            }

            // reszte DataHash z _blockDict trzeba dodac (jakoś uporządkowane) do kolejki (trzeba ja zaimplementowac) i stworzyć nowe bloki odrazu dodając je do chaina
            // TODO
        }
    }

    // adds block to the tree and dictionary
    public void AddBlockToTree(BlockType block)
    {
        _mutex.WaitOne();
        // creating a tree header from block header
        TreeHeader treeHeader = new TreeHeader
        {
            Hash = block.header.Hash,
            DataHash = block.header.DataHash,
            PreviousHash = block.header.PreviousHash
        };

        // if block with this DataHash already exists, we add it to the list of blocks with this DataHash
        if (!_blockDict.ContainsKey(treeHeader.DataHash))
        {
            _blockDict[treeHeader.DataHash] = new List<BlockType>();
        }
        _blockDict[treeHeader.DataHash].Add(block);

        _blockTree.AddChild(new BlockNode(treeHeader));
        DelateForks();
        _mutex.ReleaseMutex();
    }

    // return count of records in blockchain that let us know to create block or not 
    public BlockType? AddRecord(byte[] Record)
    {
        var rec = newestBlock.AddRecord(Record);

        if (rec >= 3)
        {
            var newBlock = newestBlock;

            // At first, there may be no blocks in the tree
            if (_blockTree._furthestNode == "")
            {
                newBlock.header.PreviousHash = chain.Count == 0 ? "0" : chain[chain.Count - 1].header.Hash;
            }
            // we take the last node from the longest path
            else
            {
                newBlock.header.PreviousHash = _blockTree._furthestNode;
            }

            newBlock.header.DataHash = validator.calcDataHash(newBlock);
            newBlock.header.Hash = validator.calcHash(newBlock);
            
            
            newBlock.header.miner = JsonKeyMaster.getServerPublicKey();
            
            // todo:
            // pierwszym rekordem danyhc powinien być rekord
            // przesyłający sumę opłat za wykopanie z konta 0x0 na konto kopacza
            AddBlockToTree(newestBlock);

            newestBlock = new BlockType();
            return newBlock;
        }

        return null;
    }

    public void CreateBlock(BlockType block)
    {
        _mutex.WaitOne();

        block.header.Hash = validator.calcHash(block);
        block.header.DataHash = validator.calcDataHash(block);

        //chain.Add(block);
        AddBlockToTree(newestBlock);

        _mutex.ReleaseMutex();
    }

    public void AddBlock(BlockType block)
    {
        _mutex.WaitOne();
        var err = validator.validate(block);
        if (err != null) // if not valid block we reject to include
        {
            Console.WriteLine($"{err.Message}");
            _mutex.ReleaseMutex();
            return;
        }

        //chain.Add(block);
        AddBlockToTree(newestBlock);

        _mutex.ReleaseMutex();
    }

    // method for testing purposes
    public List<BlockType> ExtractingBlocksFromTree()
    {
        _mutex.WaitOne();
        List<TreeHeader> hashes = _blockTree.GetPath(_blockTree._furthestNode);
        if (hashes.Count == 1 && hashes[0].Hash == "")
        {
            // if there is no blocks in tree
            _mutex.ReleaseMutex();
            return chain;
        }
        List<BlockType> blocks = new List<BlockType>();
        for (int i = hashes.Count - 1; i >= 0; i--)
        {
            // we look for a block with DataHash hashes[i].DataHash in the dictionary and take the block with the appropriate hash
            foreach (var block in _blockDict[hashes[i].DataHash])
            {
                if (block.header.Hash == hashes[i].Hash)
                {
                    // a block with the appropriate hash was found
                    blocks.Add(block);
                    break;
                }
            }
        }
        _mutex.ReleaseMutex();
        return chain.Union(blocks).ToList();
    }

    // returns list of blocks from chain + blocks from longest branch of the tree
    public List<BlockType> GetChain()
    {
        //return chain;
        return ExtractingBlocksFromTree();
    }

    public void SetChain(List<BlockType> blockchain)
    {
        chain = blockchain;
    }

    public string GetParsedBlockchain()
    {
        return JsonConvert.SerializeObject(chain);
    }

    public List<BlockType> GetBlockchain()
    {
        return chain;
    }

    public static Blockchain GetInstance()
    {
        return _instance ??= new Blockchain();
    }

    public static Blockchain GetTestInstance()
    {
        return new Blockchain();
    }

    // method is available only in test environment
    public static void Reset()
    {
        _instance = null;
    }
}

////klasa do obsługi blockchainu
//public class Blockchain : IBlockchain
//{

//    // private IValidate validator ?? pojawiennie się jakiejś klasy validatora?

//    private static Blockchain? _instance;
//    private Mutex _mut;
//    private NoneBlock _blockchain = new NoneBlock();
//    private Blockchain()
//    {
//        _mut = new Mutex();
//    }

//    public static Blockchain GetInstance()
//    {
//        return _instance ??= new Blockchain();
//    }

//