using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.AccessControl.Tests.Internal;

public sealed class AccessControlHelperTests
{
	[Theory]
	[AutoDomainData]
	public async Task GetExtensibilityOrThrow_CustomDirectoryInfo_ShouldThrowNotSupportedException(IDirectoryInfo sut)
	{
		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(IFileSystemExtensibility)).And
			.Contain(sut.GetType().Name);
	}

	[Theory]
	[AutoDomainData]
	public async Task GetExtensibilityOrThrow_CustomFileInfo_ShouldThrowNotSupportedException(IFileInfo sut)
	{
		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(IFileSystemExtensibility)).And
			.Contain(sut.GetType().Name);
	}

	[Fact]
	public async Task GetExtensibilityOrThrow_CustomFileSystemStream_ShouldThrowNotSupportedException()
	{
		FileSystemStream sut = new CustomFileSystemStream();

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(IFileSystemExtensibility)).And
			.Contain(sut.GetType().Name);
	}

	[Fact]
	public async Task ThrowIfMissing_ExistingDirectoryInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");
		fileSystem.Directory.CreateDirectory("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeNull();
	}

	[Fact]
	public async Task ThrowIfMissing_ExistingFileInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");
		fileSystem.File.WriteAllText("foo", "some content");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeNull();
	}

	[Fact]
	public async Task ThrowIfMissing_MissingDirectoryInfo_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.HResult.Should().Be(-2147024893);
		exception!.Message.Should().Contain($"'{sut.FullName}'");
	}

	[Fact]
	public async Task ThrowIfMissing_MissingFileInfo_ShouldThrowFileNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception!.Message.Should().Contain($"'{sut.FullName}'");
	}

	private sealed class CustomFileSystemStream() : FileSystemStream(Null, ".", false);
}
