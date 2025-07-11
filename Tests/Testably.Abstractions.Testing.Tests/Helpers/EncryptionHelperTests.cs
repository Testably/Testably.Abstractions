using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class EncryptionHelperTests
{
	[Theory]
	[AutoData]
	public async Task Decrypt_ShouldBeDifferentThanBefore(byte[] bytes)
	{
		byte[] encryptedBytes = EncryptionHelper.Encrypt(bytes);

		byte[] result = EncryptionHelper.Decrypt(encryptedBytes);

		await That(result).IsNotEqualTo(encryptedBytes).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task Encrypt_ShouldBeDifferentThanBefore(byte[] bytes)
	{
		byte[] result = EncryptionHelper.Encrypt(bytes);

		await That(result).IsNotEqualTo(bytes).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task Encrypt_ThenDecrypt_ShouldBeEquivalentToBefore(byte[] bytes)
	{
		byte[] encrypted = EncryptionHelper.Encrypt(bytes);
		byte[] result = EncryptionHelper.Decrypt(encrypted);

		await That(result).IsEqualTo(bytes).InAnyOrder();
	}
}
