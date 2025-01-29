using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class WriteAllBytesTests
{
	[Theory]
	[AutoData]
	public void WriteAllBytes_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public void WriteAllBytes_ShouldCreateFileWithBytes(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public void WriteAllBytes_WhenBytesAreNull_ShouldThrowArgumentNullException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, null!);
		});

		exception.Should().BeException<ArgumentNullException>(paramName: "bytes");
	}

	[Theory]
	[AutoData]
	public void
		WriteAllBytes_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, bytes);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[Theory]
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

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
	
#if FEATURE_FILE_SPAN
	[Theory]
	[AutoData]
	public void WriteAllBytes_Span_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

		FileSystem.File.WriteAllBytes(path, bytes.AsSpan());

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public void WriteAllBytes_Span_ShouldCreateFileWithBytes(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes.AsSpan());

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public void
		WriteAllBytes_Span_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, bytes.AsSpan());
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[Theory]
	[AutoData]
	public void WriteAllBytes_Span_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllBytes(path, Array.Empty<byte>());
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, bytes.AsSpan());
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
#endif
}
