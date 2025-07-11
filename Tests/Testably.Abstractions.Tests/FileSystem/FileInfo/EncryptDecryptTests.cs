using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

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
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();
		sut.Decrypt();

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
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Decrypt();

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public async Task Encrypt_Decrypt_ShouldChangeEncryptedFileAttribute(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();
		await That(sut.Attributes).HasFlag(FileAttributes.Encrypted);
		sut.Decrypt();
		await That(sut.Attributes).DoesNotHaveFlag(FileAttributes.Encrypted);
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
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();

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
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();
		sut.Encrypt();

		sut.Decrypt();
		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}
}
