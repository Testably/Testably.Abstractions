using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class OptionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Options_DeleteOnClose_ShouldDeleteFileOnClose(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.DeleteOnClose);
		FileSystem.Should().HaveFile(path);

		stream.Close();

		FileSystem.Should().NotHaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	public void Options_DeleteOnClose_ShouldDeleteFileOnDispose(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (FileSystemStream _ = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.DeleteOnClose))
		{
			// Delete on close
		}

		FileSystem.Should().NotHaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Options_Encrypt_ShouldKeepEncryptionFlag(
		string path, string contents1, string contents2)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents1);
		FileSystem.File.Encrypt(path);
		byte[] bytes = Encoding.Default.GetBytes(contents2);

		using (FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.Encrypted))
		{
			stream.Write(bytes, 0, bytes.Length);
			stream.SetLength(bytes.Length);
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents2)
			.And.HasAttribute(FileAttributes.Encrypted);
	}

	[SkippableTheory]
	[AutoData]
	public void Options_Encrypt_Unencrypted_ShouldBeIgnored(
		string path, string contents1, string contents2)
	{
		FileSystem.File.WriteAllText(path, contents1);
		byte[] bytes = Encoding.Default.GetBytes(contents2);

		using (FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.Encrypted))
		{
			stream.Write(bytes, 0, bytes.Length);
		}

		FileSystem.File.GetAttributes(path).Should()
			.NotHaveFlag(FileAttributes.Encrypted);
		FileSystem.File.ReadAllText(path).Should().Be(contents2);
	}

	[SkippableTheory]
	[AutoData]
	[SupportedOSPlatform("windows")]
	public void Options_EncryptedWithoutEncryptionOption_ShouldKeepEncryptionFlag(
		string path, string contents1, string contents2)
	{
		Skip.IfNot(Test.RunsOnWindows && FileSystem is MockFileSystem,
			"Encryption depends on the underlying device, if it is supported or not.");

		FileSystem.File.WriteAllText(path, contents1);
		FileSystem.File.Encrypt(path);
		byte[] bytes = Encoding.Default.GetBytes(contents2);

		using (FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10))
		{
			stream.Write(bytes, 0, bytes.Length);
			stream.SetLength(bytes.Length);
		}
		
		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents2)
			.And.HasAttribute(FileAttributes.Encrypted);
	}
}
