using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteAllBytesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void WriteAllBytes_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

		FileSystem.File.WriteAllBytes(path, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllBytes_WhenBytesAreNull_ShouldThrowArgumentNullException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("bytes");
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllBytes_ShouldCreateFileWithBytes(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllBytes_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllBytes(path, Array.Empty<byte>());
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, bytes);
		});

		exception.Should().BeOfType<UnauthorizedAccessException>()
			.Which.HResult.Should().Be(-2147024891);
	}
}