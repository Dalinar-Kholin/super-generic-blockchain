using System.Threading.Tasks;
using blockProject.httpServer;
using blockProject.nodeCommunicatio;
using Newtonsoft.Json;

namespace blockProject.blockchain.genericBlockchain;

public class Blockchain
{
    private static Blockchain? _instance;
    private readonly IBlockchainDataHandler _blockchainDataHandler = singleFileBlockchainDataHandler.GetInstance();
    private readonly Mutex _mutex = new();

    private int _minedBlocks = 0;

    public int GetMinedBlockCount()
    {
        return _minedBlocks;
    }

    private int _receivedBlocks = 0;
    private const double DifferenceThresholdPercent = 20.0;


    private readonly IValidator validator = new Validator();
    private List<BlockType> chain = new();


    // newest block to moduify when 3 records in it commited to blockchain
    public BlockType newestBlock = new();

    private Blockchain() { }

    // header tree
    public BlockNode _blockTree = new BlockNode();

    // dictionary by DataHash (because there may be several blocks with the same data)
    private Dictionary<string, List<BlockType>> _blockDict = new();

    private DataSender _sender;


    public async Task HandleWastedBlocks(BlockType rootBlock)
    {
        var rejectedRecords = ExtractRecords(); // extract and sort records from _blockDict
        AddBlockToTree(rootBlock);
        Console.WriteLine(rejectedRecords.Count);

        while (rejectedRecords.Count > 0)
        {
            var a = AddRecord(rejectedRecords[0]);
            rejectedRecords.RemoveAt(0);
            if (a != null)
            {
                // jezeli stworzylismy blok wysylamy go innym węzłom
                await _sender.SendData(a);
            }
        }
    }

    // wyodrębnienie oraz posortowanie rekordów z bloków odrzuconych czyli z _blockDict
    private List<byte[]> ExtractRecords()
    {
        List<byte[]> rejectedRecords = new List<byte[]>();

        // iterujemy po wszystkich blokach w _blockDict
        foreach (var blockList in _blockDict.Values)
        {
            foreach (var block in blockList)
            {
                // iterujemy po wszystkich rekordach w bloku
                for (int i = 0; i < block.header.recordsInBlock; i++)
                {
                    Span<byte> recordSpan = block.GetRecordSpan(i);
                    byte[] recordArray = recordSpan.ToArray();
                    // dodajemy rekordy do listy odrzuconych
                    rejectedRecords.Add(recordArray);
                }
            }
        }

        _blockDict.Clear();

        //rejectedRecords.Sort();
        return rejectedRecords;
    }


