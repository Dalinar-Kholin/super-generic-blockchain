using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace blockProject.blockchain
{
	// TODO: add this to validator -> calcDataHash
	public static class MerkleTreeHelper
	{
		private static string Hash(byte[] data)
		{
			using var sha = SHA512.Create();
			var hash = sha.ComputeHash(data);
			return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
		}

		public static string BuildMerkleRoot(List<byte[]> records)
		{
			if (records.Count == 0)
			{
				return Hash(Encoding.UTF8.GetBytes("")); // empty tree
			}

			// hash each record
			var hashes = records.Select(Hash).ToList();

			while (hashes.Count > 1)
			{
				var newLevel = new List<string>();

				for (int i = 0; i < hashes.Count; i += 2)
				{
					if (i + 1 < hashes.Count)
					{
						var combined = Encoding.UTF8.GetBytes(hashes[i] + hashes[i + 1]);
						newLevel.Add(Hash(combined));
					}
					else
					{
						var combined = Encoding.UTF8.GetBytes(hashes[i] + hashes[i]);
						newLevel.Add(Hash(combined));
					}
				}

				hashes = newLevel;
			}

			return hashes[0]; 
		}
	}
}
