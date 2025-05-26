namespace blockProject.blockchain.genericBlockchain;

public struct TreeHeader
{
    public string Hash { get; set; }
    public string DataHash { get; set; }
    public string PreviousHash { get; set; }
}

public class BlockNode
{
    public TreeHeader _header;
    public List<BlockNode> _childrens { get; set; } = new List<BlockNode>();
    public string _furthestNode;
    public int _depth;
    public string _secondFurthestNode;
    public int _secondDepth;

    public BlockNode()
    {
        _header = new TreeHeader
        {
            Hash = "",
            DataHash = "",
            PreviousHash = ""
        };
        _furthestNode = "";
        _secondFurthestNode = "";
        _depth = 0;
        _secondDepth = -1;
    }

    public BlockNode(TreeHeader header)
    {
        _header = header;
        _furthestNode = header.Hash;
        _secondFurthestNode = "";
        _depth = 0;
        _secondDepth = -1;
    }

    public List<TreeHeader> GetPath(string Hash)
    {
        if (_header.Hash == Hash)
        {
            List<TreeHeader> path = new List<TreeHeader>();
            path.Add(_header);
            return path;
        }
        foreach (var child in _childrens)
        {
            var path = child.GetPath(Hash);
            if (path.Count > 0)
            {
                path.Add(_header);
                return path;
            }
        }
        return new List<TreeHeader>();
    }

    // adding header to appropriate place + depth update
    public (int, string, int, string) AddChild(BlockNode child) //returns depth of the tree and furthest and second furthest nodes
    {
        // if we have empty tree, we add the first child
        if (_furthestNode == "")
        {
            _furthestNode = child._header.Hash;
            _header = child._header;

            Console.WriteLine($"{_header.Hash}: {_depth}, {_furthestNode},{_secondDepth}, {_secondFurthestNode}");
            return (_depth, _furthestNode, _secondDepth, _secondFurthestNode);
        }

        // if we find a place for the header
        if (child._header.PreviousHash == _header.Hash)
        {
            // if the node had no children then we update its depth and farthest node
            if (_childrens.Count == 0)
            {
                _furthestNode = child._header.Hash;
                _depth = 1;
            }
            // if a node had only 1 child, we update its second depth and second deepest node
            else if (_childrens.Count == 1 && _secondDepth == -1)
            {
                _secondFurthestNode = child._header.Hash;
                _secondDepth = 1;
            }
            _childrens.Add(child);

            Console.WriteLine($"{_header.Hash}: {_depth}, {_furthestNode},{_secondDepth}, {_secondFurthestNode}");
            return (_depth, _furthestNode, _secondDepth, _secondFurthestNode);
        }

        // reaching the end of the tree
        if (_childrens.Count == 0)
        {
            Console.WriteLine($"{_header.Hash}: {_depth}, {_furthestNode},{_secondDepth}, {_secondFurthestNode}");
            return (_depth, _furthestNode, _secondDepth, _secondFurthestNode);
        }

        int max = -1;
        int secondMax = -1;
        string furthestChild = "";
        string secondFurthestChild = "";

        // recursive call on children and find max and second max (but taking into account that both values ​​cannot be contained in each other)
        foreach (var existingChild in _childrens)
        {
            var val = existingChild.AddChild(child);
            if (max < val.Item1)
            {
                // if we have a single path without forks, we do not add secondMax
                if (max != -1)
                {
                    secondMax = max;
                    secondFurthestChild = furthestChild;
                }
                max = val.Item1;
                furthestChild = val.Item2;
            }
            else if (val.Item1 > secondMax)
            {
                secondMax = val.Item1;
                secondFurthestChild = val.Item2;
            }
            if (secondMax < val.Item3)
            {
                secondMax = val.Item3;
                secondFurthestChild = val.Item4;
            }

        }

        max += 1;
        if (secondMax > -1)
        {
            secondMax += 1;
        }

        if (max >= _depth)
        {
            _furthestNode = furthestChild;
            _depth = max;
        }
        else if (max >= _secondDepth)
        {
            _secondFurthestNode = furthestChild;
            _secondDepth = max;
        }
        if (secondMax >= _secondDepth)
        {
            _secondFurthestNode = secondFurthestChild;
            _secondDepth = secondMax;
        }
        Console.WriteLine($"{_header.Hash}: {_depth}, {_furthestNode}, {_secondDepth}, {_secondFurthestNode}");
        return (_depth, _furthestNode, _secondDepth, _secondFurthestNode);
    }
};


