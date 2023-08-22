using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Testably.Abstractions.Testing.Helpers;

internal static class EncryptionHelper
{
	private const string DummyEncryptionKey =
		"THIS IS ONLY A DUMMY ENCRYPTION FOR TESTING HELPERS!";

	/// <summary>
	///     Encrypts the <paramref name="cypherBytes" /> with a fixed encryption algorithm.
	/// </summary>
	internal static byte[] Decrypt(byte[] cypherBytes)
	{
		using Aes algorithm = CreateDummyEncryptionAlgorithm();
		using ICryptoTransform decryptor = algorithm
			.CreateDecryptor(algorithm.Key, algorithm.IV);

		return PerformCryptography(decryptor, cypherBytes);
	}

	/// <summary>
	///     Encrypts the <paramref name="plainBytes" /> with a fixed encryption algorithm.
	/// </summary>
	internal static byte[] Encrypt(byte[] plainBytes)
	{
		using Aes algorithm = CreateDummyEncryptionAlgorithm();
		using ICryptoTransform encryptor = algorithm
			.CreateEncryptor(algorithm.Key, algorithm.IV);

		return PerformCryptography(encryptor, plainBytes);
	}

	private static Aes CreateDummyEncryptionAlgorithm()
	{
		#pragma warning disable CA1850
		byte[] bytes = Encoding.UTF8.GetBytes(DummyEncryptionKey);
		using (SHA256 sha256Hash = SHA256.Create())
		{
			byte[] key = sha256Hash.ComputeHash(bytes);
			byte[] iv = sha256Hash.ComputeHash(key)
				.Skip(8).Take(16).ToArray();
			Aes algorithm = Aes.Create();
			algorithm.Key = key;
			algorithm.IV = iv;
			return algorithm;
		}
		#pragma warning restore CA1850
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
