using System.IO;
using System.Text;
using Testably.Abstractions.FileSystem;

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

		stream.Close();

		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Options_DeleteOnClose_ShouldDeleteFileOnDispose(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.DeleteOnClose);

		stream.Dispose();

		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Options_Encrypt_ShouldDeleteFileOnDispose(
		string path, string contents1, string contents2)
	{
		FileSystem.File.WriteAllText(path, contents1);
		FileSystem.File.Encrypt(path);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.Encrypted);
		var bytes = Encoding.Default.GetBytes(contents2);

		stream.Write(bytes, 0, bytes.Length);
		stream.Dispose();

		FileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
		FileSystem.File.ReadAllText(path).Should().NotBe(contents2);
		FileSystem.File.Decrypt(path);
		FileSystem.File.ReadAllText(path).Should().Be(contents2);
	}
}