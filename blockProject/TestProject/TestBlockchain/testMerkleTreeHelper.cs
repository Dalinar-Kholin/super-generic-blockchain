using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Xunit;
using blockProject.blockchain;

namespace TestProject.TestBlockchain;

[Collection("SequentialTests")]
public class testMerkleTreeHelper
{
	[Fact]
	[Trait("cat", "merkle")]
	public void BuildMerkleRoot_WithEmptyList_ReturnsHashOfEmptyString()
	{
		var result = MerkleTreeHelper.BuildMerkleRoot(new List<byte[]>());
		var expected = Hash(Encoding.UTF8.GetBytes(""));
		Assert.Equal(expected, result);
	}

	[Fact]
	[Trait("cat", "merkle")]
	public void BuildMerkleRoot_WithSingleRecord_ReturnsHashedRecord()
	{
		var record = Encoding.UTF8.GetBytes("test");
		var result = MerkleTreeHelper.BuildMerkleRoot(new List<byte[]> { record });
		var expected = Hash(record);
		Assert.Equal(expected, result);
	}

	[Fact]
	[Trait("cat", "merkle")]
	public void BuildMerkleRoot_WithTwoRecords_ReturnsCorrectMerkleRoot()
	{
		var r1 = Encoding.UTF8.GetBytes("a");
		var r2 = Encoding.UTF8.GetBytes("b");

		var h1 = Hash(r1);
		var h2 = Hash(r2);
		var combined = Encoding.UTF8.GetBytes(h1 + h2);
		var expected = Hash(combined);

		var result = MerkleTreeHelper.BuildMerkleRoot(new List<byte[]> { r1, r2 });
		Assert.Equal(expected, result);
	}

	[Fact]
	[Trait("cat", "merkle")]
	public void BuildMerkleRoot_WithOddNumberOfRecords_PadsLast()
	{
		var r1 = Encoding.UTF8.GetBytes("r1");
		var r2 = Encoding.UTF8.GetBytes("r2");
		var r3 = Encoding.UTF8.GetBytes("r3");

		var h1 = Hash(r1);
		var h2 = Hash(r2);
		var h3 = Hash(r3);

		var l1 = Hash(Encoding.UTF8.GetBytes(h1 + h2));
		var l2 = Hash(Encoding.UTF8.GetBytes(h3 + h3)); // Padded

		var expected = Hash(Encoding.UTF8.GetBytes(l1 + l2));

		var result = MerkleTreeHelper.BuildMerkleRoot(new List<byte[]> { r1, r2, r3 });
		Assert.Equal(expected, result);
	}

	[Fact]
	[Trait("cat", "merkle")]
	public void BuildMerkleRoot_WithKnownData_ProducesStableOutput()
	{
		var records = new List<byte[]>
		{
			Encoding.UTF8.GetBytes("one"),
			Encoding.UTF8.GetBytes("two"),
			Encoding.UTF8.GetBytes("three"),
			Encoding.UTF8.GetBytes("four")
		};

		var root1 = MerkleTreeHelper.BuildMerkleRoot(records);
		var root2 = MerkleTreeHelper.BuildMerkleRoot(records);
		Assert.Equal(root1, root2); // Should be deterministic
	}

	// Mimic the same hashing logic as MerkleTreeHelper
	private static string Hash(byte[] data)
	{
		using var sha = SHA512.Create();
		var hash = sha.ComputeHash(data);
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}
}
