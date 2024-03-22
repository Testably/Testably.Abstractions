using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class OpenReadTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.OpenRead(path);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void OpenRead_SetLength_ShouldThrowNotSupportedException(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			stream.SetLength(3);
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void OpenRead_ShouldUseReadAccessAndReadShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Read);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(
			Test.RunsOnWindows ? FileShare.Read : FileShare.ReadWrite);
		stream.CanRead.Should().BeTrue();
		stream.CanWrite.Should().BeFalse();
		stream.CanSeek.Should().BeTrue();
		stream.CanTimeout.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void OpenRead_Write_ShouldThrowNotSupportedException(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			stream.Write(bytes, 0, bytes.Length);
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public async Task OpenRead_WriteAsync_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		// ReSharper disable once MethodHasAsyncOverload
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			// ReSharper disable once UseAwaitUsing
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			#pragma warning disable CA1835
			await stream.WriteAsync(bytes, 0, bytes.Length);
			#pragma warning restore CA1835
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public async Task OpenRead_WriteAsyncWithMemory_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllTextAsync(path, null);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			await stream.WriteAsync(bytes.AsMemory());
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void OpenRead_WriteByte_ShouldThrowNotSupportedException(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			stream.WriteByte(0);
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void OpenRead_WriteWithSpan_ShouldThrowNotSupportedException(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			stream.Write(bytes.AsSpan());
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif
}
