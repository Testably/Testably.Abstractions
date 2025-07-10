using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class OpenReadTests
{
	[Theory]
	[AutoData]
	public async Task OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.OpenRead(path);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[AutoData]
	public async Task OpenRead_SetLength_ShouldThrowNotSupportedException(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			stream.SetLength(3);
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[Theory]
	[AutoData]
	public async Task OpenRead_ShouldUseReadAccessAndReadShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.Read);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(
			Test.RunsOnWindows ? FileShare.Read : FileShare.ReadWrite);
		await That(stream.CanRead).IsTrue();
		await That(stream.CanWrite).IsFalse();
		await That(stream.CanSeek).IsTrue();
		await That(stream.CanTimeout).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task OpenRead_Write_ShouldThrowNotSupportedException(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			stream.Write(bytes, 0, bytes.Length);
		});

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[Theory]
	[AutoData]
	public async Task OpenRead_WriteAsync_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		// ReSharper disable once MethodHasAsyncOverload
		FileSystem.File.WriteAllText(path, null);

		async Task Act()
		{
			// ReSharper disable once UseAwaitUsing
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			#pragma warning disable CA1835
			await stream.WriteAsync(bytes, 0, bytes.Length, TestContext.Current.CancellationToken);
			#pragma warning restore CA1835
		}
		
		Exception? exception = await Record.ExceptionAsync(Act);
		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task OpenRead_WriteAsyncWithMemory_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllTextAsync(path, "", TestContext.Current.CancellationToken);

		async Task Act()
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			await stream.WriteAsync(bytes.AsMemory(), TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

	[Theory]
	[AutoData]
	public async Task OpenRead_WriteByte_ShouldThrowNotSupportedException(string path)
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
	[Theory]
	[AutoData]
	public async Task OpenRead_WriteWithSpan_ShouldThrowNotSupportedException(string path, byte[] bytes)
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
