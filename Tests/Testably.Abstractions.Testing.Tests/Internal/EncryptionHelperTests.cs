using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Tests.Internal;

public class EncryptionHelperTests
{
	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(EncryptionHelper))]
	public void Decrypt_ShouldBeDifferentThanBefore(byte[] bytes)
	{
		byte[] result = EncryptionHelper.Encrypt(bytes);

		result.Should().NotBeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(EncryptionHelper))]
	public void Encrypt_ShouldBeDifferentThanBefore(byte[] bytes)
	{
		byte[] result = EncryptionHelper.Encrypt(bytes);

		result.Should().NotBeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(EncryptionHelper))]
	public void Encrypt_ThenDecrypt_ShouldBeEquivalentToBefore(byte[] bytes)
	{
		byte[] encrypted = EncryptionHelper.Encrypt(bytes);
		byte[] result = EncryptionHelper.Decrypt(encrypted);

		result.Should().BeEquivalentTo(bytes);
	}
}