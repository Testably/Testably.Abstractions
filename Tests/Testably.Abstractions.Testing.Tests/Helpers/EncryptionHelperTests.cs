using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class EncryptionHelperTests
{
	[Theory]
	[AutoData]
	public void Decrypt_ShouldBeDifferentThanBefore(byte[] bytes)
	{
		byte[] result = EncryptionHelper.Encrypt(bytes);

		result.Should().NotBeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void Encrypt_ShouldBeDifferentThanBefore(byte[] bytes)
	{
		byte[] result = EncryptionHelper.Encrypt(bytes);

		result.Should().NotBeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void Encrypt_ThenDecrypt_ShouldBeEquivalentToBefore(byte[] bytes)
	{
		byte[] encrypted = EncryptionHelper.Encrypt(bytes);
		byte[] result = EncryptionHelper.Decrypt(encrypted);

		result.Should().BeEquivalentTo(bytes);
	}
}