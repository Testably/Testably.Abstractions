using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class WriteAllBytesTests
{
	[Theory]
	[AutoData]
	public async Task WriteAllBytes_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

		FileSystem.File.WriteAllBytes(path, bytes);

		await That(FileSystem.File.Exists(path)).IsTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytes_ShouldCreateFileWithBytes(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		await That(FileSystem.File.Exists(path)).IsTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytes_WhenBytesAreNull_ShouldThrowArgumentNullException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, null!);
		});

		exception.Should().BeException<ArgumentNullException>(paramName: "bytes");
	}

	[Theory]
	[AutoData]
	public async Task
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
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytes_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
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
	public async Task WriteAllBytes_Span_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

		FileSystem.File.WriteAllBytes(path, bytes.AsSpan());

		await That(FileSystem.File.Exists(path)).IsTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytes_Span_ShouldCreateFileWithBytes(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes.AsSpan());

		await That(FileSystem.File.Exists(path)).IsTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task
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
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytes_Span_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
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
