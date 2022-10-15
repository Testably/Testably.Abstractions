#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]

#if NET6_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public void Decrypt_EncryptedData_ShouldReturnOriginalText(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is FileSystemMock,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Encrypt(path);
		FileSystem.File.Decrypt(path);

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]

#if NET6_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public void Decrypt_UnencryptedData_ShouldReturnOriginalText(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Decrypt(path);

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]

#if NET6_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public void Encrypt_ShouldChangeData(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is FileSystemMock,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystem.File.Encrypt(path);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().NotBeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]

#if NET6_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public void Encrypt_Twice_ShouldIgnoreTheSecondTime(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is FileSystemMock,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents);

		FileSystem.File.Encrypt(path);
		FileSystem.File.Encrypt(path);

		FileSystem.File.Decrypt(path);
		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}
}