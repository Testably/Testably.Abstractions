using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class OptionsTests
{
	[Theory]
	[AutoData]
	public void Options_DeleteOnClose_ShouldDeleteFileOnClose(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.DeleteOnClose);
		FileSystem.File.Exists(path).Should().BeTrue();

		stream.Close();

		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
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

		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
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

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents2);
		FileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
	}

	[Theory]
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

	[Theory]
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

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents2);
		FileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
	}
}
