using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class EncryptDecryptTests
{
	[Theory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public async Task Decrypt_EncryptedData_ShouldReturnOriginalText(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Encrypt(path);
		FileSystem.File.Decrypt(path);

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public async Task Decrypt_UnencryptedData_ShouldReturnOriginalText(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Decrypt(path);

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Encrypt_Decrypt_ShouldChangeEncryptedFileAttribute(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Encrypt(path);
		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
		FileSystem.File.Decrypt(path);
		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
	}

	[Theory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public async Task Encrypt_ShouldChangeData(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystem.File.Encrypt(path);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		await That(result).IsNotEqualTo(bytes).InAnyOrder();
	}

	[Theory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public async Task Encrypt_Twice_ShouldIgnoreTheSecondTime(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Encrypt(path);
		FileSystem.File.Encrypt(path);

		FileSystem.File.Decrypt(path);
		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}
}
