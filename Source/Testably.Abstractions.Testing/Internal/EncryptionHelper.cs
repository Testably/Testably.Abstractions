using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Testably.Abstractions.Testing.Internal;

internal static class EncryptionHelper
{
	/// <summary>
	///     Encrypts the <paramref name="cypherBytes" /> with a fixed encryption algorithm.
	/// </summary>
	internal static byte[] Decrypt(byte[] cypherBytes)
	{
		using Aes algorithm = CreateAlgorithm();
		using ICryptoTransform decryptor = algorithm
		   .CreateDecryptor(algorithm.Key, algorithm.IV);

		return PerformCryptography(decryptor, cypherBytes);
	}

	/// <summary>
	///     Encrypts the <paramref name="plainBytes" /> with a fixed encryption algorithm.
	/// </summary>
	internal static byte[] Encrypt(byte[] plainBytes)
	{
		using Aes algorithm = CreateAlgorithm();
		using ICryptoTransform? encryptor = algorithm
		   .CreateEncryptor(algorithm.Key, algorithm.IV);

		return PerformCryptography(encryptor, plainBytes);
	}
	
	private static Aes CreateAlgorithm()
	{
		byte[] bytes = Encoding.UTF8.GetBytes(
			"THIS IS ONLY A DUMMY ENCRYPTION FOR TESTING HELPERS!");

		using (SHA256? sha256Hash = SHA256.Create())
		{
			byte[] key = sha256Hash.ComputeHash(bytes);
			byte[] iv = sha256Hash.ComputeHash(key).Take(16).ToArray();
			Aes algorithm = Aes.Create();
			algorithm.Key = key;
			algorithm.IV = iv;
			return algorithm;
		}
	}

	/// <summary>
	///     <see href="https://stackoverflow.com/a/24903689" />
	/// </summary>
	private static byte[] PerformCryptography(ICryptoTransform cryptoTransform,
	                                          byte[] data)
	{
		using (MemoryStream memoryStream = new())
		{
			using (CryptoStream cryptoStream = new(
				memoryStream,
				cryptoTransform,
				CryptoStreamMode.Write))
			{
				cryptoStream.Write(data, 0, data.Length);
				cryptoStream.FlushFinalBlock();
				return memoryStream.ToArray();
			}
		}
	}
}