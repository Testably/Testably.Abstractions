using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class EncryptDecryptTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Decrypt_EncryptedData_ShouldReturnOriginalText(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();
		sut.Decrypt();

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Decrypt_UnencryptedData_ShouldReturnOriginalText(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, contents);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Decrypt();

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Encrypt_Decrypt_ShouldChangeEncryptedFileAttribute(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();
		sut.Attributes.Should().HaveFlag(FileAttributes.Encrypted);
		sut.Decrypt();
		sut.Attributes.Should().NotHaveFlag(FileAttributes.Encrypted);
	}

	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Encrypt_ShouldChangeData(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllBytes(path, bytes);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Encrypt();

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().NotBeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Encrypt_Twice_ShouldIgnoreTheSecondTime(
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
		result.Should().Be(contents);
	}
}
