using blockProject.blockchain;
using blockProject.nodeCommunicatio;
using blockProject.randomSrc;

namespace TestProject.TestBlockchain;

[Collection("SequentialTests")]
public class testValidator
{
    private readonly IValidator _validator = new Validator();

    [Fact]
    [Trait("cat", "validator")]
    public void validatorTest()
    {
        Blockchain.Reset();
        var dh = singleFileBlockchainDataHandler.GetTestInstance();
        dh._filePath = "../../../data.json";
        var (storedChain, error) = dh.readBlockchain();
        if (error != null)
        {
            // jeżeli nie udało się załadować blockchainu spadamy z rowerka
            Console.WriteLine($"nie udało się załadować blockchainu z powodu {error.Message}");
            Environment.Exit(1);
        }

        Blockchain.GetInstance().SetChain(storedChain);

        Assert.Null(goodBlock());
        Assert.NotNull(badDataHash());
        Assert.NotNull(badHashBlock());
        Assert.NotNull(toMuchRecordInBlock());
        Assert.NotNull(blockAlreadyInBlockchain());
        // todo: kiedy dodamy jakieś sensowne dane w rekordach to przetestować czy są odpowiednio sprawdzane
        Blockchain.Reset();
    }

    private Error? goodBlock()
    {
        var blk = new Block(Blockchain.GetInstance().GetChain()[1].PreviousHash);
        for (var i = 0; i < Random.Shared.Next() % 3; i++)
            blk.AddRecord(testHelper.getRandomDummyRecord().toByte());


        blk.DataHash = _validator.calcDataHash(blk);
        blk.Hash = _validator.calcHash(blk);
        return _validator.validate(blk);
    }

    private Error? badHashBlock()
    {
        var blk = new Block(Blockchain.GetInstance().GetChain()[0].PreviousHash);
        for (var i = 0; i < Random.Shared.Next() % 3; i++)
            blk.AddRecord(testHelper.getRandomDummyRecord().toByte());

        blk.DataHash = _validator.calcDataHash(blk);
        blk.Hash = "skratada";
        return _validator.validate(blk);
    }

    private Error? badDataHash()
    {
        var blk = new Block(Blockchain.GetInstance().GetChain()[0].PreviousHash);
        for (var i = 0; i < Random.Shared.Next() % 3; i++)
            blk.AddRecord(testHelper.getRandomDummyRecord().toByte());

        blk.DataHash = _validator.calcDataHash(blk);
        blk.Hash = "frlateda";
        return _validator.validate(blk);
    }

    private Error? toMuchRecordInBlock()
    {
        var blk = new Block(Blockchain.GetInstance().GetChain()[0].PreviousHash);
        for (var i = 0; i < Random.Shared.Next() % 3 + 4; i++)
            blk.AddRecord(testHelper.getRandomDummyRecord().toByte());

        blk.DataHash = _validator.calcDataHash(blk);
        blk.Hash = _validator.calcHash(blk);
        return _validator.validate(blk);
    }

    private Error? blockAlreadyInBlockchain()
    {
        return _validator.validate(Blockchain.GetInstance().GetChain()[0]);
    }
}