/*
// testy

var root = new BlockNode();
var child0 = new BlockNode(new TreeHeader
{
    Hash = "0",
    PreviousHash = "999"
});
var child1 = new BlockNode(new TreeHeader
{
    Hash = "1",
    PreviousHash = "0"
});
var child2 = new BlockNode(new TreeHeader
{
    Hash = "2",
    PreviousHash = "0"
});
var child3 = new BlockNode(new TreeHeader
{
    Hash = "3",
    PreviousHash = "0"
});
var child4 = new BlockNode(new TreeHeader
{
    Hash = "4",
    PreviousHash = "0"
});
var child5 = new BlockNode(new TreeHeader
{
    Hash = "5",
    PreviousHash = "1"
});
var child6 = new BlockNode(new TreeHeader
{
    Hash = "6",
    PreviousHash = "1"
});
var child7 = new BlockNode(new TreeHeader
{
    Hash = "7",
    PreviousHash = "1"
});
var child8 = new BlockNode(new TreeHeader
{
    Hash = "8",
    PreviousHash = "2"
});
var child9 = new BlockNode(new TreeHeader
{
    Hash = "9",
    PreviousHash = "4"
});
var child10 = new BlockNode(new TreeHeader
{
    Hash = "10",
    PreviousHash = "4"
});
var child11 = new BlockNode(new TreeHeader
{
    Hash = "11",
    PreviousHash = "4"
});
var child12 = new BlockNode(new TreeHeader
{
    Hash = "12",
    PreviousHash = "4"
});
var child13 = new BlockNode(new TreeHeader
{
    Hash = "13",
    PreviousHash = "5"
});
var child14 = new BlockNode(new TreeHeader
{
    Hash = "14",
    PreviousHash = "6"
});
var child15 = new BlockNode(new TreeHeader
{
    Hash = "15",
    PreviousHash = "8"
});
var child16 = new BlockNode(new TreeHeader
{
    Hash = "16",
    PreviousHash = "8"
});
var child17 = new BlockNode(new TreeHeader
{
    Hash = "17",
    PreviousHash = "10"
});
var child18 = new BlockNode(new TreeHeader
{
    Hash = "18",
    PreviousHash = "10"
});
var child19 = new BlockNode(new TreeHeader
{
    Hash = "19",
    PreviousHash = "12"
});
var child20 = new BlockNode(new TreeHeader
{
    Hash = "20",
    PreviousHash = "14"
});
var child21 = new BlockNode(new TreeHeader
{
    Hash = "21",
    PreviousHash = "14"
});
var child22 = new BlockNode(new TreeHeader
{
    Hash = "22",
    PreviousHash = "17"
});
var child23 = new BlockNode(new TreeHeader
{
    Hash = "23",
    PreviousHash = "19"
});
var child24 = new BlockNode(new TreeHeader
{
    Hash = "24",
    PreviousHash = "22"
});
var child25 = new BlockNode(new TreeHeader
{
    Hash = "25",
    PreviousHash = "24"
});
var child26 = new BlockNode(new TreeHeader
{
    Hash = "26",
    PreviousHash = "25"
});
var child27 = new BlockNode(new TreeHeader
{
    Hash = "27",
    PreviousHash = "26"
});
var child28 = new BlockNode(new TreeHeader
{
    Hash = "28",
    PreviousHash = "27"
});

var child29 = new BlockNode(new TreeHeader
{
    Hash = "29",
    PreviousHash = "6"
});

var child30 = new BlockNode(new TreeHeader
{
    Hash = "30",
    PreviousHash = "29"
});
var child31 = new BlockNode(new TreeHeader
{
    Hash = "31",
    PreviousHash = "30"
});






root.AddChild(child0);
root.AddChild(child1);
root.AddChild(child2);

root.AddChild(child4);
root.AddChild(child5);
root.AddChild(child6);
root.AddChild(child7);
root.AddChild(child8);



root.AddChild(child10);
root.AddChild(child11);
root.AddChild(child12);
root.AddChild(child13);
root.AddChild(child14);

root.AddChild(child15);

root.AddChild(child17);
root.AddChild(child18);

root.AddChild(child20);


root.AddChild(child22);


Console.WriteLine("//////////////////////////////////////////");
root.AddChild(child24);
Console.WriteLine("//////////////////////////////////////////");
root.AddChild(child25);
Console.WriteLine("//////////////////////////////////////////");
root.AddChild(child26);
root.AddChild(child27);
root.AddChild(child28);
Console.WriteLine("//////////////////////////////////////////");
root.AddChild(child29);
root.AddChild(child19);
root.AddChild(child23);
root.AddChild(child16);
root.AddChild(child3);
root.AddChild(child21);

root.AddChild(child9);

root.AddChild(child30);
Console.WriteLine("//////////////////////////////////////////");
root.AddChild(child31);




Console.WriteLine($"Root Depth: {root._depth}");
Console.WriteLine($"Furthest Node: {root._furthestNode} at depth {root._depth}");
Console.WriteLine($"Second Furthest Node: {root._secondFurthestNode} at depth {root._secondDepth}");
foreach (var item in root.GetPath("31"))
{
    Console.WriteLine(item.Hash);
}
*/