    // shortening the tree when possible (one of the branches is 5 blocks ahead of the others)
    private void DelateForks()
    {
        if (_blockTree._depth >= _blockTree._secondDepth + 5)
        {
            List<TreeHeader> hashes = _blockTree.GetPath(_blockTree._furthestNode); // returns a list of headers from the longest path in the tree

            _blockTree = new BlockNode(); // resetting the block tree
            var rootBlock = new BlockType(); // the last block that will be added to the tree
            for (int i = 0; i < hashes.Count; i++)
            {
                // we look for a block with DataHash hashes[i].DataHash in the dictionary and take the block with the appropriate hash
                foreach (var block in _blockDict[hashes[i].DataHash])
                {
                    // a block with the appropriate hash was found
                    if (block.header.Hash == hashes[i].Hash)
                    {
                        // we remove all blocks from _blockDict with DataHash hashes[i].DataHash
                        _blockDict.Remove(hashes[i].DataHash);

                        // adding last block back to _blockTree
                        if (i == hashes.Count - 1)
                        {
                            rootBlock = block;
                        }
                        else
                        {
                            chain.Add(block);
                        }
                        break;
                    }
                }
            }

            HandleWastedBlocks(rootBlock); // handle all records from _blockDict that were not added to the chain

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
            PreviousHash = block.header.PreviousHash,
            Nonce = block.header.Nonce
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
        // TODO poprawic czytanie blockchain z pliku bo zle cos wczytuje po dodaniu positionOfRecords
        /*
        if (IsRecordInBlockchain(Record))

        {

            // if record already exists in blockchain we do not add it
            //_mutex.ReleaseMutex();
            return null;
        }
    */

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

            _minedBlocks++;
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

        AddBlockToTree(block);

        _mutex.ReleaseMutex();
    }

    public void AddBlock(BlockType block)
    {
        _mutex.WaitOne();

        if (IsBlockInBlockchain(block))
        {
            // if block already exists in blockchain we do not add it
            _mutex.ReleaseMutex();
            return;
        }

        var err = validator.validate(block);
        if (err != null) // if not valid block we reject to include
        {
            Console.WriteLine($"{err.Message}");
            _mutex.ReleaseMutex();
            return;
        }

        _receivedBlocks++;

        AddBlockToTree(block);

        _mutex.ReleaseMutex();
    }

    // method for testing purposes
    public List<BlockType> ExtractBlocksFromTree()
    {
        _mutex.WaitOne();
        List<TreeHeader> hashes = _blockTree.GetPath(_blockTree._furthestNode);
        //Console.WriteLine(_blockTree._furthestNode);

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
    // nie wiem czy moze to tak być bo zwracamy możliwe rozwidlenie ale nie pewne (narazie jest to wprowadzone w celu spelnienia testow)
    public List<BlockType> GetChain()
    {
        //return chain;
        return ExtractBlocksFromTree();
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

    public void SetSender(DataSender sender)
    {
        this._sender = sender;
    }

    public void LogIfDataDifferenceExceedsThreshold()
    {
        _mutex.WaitOne();
        int localBlockCount = ExtractBlocksFromTree().Count;
        int localRecordCount = ExtractBlocksFromTree().Sum(b => b.body.Records.Count());
        _mutex.ReleaseMutex();

        if (localBlockCount == 0 || localRecordCount == 0)
        {
            Console.WriteLine("[INFO] Local blockchain is empty, skipping difference check.");
            return;
        }

        double blockDiff = Math.Abs(_minedBlocks - _receivedBlocks) / (double)(Math.Max(1, _minedBlocks + _receivedBlocks)) * 100.0;

        if (blockDiff >= DifferenceThresholdPercent)
        {
            Console.WriteLine($"[WARNING] Block mining/receiving difference exceeds" +
                $" {DifferenceThresholdPercent}%: {blockDiff:F2}% (mined: {_minedBlocks}, received: {_receivedBlocks})");
        }

    }


    public void ResetCounters()
    {
        _mutex.WaitOne();
        _receivedBlocks = 0;
        _minedBlocks = 0;
        _mutex.ReleaseMutex();
    }

    private bool IsBlockInBlockchain(BlockType block)
    {
        //_mutex.WaitOne();

        // sprawdzamy czy blok jest juz _blockTree
        if (_blockDict.ContainsKey(block.header.DataHash))
        {
            foreach (var b in _blockDict[block.header.DataHash])
            {
                if (b.header.Hash == block.header.Hash)
                {
                    //_mutex.ReleaseMutex();
                    return true;
                }
            }
        }

        // sprawdzamy czy blok jest w łańcuchu
        foreach (var b in chain)
        {
            if (b.header.Hash == block.header.Hash)
            {
                //_mutex.ReleaseMutex();
                return true;
            }
        }

        //_mutex.ReleaseMutex();
        return false;
    }

    // sprawdzenie czy rekord jest juz w blokach
    private bool IsRecordInBlockchain(byte[] record)
    {
        //_mutex.WaitOne();

        Span<byte> recordSpan = record.AsSpan();

        // sprawdzenei czy rekord jest w newestBlock
        if (newestBlock.header.recordsInBlock > 0)
        {

            for (int i = 0; i < newestBlock.header.recordsInBlock; i++)
            {

                if (recordSpan.SequenceEqual(newestBlock.GetRecordSpan(i)))
                {
                    //_mutex.ReleaseMutex();
                    Console.WriteLine("a1");
                    return true;
                }

            }
        }


        // iterujemy po wszystkich blokach w _blockDict
        foreach (var blockList in _blockDict.Values)
        {
            foreach (var block in blockList)
            {
                // iterujemy po wszystkich rekordach w bloku
                int recordCount = block.header.recordsInBlock;
                for (int i = 0; i < recordCount; i++)
                {

                    // sprawdzamy czy rekord jest taki sam jak ten w bloku
                    if (recordSpan.SequenceEqual(block.GetRecordSpan(i)))
                    {
                        Console.WriteLine("a2");
                        //_mutex.ReleaseMutex();
                        return true;
                    }


                }
            }
        }

        // sprawdzamy czy rekord jest w łańcuchu
        foreach (var block in chain)
        {

            for (int i = 0; i < block.header.recordsInBlock; i++)
            {

                // sprawdzamy czy rekord jest taki sam jak ten w bloku
                // TODO naprawic czytanie blockchain z pliku
                if (recordSpan.SequenceEqual(block.GetRecordSpan(i)))
                {
                    //_mutex.ReleaseMutex();
                    return true;
                }
            }
        }

        //_mutex.ReleaseMutex();
        Console.WriteLine("b1");
        return false;
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